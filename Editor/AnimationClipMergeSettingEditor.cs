using System.IO;
using UnityEngine;
using UnityEditor;

namespace Narazaka.Unity.MergedAnimationClip.Editor
{
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
    }
}
