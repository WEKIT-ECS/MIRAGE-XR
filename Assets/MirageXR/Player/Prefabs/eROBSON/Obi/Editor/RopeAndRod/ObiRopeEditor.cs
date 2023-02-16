using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditorInternal;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Obi
{

    [CustomEditor(typeof(ObiRope))]
    public class ObiRopeEditor : Editor
    {

        [MenuItem("GameObject/3D Object/Obi/Obi Rope", false, 300)]
        static void CreateObiRope(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("Obi Rope", typeof(ObiRope), typeof(ObiRopeExtrudedRenderer));
            ObiEditorUtils.PlaceActorRoot(go, menuCommand);
        }

        ObiRope actor;

        SerializedProperty ropeBlueprint;

        SerializedProperty collisionMaterial;
        SerializedProperty selfCollisions;
        SerializedProperty surfaceCollisions;

        SerializedProperty distanceConstraintsEnabled;
        SerializedProperty stretchingScale;
        SerializedProperty stretchCompliance;
        SerializedProperty maxCompression;

        SerializedProperty bendConstraintsEnabled;
        SerializedProperty bendCompliance;
        SerializedProperty maxBending;
        SerializedProperty plasticYield;
        SerializedProperty plasticCreep;

        SerializedProperty tearingEnabled;
        SerializedProperty tearResistanceMultiplier;
        SerializedProperty tearRate;

        GUIStyle editLabelStyle;

        public void OnEnable()
        {
            actor = (ObiRope)target;

            ropeBlueprint = serializedObject.FindProperty("m_RopeBlueprint");

            collisionMaterial = serializedObject.FindProperty("m_CollisionMaterial");
            selfCollisions = serializedObject.FindProperty("m_SelfCollisions");
            surfaceCollisions = serializedObject.FindProperty("m_SurfaceCollisions");

            distanceConstraintsEnabled = serializedObject.FindProperty("_distanceConstraintsEnabled");
            stretchingScale = serializedObject.FindProperty("_stretchingScale");
            stretchCompliance = serializedObject.FindProperty("_stretchCompliance");
            maxCompression = serializedObject.FindProperty("_maxCompression");

            bendConstraintsEnabled = serializedObject.FindProperty("_bendConstraintsEnabled");
            bendCompliance = serializedObject.FindProperty("_bendCompliance");
            maxBending = serializedObject.FindProperty("_maxBending");
            plasticYield = serializedObject.FindProperty("_plasticYield");
            plasticCreep = serializedObject.FindProperty("_plasticCreep");

            tearingEnabled = serializedObject.FindProperty("tearingEnabled");
            tearResistanceMultiplier = serializedObject.FindProperty("tearResistanceMultiplier");
            tearRate = serializedObject.FindProperty("tearRate");

        }

        private void DoEditButton()
        {
            using (new EditorGUI.DisabledScope(actor.ropeBlueprint == null))
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUIUtility.labelWidth);
                EditorGUI.BeginChangeCheck();
                bool edit = GUILayout.Toggle(ToolManager.activeToolType == typeof(ObiPathEditor), new GUIContent(Resources.Load<Texture2D>("EditCurves")), "Button", GUILayout.MaxWidth(36), GUILayout.MaxHeight(24));
                EditorGUILayout.LabelField("Edit path", editLabelStyle, GUILayout.ExpandHeight(true), GUILayout.MaxHeight(24));
                if (EditorGUI.EndChangeCheck())
                {
                    if (edit)
                        ToolManager.SetActiveTool<ObiPathEditor>();
                    else
                        ToolManager.RestorePreviousPersistentTool();

                    SceneView.RepaintAll();
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        public override void OnInspectorGUI()
        {
            if (editLabelStyle == null)
            {
                editLabelStyle = new GUIStyle(GUI.skin.label);
                editLabelStyle.alignment = TextAnchor.MiddleLeft;
            }

            serializedObject.UpdateIfRequiredOrScript();

            if (actor.sourceBlueprint != null && actor.ropeBlueprint.path.ControlPointCount < 2)
            {
                actor.ropeBlueprint.GenerateImmediate();
            }

            using (new EditorGUI.DisabledScope(ToolManager.activeToolType == typeof(ObiPathEditor)))
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(ropeBlueprint, new GUIContent("Blueprint"));
                if (EditorGUI.EndChangeCheck())
                {
                    foreach (var t in targets)
                    {
                        (t as ObiRope).RemoveFromSolver();
                        (t as ObiRope).ClearState();
                    }
                    serializedObject.ApplyModifiedProperties();
                    foreach (var t in targets)
                        (t as ObiRope).AddToSolver();
                }
            }

            DoEditButton();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Collisions", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(collisionMaterial, new GUIContent("Collision material"));
            EditorGUILayout.PropertyField(selfCollisions, new GUIContent("Self collisions"));
            EditorGUILayout.PropertyField(surfaceCollisions, new GUIContent("Surface-based collisions"));

            EditorGUILayout.Space();
            ObiEditorUtils.DoToggleablePropertyGroup(tearingEnabled, new GUIContent("Tearing"),
            () =>
            {
                EditorGUILayout.PropertyField(tearResistanceMultiplier, new GUIContent("Tear resistance"));
                EditorGUILayout.PropertyField(tearRate, new GUIContent("Tear rate"));
            });
            ObiEditorUtils.DoToggleablePropertyGroup(distanceConstraintsEnabled, new GUIContent("Distance Constraints", Resources.Load<Texture2D>("Icons/ObiDistanceConstraints Icon")),
            () =>
            {
                EditorGUILayout.PropertyField(stretchingScale, new GUIContent("Stretching scale"));
                EditorGUILayout.PropertyField(stretchCompliance, new GUIContent("Stretch compliance"));
                EditorGUILayout.PropertyField(maxCompression, new GUIContent("Max compression"));
            });

            ObiEditorUtils.DoToggleablePropertyGroup(bendConstraintsEnabled, new GUIContent("Bend Constraints", Resources.Load<Texture2D>("Icons/ObiBendConstraints Icon")),
            () =>
            {
                EditorGUILayout.PropertyField(bendCompliance, new GUIContent("Bend compliance"));
                EditorGUILayout.PropertyField(maxBending, new GUIContent("Max bending"));
                EditorGUILayout.PropertyField(plasticYield, new GUIContent("Plastic yield"));
                EditorGUILayout.PropertyField(plasticCreep, new GUIContent("Plastic creep"));
            });


            if (GUI.changed)
                serializedObject.ApplyModifiedProperties();

        }

        [DrawGizmo(GizmoType.Selected)]
        private static void DrawGizmos(ObiRope actor, GizmoType gizmoType)
        {
            Handles.color = Color.white;
            if (actor.ropeBlueprint != null)
                ObiPathHandles.DrawPathHandle(actor.ropeBlueprint.path, actor.transform.localToWorldMatrix, actor.ropeBlueprint.thickness, 20, false);
        }

    }

}


