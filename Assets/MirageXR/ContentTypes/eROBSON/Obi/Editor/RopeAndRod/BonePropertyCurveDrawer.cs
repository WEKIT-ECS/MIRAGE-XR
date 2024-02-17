using UnityEngine;
using UnityEditor;

namespace Obi
{ 
    [CustomPropertyDrawer(typeof(ObiBone.BonePropertyCurve))]
    public class BonePropertyCurveDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float curveFieldWidth = (position.width - EditorGUIUtility.labelWidth) * 0.5f;

            var multRect = new Rect(position.x, position.y, position.width - curveFieldWidth, position.height);
            EditorGUI.PropertyField(multRect, property.FindPropertyRelative("multiplier"), label);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var curveRect = new Rect(position.x + position.width - curveFieldWidth + 3, position.y, curveFieldWidth - 3, position.height);
            EditorGUI.CurveField(curveRect, property.FindPropertyRelative("curve"), Color.green, new Rect(0, 0, 1, 1), GUIContent.none);

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}
