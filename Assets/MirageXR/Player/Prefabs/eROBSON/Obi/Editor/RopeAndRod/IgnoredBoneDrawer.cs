using UnityEngine;
using UnityEditor;

namespace Obi
{
    [CustomPropertyDrawer(typeof(ObiBone.IgnoredBone))]
    public class IgnoredBoneDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            GUI.Box(EditorGUI.IndentedRect(position), GUIContent.none, ObiEditorUtils.GetToggleablePropertyGroupStyle());

            var rect = new Rect(position.x + 4, position.y + EditorGUIUtility.standardVerticalSpacing, position.width - 8, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("bone"), label);

            rect.position = new Vector2(rect.position.x, rect.position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("ignoreChildren"));

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int lineCount = 2;
            return EditorGUIUtility.singleLineHeight * lineCount + EditorGUIUtility.standardVerticalSpacing * (lineCount + 1);
        }
    }
}
