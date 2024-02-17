using UnityEditor;
using UnityEngine;

namespace Obi{
	
	[CustomEditor(typeof(ObiColliderBase), true), CanEditMultipleObjects] 
	public class ObiColliderEditor : Editor
	{

        ObiColliderBase collider;
        SerializedProperty collisionFilter;

        public void OnEnable()
        {
            collider = (ObiColliderBase)target;
            collisionFilter = serializedObject.FindProperty("filter");
        }

        public override void OnInspectorGUI()
        {

            serializedObject.UpdateIfRequiredOrScript();


            var rect = EditorGUILayout.GetControlRect();
            var label = EditorGUI.BeginProperty(rect, new GUIContent("Collision category"), collisionFilter);
            rect = EditorGUI.PrefixLabel(rect, label);

            EditorGUI.BeginChangeCheck();
            var newCategory = EditorGUI.Popup(rect, ObiUtils.GetCategoryFromFilter(collider.Filter), ObiUtils.categoryNames);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (ObiColliderBase t in targets)
                {
                    Undo.RecordObject(t, "Set collision category");
                    t.Filter = ObiUtils.MakeFilter(ObiUtils.GetMaskFromFilter(t.Filter), newCategory);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(t);
                }
            }
            EditorGUI.EndProperty();

            rect = EditorGUILayout.GetControlRect();
            label = EditorGUI.BeginProperty(rect, new GUIContent("Collides with"), collisionFilter);
            rect = EditorGUI.PrefixLabel(rect, label);

            EditorGUI.BeginChangeCheck();
            var newMask = EditorGUI.MaskField(rect, ObiUtils.GetMaskFromFilter(collider.Filter), ObiUtils.categoryNames);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (ObiColliderBase t in targets)
                {
                    Undo.RecordObject(t, "Set collision mask");
                    t.Filter = ObiUtils.MakeFilter(newMask, ObiUtils.GetCategoryFromFilter(t.Filter));
                    PrefabUtility.RecordPrefabInstancePropertyModifications(t);
                }
            }
            EditorGUI.EndProperty();


            DrawPropertiesExcluding(serializedObject, "m_Script", "CollisionMaterial", "filter", "Thickness");

            // Apply changes to the serializedProperty
            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }

        }

    }
}


