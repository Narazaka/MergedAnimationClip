using System.IO;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Narazaka.Unity.MergedAnimationClip.Editor
{
    [CustomEditor(typeof(AnimationClipMergeSetting))]
    public class AnimationClipMergeSettingEditor : UnityEditor.Editor
    {
        [MenuItem("Assets/Create/Animation (Merged)", priority = 402)]
        public static void CreateAssetInstance()
        {
            var getActiveFolderPath = typeof(ProjectWindowUtil).GetMethod("GetActiveFolderPath", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var basePath = getActiveFolderPath.Invoke(null, new object[0]) as string;
            var path = AssetDatabase.GenerateUniqueAssetPath(Path.Join(basePath, "Merged Animation Clip.anim"));

            var clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, path);
            var mergedAnimationClip = CreateInstance<AnimationClipMergeSetting>();
            mergedAnimationClip.name = "Merge Setting";
            AssetDatabase.AddObjectToAsset(mergedAnimationClip, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = clip;
        }

        [MenuItem("Assets/[Convert] Animation to (Merged)", validate = false)]
        public static void ConvertAnimationToMerged()
        {
            var clips = Selection.GetFiltered<AnimationClip>(SelectionMode.Assets);
            if (clips.Length == 0)
            {
                return;
            }
            var paths = clips.Select(clip => AssetDatabase.GetAssetPath(clip)).Where(path => AnimationClipMergeProcessor.GetMergedAnimationClip(path) == null).ToArray();
            if (paths.Length == 0)
            {
                return;
            }
            if (!EditorUtility.DisplayDialog("Convert Animation to (Merged)", "**DANGER**\nAll selected clips will be cleared!!!\nAre you sure you want to continue?", "OK", "Cancel"))
            {
                return;
            }
            foreach (var path in paths)
            {
                var mergedAnimationClip = CreateInstance<AnimationClipMergeSetting>();
                mergedAnimationClip.name = "Merge Setting";
                AssetDatabase.AddObjectToAsset(mergedAnimationClip, path);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/[Convert] Animation to (Merged)", validate = true)]
        public static bool ValidateConvertAnimationToMerged()
        {
            var clips = Selection.GetFiltered<AnimationClip>(SelectionMode.Assets);
            if (clips.Length == 0)
            {
                return false;
            }
            var paths = clips.Select(clip => AssetDatabase.GetAssetPath(clip)).Where(path => AnimationClipMergeProcessor.GetMergedAnimationClip(path) == null).ToArray();
            if (paths.Length == 0)
            {
                return false;
            }
            return true;
        }

        bool foldout;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            foldout = EditorGUILayout.Foldout(foldout, "Advanced");
            if (foldout && GUILayout.Button("Delete setting (convert to normal AnimationClip)") && EditorUtility.DisplayDialog("Delete setting", "Are you sure you want to continue?", "OK", "Cancel"))
            {
                Undo.DestroyObjectImmediate(target);
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
        }
    }
}
