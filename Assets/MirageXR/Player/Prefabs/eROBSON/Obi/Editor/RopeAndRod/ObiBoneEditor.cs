using UnityEditor;
using UnityEngine;

namespace Obi
{

    [CustomEditor(typeof(ObiBone))]
    public class ObiBoneEditor : Editor
    {
        ObiBone bone;

        SerializedProperty collisionMaterial;
        SerializedProperty selfCollisions;
        SerializedProperty surfaceCollisions;

        SerializedProperty mass;
        SerializedProperty rotationalMass;
        SerializedProperty radius;

        SerializedProperty skinConstraintsEnabled;
        SerializedProperty skinCompliance;
        SerializedProperty skinRadius;

        SerializedProperty stretchShearConstraintsEnabled;
        SerializedProperty stretchCompliance;
        SerializedProperty shear1Compliance;
        SerializedProperty shear2Compliance;

        SerializedProperty bendTwistConstraintsEnabled;
        SerializedProperty torsionCompliance;
        SerializedProperty bend1Compliance;
        SerializedProperty bend2Compliance;
        SerializedProperty plasticYield;
        SerializedProperty plasticCreep;

        SerializedProperty fixRoot;
        SerializedProperty stretchBones;
        SerializedProperty ignored;

        public void OnEnable()
        {
            bone = (ObiBone)target;

            fixRoot = serializedObject.FindProperty("fixRoot");
            stretchBones = serializedObject.FindProperty("stretchBones");
            ignored = serializedObject.FindProperty("ignored");

            collisionMaterial = serializedObject.FindProperty("m_CollisionMaterial");
            selfCollisions = serializedObject.FindProperty("m_SelfCollisions");
            surfaceCollisions = serializedObject.FindProperty("m_SurfaceCollisions");

            mass = serializedObject.FindProperty("_mass");
            rotationalMass = serializedObject.FindProperty("_rotationalMass");
            radius = serializedObject.FindProperty("_radius");

            skinConstraintsEnabled = serializedObject.FindProperty("_skinConstraintsEnabled");
            skinRadius = serializedObject.FindProperty("_skinRadius");
            skinCompliance = serializedObject.FindProperty("_skinCompliance");

            stretchShearConstraintsEnabled = serializedObject.FindProperty("_stretchShearConstraintsEnabled");
            stretchCompliance = serializedObject.FindProperty("_stretchCompliance");
            shear1Compliance = serializedObject.FindProperty("_shear1Compliance");
            shear2Compliance = serializedObject.FindProperty("_shear2Compliance");

            bendTwistConstraintsEnabled = serializedObject.FindProperty("_bendTwistConstraintsEnabled");
            torsionCompliance = serializedObject.FindProperty("_torsionCompliance");
            bend1Compliance = serializedObject.FindProperty("_bend1Compliance");
            bend2Compliance = serializedObject.FindProperty("_bend2Compliance");
            plasticYield = serializedObject.FindProperty("_plasticYield");
            plasticCreep = serializedObject.FindProperty("_plasticCreep");

        }

        public void OnDisable()
        {
            Tools.hidden = false;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.LabelField("Bones", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(fixRoot);
            EditorGUILayout.PropertyField(stretchBones);
            EditorGUILayout.PropertyField(ignored);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Collisions", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(collisionMaterial, new GUIContent("Collision material"));
            EditorGUI.BeginChangeCheck();
            var newCategory = EditorGUILayout.Popup("Collision category", ObiUtils.GetCategoryFromFilter(bone.Filter), ObiUtils.categoryNames);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (ObiBone t in targets)
                {
                    Undo.RecordObject(t, "Set collision category");
                    t.Filter = ObiUtils.MakeFilter(ObiUtils.GetMaskFromFilter(t.Filter), newCategory);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(t);
                }
            }

            EditorGUI.BeginChangeCheck();
            var newMask = EditorGUILayout.MaskField("Collides with", ObiUtils.GetMaskFromFilter(bone.Filter), ObiUtils.categoryNames);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (ObiBone t in targets)
                {
                    Undo.RecordObject(t, "Set collision mask");
                    t.Filter = ObiUtils.MakeFilter(newMask, ObiUtils.GetCategoryFromFilter(t.Filter));
                    PrefabUtility.RecordPrefabInstancePropertyModifications(t);
                }
            }
            EditorGUILayout.PropertyField(selfCollisions, new GUIContent("Self collisions"));
            EditorGUILayout.PropertyField(surfaceCollisions, new GUIContent("Surface-based collisions"));
            EditorGUILayout.Space();

            ObiEditorUtils.DoPropertyGroup(new GUIContent("Particles"),
            () => {
                EditorGUILayout.PropertyField(mass);
                EditorGUILayout.PropertyField(rotationalMass);
                EditorGUILayout.PropertyField(radius);
            });

            ObiEditorUtils.DoToggleablePropertyGroup(skinConstraintsEnabled, new GUIContent("Skin Constraints", Resources.Load<Texture2D>("Icons/ObiSkinConstraints Icon")),
            () => {
                EditorGUILayout.PropertyField(skinRadius, new GUIContent("Skin radius"));
                EditorGUILayout.PropertyField(skinCompliance, new GUIContent("Skin compliance"));
            });

            ObiEditorUtils.DoToggleablePropertyGroup(stretchShearConstraintsEnabled, new GUIContent("Stretch & Shear Constraints", Resources.Load<Texture2D>("Icons/ObiStretchShearConstraints Icon")),
            () => {
                EditorGUILayout.PropertyField(stretchCompliance, new GUIContent("Stretch compliance"));
                EditorGUILayout.PropertyField(shear1Compliance, new GUIContent("Shear compliance X"));
                EditorGUILayout.PropertyField(shear2Compliance, new GUIContent("Shear compliance Y"));
            });

            ObiEditorUtils.DoToggleablePropertyGroup(bendTwistConstraintsEnabled, new GUIContent("Bend & Twist Constraints", Resources.Load<Texture2D>("Icons/ObiBendTwistConstraints Icon")),
            () => {
                EditorGUILayout.PropertyField(torsionCompliance, new GUIContent("Torsion compliance"));
                EditorGUILayout.PropertyField(bend1Compliance, new GUIContent("Bend compliance X"));
                EditorGUILayout.PropertyField(bend2Compliance, new GUIContent("Bend compliance Y"));
                EditorGUILayout.PropertyField(plasticYield, new GUIContent("Plastic yield"));
                EditorGUILayout.PropertyField(plasticCreep, new GUIContent("Plastic creep"));
            });

            if (GUI.changed)
                serializedObject.ApplyModifiedProperties();

        }


        [DrawGizmo(GizmoType.Selected)]
        private static void DrawGizmos(ObiBone actor, GizmoType gizmoType)
        {
            if (actor.boneBlueprint != null && actor.isLoaded)
            {
                var color = new Color(1, 1, 1, 0.5f);
                var upColor = new Color(0, 1, 0, 1);

                for (int i = 0; i < actor.boneBlueprint.parentIndices.Count; ++i)
                {
                    int parent = actor.boneBlueprint.parentIndices[i];
                    if (parent >= 0)
                    {
                        var index = actor.solverIndices[parent];
                        var nextIndex = actor.solverIndices[i];

                        var pos = actor.GetParticlePosition(index);
                        var npos = actor.GetParticlePosition(nextIndex);
                        var or = actor.GetParticleOrientation(index);
                        var nor = actor.GetParticleOrientation(nextIndex);
                        var rad = actor.GetParticleMaxRadius(index);
                        var nrad = actor.GetParticleMaxRadius(nextIndex);

                        var up = pos + or * Vector3.up * rad;
                        var down = pos + or * Vector3.down * rad;
                        var left = pos + or * Vector3.left * rad;
                        var right = pos + or * Vector3.right * rad;

                        var nup = npos + nor * Vector3.up * nrad;
                        var ndown = npos + nor * Vector3.down * nrad;
                        var nleft = npos + nor * Vector3.left * nrad;
                        var nright = npos + nor * Vector3.right * nrad;

                        Handles.color = upColor;
                        Handles.DrawLine(up, nup);

                        Handles.color = color;
                        Handles.DrawLine(down, ndown);
                        Handles.DrawLine(left, nleft);
                        Handles.DrawLine(right, nright);
                        Handles.DrawWireDisc(npos, nor * Vector3.forward, nrad);
                    }
                }
                if (actor.particleCount > 0)
                {
                    var index = actor.solverIndices[0];
                    var pos = actor.GetParticlePosition(index);
                    var or = actor.GetParticleOrientation(index);
                    var rad = actor.GetParticleMaxRadius(index);

                    Handles.DrawWireDisc(pos, or * Vector3.forward, rad);
                }
            }
        }

    }

}


