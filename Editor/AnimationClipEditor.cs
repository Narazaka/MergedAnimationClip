using UnityEngine;
using UnityEditor;

namespace Narazaka.Unity.MergedAnimationClip.Editor
{
    [CustomEditor(typeof(AnimationClip))]
    public class AnimationClipEditor : UnityEditor.Editor
    {
        UnityEditor.Editor animationClipEditor = null;
        UnityEditor.Editor mergedAnimationClipEditor = null;
        private void OnEnable()
        {
            var ae = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.Editor)).GetType("UnityEditor.AnimationClipEditor", true);
            Debug.Log(ae);
            CreateCachedEditor(target, System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.Editor)).GetType("UnityEditor.AnimationClipEditor", true), ref animationClipEditor);
            var setting = AnimationClipMergeProcessor.GetMergedAnimationClip(AssetDatabase.GetAssetPath(target));
            CreateCachedEditor(setting, typeof(AnimationClipMergeSettingEditor), ref mergedAnimationClipEditor);
        }

        public override void OnInspectorGUI()
        {
            animationClipEditor.OnInspectorGUI();
            if (mergedAnimationClipEditor != null)
            {
                var color = GUI.color;
                GUI.color = Color.gray;
                GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(2));
                GUI.color = color;
                EditorGUILayout.LabelField("Merge Setting", EditorStyles.boldLabel);
                mergedAnimationClipEditor.OnInspectorGUI();
            }
        }
    }
}
