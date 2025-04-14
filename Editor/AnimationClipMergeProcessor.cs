using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Narazaka.Unity.MergedAnimationClip.Editor
{
    public class AnimationClipMergeProcessor : AssetPostprocessor
    {
        // Key: source clip GUID / Value: merged clips GUID
        static Dictionary<GUID, HashSet<GUID>> animationDependencies = null;

        static void OnPostprocessAnimation(GameObject gameObject, AnimationClip clip)
        {
            var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(clip));
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            var changed = false;
            foreach (var asset in importedAssets)
            {
                if (ProcessAsset(asset))
                {
                    changed = true;
                }
            }
            if (changed)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        static bool ProcessAsset(string path)
        {
            if (!path.EndsWith(".anim")) return false;
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null) return false;
            var changed = false;
            var mergedAnimationClip = GetMergedAnimationClip(path);
            if (mergedAnimationClip != null)
            {
                if (ProcessMergedClip(path, clip, mergedAnimationClip))
                {
                    changed = true;
                }
            }
            return ProcessClip(path, clip) || changed;
        }

        static bool ProcessClip(string clipPath, AnimationClip clip)
        {
            if (animationDependencies == null || animationDependencies.Count == 0) return false;
            var guid = AssetDatabase.GUIDFromAssetPath(clipPath);
            var changed = false;
            if (animationDependencies.TryGetValue(guid, out var dependencies))
            {
                foreach (var d in dependencies)
                {
                    if (MergeClip(d))
                    {
                        changed = true;
                    }
                }
            }
            return changed;
        }

        static bool ProcessMergedClip(string clipPath, AnimationClip clip, AnimationClipMergeSetting merged)
        {
            var guid = AssetDatabase.GUIDFromAssetPath(clipPath);
            if (animationDependencies == null)
            {
                animationDependencies = new Dictionary<GUID, HashSet<GUID>>();
            }
            var dependencyClips = merged.clips;
            if (dependencyClips == null)
            {
                dependencyClips = new AnimationClip[0];
            }
            // clean previous dependencies
            foreach (var source in animationDependencies.Keys)
            {
                animationDependencies[source].Remove(guid);
            }
            // add new dependencies
            foreach (var dependency in dependencyClips)
            {
                var dependencyPath = AssetDatabase.GetAssetPath(dependency);
                var dependencyGuid = AssetDatabase.GUIDFromAssetPath(dependencyPath);
                if (!animationDependencies.ContainsKey(dependencyGuid))
                {
                    animationDependencies[dependencyGuid] = new HashSet<GUID>();
                }
                animationDependencies[dependencyGuid].Add(guid);
            }
            // merge clips
            return MergeClip(guid);
        }

        internal static AnimationClipMergeSetting GetMergedAnimationClip(string path)
        {
            var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
            if (subAssets == null || subAssets.Length == 0) return null;
            var mergedAnimationClip = subAssets.FirstOrDefault(a => a is AnimationClipMergeSetting) as AnimationClipMergeSetting;
            if (mergedAnimationClip == null) return null;
            return mergedAnimationClip;
        }

        static bool MergeClip(GUID guid)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var targetClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (targetClip == null) return false;
            var mergedAnimationClip = GetMergedAnimationClip(path);
            if (mergedAnimationClip == null) return false;
            var dependencyClips = mergedAnimationClip.clips;
            if (dependencyClips == null) dependencyClips = new AnimationClip[0];
            var mergedClip = MergeClips(new AnimationClip(), dependencyClips);
            if (IsSameClipContent(targetClip, mergedClip)) return false;
            targetClip.ClearCurves();
            AnimationUtility.SetAnimationEvents(targetClip, new AnimationEvent[0]);
            MergeClips(targetClip, dependencyClips);
            EditorUtility.SetDirty(targetClip);
            return true;
        }

        static AnimationClip MergeClips(AnimationClip targetClip, AnimationClip[] dependencyClips)
        {
            foreach (var dependencyClip in dependencyClips)
            {
                foreach (var binding in AnimationUtility.GetCurveBindings(dependencyClip))
                {
                    var curve = AnimationUtility.GetEditorCurve(dependencyClip, binding);
                    targetClip.SetCurve(binding.path, binding.type, binding.propertyName, curve);
                }
                foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(dependencyClip))
                {
                    var curve = AnimationUtility.GetObjectReferenceCurve(dependencyClip, binding);
                    AnimationUtility.SetObjectReferenceCurve(targetClip, binding, curve);
                }
                foreach (var ev in AnimationUtility.GetAnimationEvents(dependencyClip))
                {
                    var newEvent = new AnimationEvent
                    {
                        functionName = ev.functionName,
                        time = ev.time,
                        objectReferenceParameter = ev.objectReferenceParameter,
                        stringParameter = ev.stringParameter,
                        floatParameter = ev.floatParameter,
                        intParameter = ev.intParameter,
                        messageOptions = ev.messageOptions,
                    };
                    targetClip.AddEvent(newEvent);
                }
            }
            return targetClip;
        }

        static bool IsSameClipContent(AnimationClip clip1, AnimationClip clip2)
        {
            var bindings1 = AnimationUtility.GetCurveBindings(clip1);
            var bindings2 = AnimationUtility.GetCurveBindings(clip2);
            if (bindings1.Length != bindings2.Length) return false;
            var bindingSet1 = new HashSet<EditorCurveBinding>(bindings1);
            var bindingSet2 = new HashSet<EditorCurveBinding>(bindings2);
            if (!bindingSet1.SetEquals(bindingSet2)) return false;
            foreach (var binding in bindings1)
            {
                var curve1 = AnimationUtility.GetEditorCurve(clip1, binding);
                var curve2 = AnimationUtility.GetEditorCurve(clip2, binding);
                if (!curve1.Equals(curve2)) return false;
            }

            var objectReferenceBindings1 = AnimationUtility.GetObjectReferenceCurveBindings(clip1);
            var objectReferenceBindings2 = AnimationUtility.GetObjectReferenceCurveBindings(clip2);
            if (objectReferenceBindings1.Length != objectReferenceBindings2.Length) return false;
            var objectReferenceBindingSet1 = new HashSet<EditorCurveBinding>(objectReferenceBindings1);
            var objectReferenceBindingSet2 = new HashSet<EditorCurveBinding>(objectReferenceBindings2);
            if (!objectReferenceBindingSet1.SetEquals(objectReferenceBindingSet2)) return false;
            foreach (var binding in objectReferenceBindings1)
            {
                var curve1 = AnimationUtility.GetObjectReferenceCurve(clip1, binding);
                var curve2 = AnimationUtility.GetObjectReferenceCurve(clip2, binding);
                if (!curve1.SequenceEqual(curve2)) return false;
            }

            var events1 = AnimationUtility.GetAnimationEvents(clip1);
            var events2 = AnimationUtility.GetAnimationEvents(clip2);
            if (events1.Length != events2.Length) return false;
            for (int i = 0; i < events1.Length; i++)
            {
                if (events1[i].functionName != events2[i].functionName ||
                    events1[i].time != events2[i].time ||
                    events1[i].objectReferenceParameter != events2[i].objectReferenceParameter ||
                    events1[i].stringParameter != events2[i].stringParameter ||
                    events1[i].floatParameter != events2[i].floatParameter ||
                    events1[i].intParameter != events2[i].intParameter ||
                    events1[i].messageOptions != events2[i].messageOptions)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
