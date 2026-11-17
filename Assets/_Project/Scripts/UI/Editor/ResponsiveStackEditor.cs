using UnityEditor;
using UnityEditor.UI;

namespace AimCombat.UI.Editor
{
    /// <summary>
    /// ResponsiveStack 인스펙터. 기본 레이아웃 그룹 인스펙터가 자식 클래스에도 쓰여 방향 설정이 가려지므로, 방향 두 칸을 위에 더 그린다.
    /// </summary>
    [CustomEditor(typeof(ResponsiveStack), true)]
    [CanEditMultipleObjects]
    public sealed class ResponsiveStackEditor : HorizontalOrVerticalLayoutGroupEditor
    {
        private SerializedProperty verticalInLandscape;
        private SerializedProperty verticalInPortrait;

        protected override void OnEnable()
        {
            base.OnEnable();
            verticalInLandscape = serializedObject.FindProperty("verticalInLandscape");
            verticalInPortrait = serializedObject.FindProperty("verticalInPortrait");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(verticalInLandscape);
            EditorGUILayout.PropertyField(verticalInPortrait);
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}
