using UnityEditor;

namespace ReadyPlayerMe.Core.Editor
{
    [CustomEditor(typeof(AvatarData))]
    public class AvatarDataEditor : UnityEditor.Editor
    {
        private SerializedProperty avatarIdProperty;
        private SerializedProperty avatarMetadataProperty;

        private void OnEnable()
        {
            var avatarData = (AvatarData) target;

            avatarIdProperty = serializedObject.FindProperty(nameof(avatarData.AvatarId));
            avatarMetadataProperty = serializedObject.FindProperty(nameof(avatarData.AvatarMetadata));
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(avatarIdProperty);
            EditorGUILayout.PropertyField(avatarMetadataProperty);
            EditorGUI.EndDisabledGroup();
        }
    }
}
