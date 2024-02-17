using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Obi
{
    public class ObiParticleSelectionEditorTool : ObiBlueprintEditorTool
    {
        ObiScreenSpaceBrush selectionBrush;
        ObiSelectBrushMode selectMode;
        ObiTethersTool tethersTool;

        protected ReorderableList particleGroupList;
        protected bool mixedPropertyValue = false;
        protected float minSelectionValue;
        protected float maxSelectionValue;

        public ObiParticleSelectionEditorTool(ObiActorBlueprintEditor editor) : base(editor)
        {
            m_Icon = Resources.Load<Texture2D>("SelectIcon");
            m_Name = "Particle selection";

            selectionBrush = new ObiScreenSpaceBrush(null, UpdateSelection, null);
            selectMode = new ObiSelectBrushMode(new ObiBlueprintSelected(editor));

            selectionBrush.brushMode = selectMode;
            tethersTool = new ObiTethersTool();

            InitializeGroupsList();
        }


        public override string GetHelpString()
        {
            if (editor.selectedCount > 0)
                return "" + editor.selectedCount + " selected particles.";
            else
                return "No particles selected. Click and drag over particles to select them.";
        }

        private void InitializeGroupsList()
        {
            particleGroupList = new ReorderableList(editor.serializedObject,
                                                    editor.serializedObject.FindProperty("groups"),
                              false, true, true, true);

            particleGroupList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Groups");
            };

            particleGroupList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = particleGroupList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 4;

                SerializedObject obj = new SerializedObject(element.objectReferenceValue);
                ObiParticleGroup group = obj.targetObject as ObiParticleGroup;

                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                                        obj.FindProperty("m_Name"), new GUIContent("Name"));
                rect.y += EditorGUIUtility.singleLineHeight + 2;

                if (GUI.Button(new Rect(rect.x, rect.y, rect.width * 0.5f, EditorGUIUtility.singleLineHeight), "Select", EditorStyles.miniButtonLeft))
                {
                    if ((Event.current.modifiers & EventModifiers.Shift) == 0)
                    {
                        for (int p = 0; p < editor.selectionStatus.Length; p++)
                            editor.selectionStatus[p] = false;
                    }

                    foreach (int p in group.particleIndices)
                        editor.selectionStatus[p] = true;

                    UpdateSelection();
                }

                if (GUI.Button(new Rect(rect.x + rect.width * 0.5f, rect.y, rect.width * 0.5f, EditorGUIUtility.singleLineHeight), "Set", EditorStyles.miniButtonRight))
                {
                    group.particleIndices.Clear();
                    for (int p = 0; p < editor.selectionStatus.Length; p++)
                    {
                        if (editor.selectionStatus[p])
                            group.particleIndices.Add(p);
                    }
                }

                obj.ApplyModifiedProperties();
            };

            particleGroupList.elementHeight = (EditorGUIUtility.singleLineHeight + 2) * 2 + 8;

            particleGroupList.onAddCallback = (ReorderableList list) =>
            {

                var group = editor.blueprint.AppendNewParticleGroup("new group");

                for (int i = 0; i < editor.selectionStatus.Length; i++)
                {
                    if (editor.selectionStatus[i])
                        group.particleIndices.Add(i);
                }

                AssetDatabase.SaveAssets();
            };

            particleGroupList.onRemoveCallback = (ReorderableList list) =>
            {
                editor.blueprint.RemoveParticleGroupAt(list.index);
            };
        }

        private void SelectionTools()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("InvertButton"), "Invert selection"), GUILayout.MaxHeight(24), GUILayout.MaxWidth(48)))
            {
                for (int i = 0; i < editor.selectionStatus.Length; i++)
                {
                    if (editor.blueprint.IsParticleActive(i))
                        editor.selectionStatus[i] = !editor.selectionStatus[i];
                }
                UpdateSelection();
            }

            GUI.enabled = editor.selectedCount > 0;
            if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("ClearButton"), "Clear selection"), GUILayout.MaxHeight(24), GUILayout.MaxWidth(48)))
            {
                for (int i = 0; i < editor.selectionStatus.Length; i++)
                    editor.selectionStatus[i] = false;
                UpdateSelection();
            }

            if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("OptimizeButton"), "Optimize selected"), GUILayout.MaxHeight(24), GUILayout.MaxWidth(48)))
            {
                Undo.RecordObject(editor.blueprint, "Optimize particles away");
                editor.blueprint.RemoveSelectedParticles(ref editor.selectionStatus);
                editor.Refresh();
            }

            if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("RemoveButton"), "Remove selected"), GUILayout.MaxHeight(24), GUILayout.MaxWidth(48)))
            {
                Undo.RecordObject(editor.blueprint, "Remove particles");
                editor.blueprint.RemoveSelectedParticles(ref editor.selectionStatus, false);
                editor.Refresh();
            }
            GUI.enabled = true;

            if (GUILayout.Button(new GUIContent(Resources.Load<Texture2D>("RestoreButton"), "Restore removed particles"), GUILayout.MaxHeight(24), GUILayout.MaxWidth(48)))
            {
                Undo.RecordObject(editor.blueprint, "Restore removed particles");
                editor.blueprint.RestoreRemovedParticles();
                editor.Refresh();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Property-based selection", EditorStyles.boldLabel);
            var property = editor.currentProperty as ObiBlueprintFloatProperty;
            if (property != null)
            {
                if (!Mathf.Approximately(property.minVisualizationValue,property.maxVisualizationValue))
                {
                    EditorGUILayout.HelpBox("Drag the slider to select based on " + property.name + ". You can choose a different property in the \"Property\" dropdown below.", MessageType.None);
                    minSelectionValue = Mathf.Max(minSelectionValue, property.minVisualizationValue);
                    maxSelectionValue = Mathf.Min(maxSelectionValue, property.maxVisualizationValue);
                    maxSelectionValue = Mathf.Max(maxSelectionValue, minSelectionValue);

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.MinMaxSlider("Select by " + property.name, ref minSelectionValue, ref maxSelectionValue, property.minVisualizationValue, property.maxVisualizationValue);
                    minSelectionValue = EditorGUILayout.FloatField("Minimum " + property.name, minSelectionValue);
                    maxSelectionValue = EditorGUILayout.FloatField("Maximum " + property.name, maxSelectionValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        for (int i = 0; i < editor.selectionStatus.Length; i++)
                        {
                            if (editor.blueprint.IsParticleActive(i))
                            {
                                var value = property.Get(i);
                                editor.selectionStatus[i] = value >= minSelectionValue && value <= maxSelectionValue;
                            }
                        }
                        UpdateSelection();
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("All particles have the same " + property.name + " value.", MessageType.Info);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Property-based selection only works with scalar properties.",MessageType.Info);
            }
        }

        public override void OnInspectorGUI()
        {
            // Selection tools:
            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
            EditorGUILayout.Space();

            selectionBrush.radius = EditorGUILayout.Slider("Brush size", selectionBrush.radius, 5, 200);

            if (editor is ObiMeshBasedActorBlueprintEditor)
            {
                EditorGUI.BeginChangeCheck();
                (editor as ObiMeshBasedActorBlueprintEditor).particleCulling = (ObiMeshBasedActorBlueprintEditor.ParticleCulling)EditorGUILayout.EnumPopup("Culling", (editor as ObiMeshBasedActorBlueprintEditor).particleCulling);
                if (EditorGUI.EndChangeCheck())
                    SceneView.RepaintAll();
            }


            EditorGUILayout.Space();
            SelectionTools();

            EditorGUILayout.EndVertical();


            // Properties:
            EditorGUILayout.Space();
            GUILayout.Box(GUIContent.none, ObiEditorUtils.GetSeparatorLineStyle());

            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Select a property to view and edit. Currently editing " + editor.currentProperty.name+".", MessageType.None);

            EditorGUI.BeginChangeCheck();
            editor.currentPropertyIndex = editor.PropertySelector(editor.currentPropertyIndex);
            if (EditorGUI.EndChangeCheck())
            {
                editor.Refresh();
                UpdateSelection();
            }

            // Property value:
            EditorGUI.showMixedValue = mixedPropertyValue;
            EditorGUI.BeginChangeCheck();
            editor.currentProperty.PropertyField();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(editor.blueprint, "Set particle property");
                for (int i = 0; i < editor.selectionStatus.Length; i++)
                {
                    if (!editor.selectionStatus[i]) continue;
                    editor.currentProperty.SetDefaultToIndex(i);
                }
                editor.Refresh();
            }

            EditorGUI.showMixedValue = false;

            EditorGUILayout.EndVertical();


            // Particle groups:
            EditorGUILayout.Space();
            GUILayout.Box(GUIContent.none, ObiEditorUtils.GetSeparatorLineStyle());

            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Particle groups", EditorStyles.boldLabel);
            particleGroupList.DoLayoutList();

            EditorGUILayout.EndVertical();



            if (editor.blueprint.usesTethers)
            {
                EditorGUILayout.Space();
                GUILayout.Box(GUIContent.none, ObiEditorUtils.GetSeparatorLineStyle());
                EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
                EditorGUILayout.Space();
                tethersTool.DoTethers(editor);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();
            GUILayout.Box(GUIContent.none, ObiEditorUtils.GetSeparatorLineStyle());

            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);

                editor.RenderModeSelector();
                editor.dotRadiusScale = EditorGUILayout.Slider(new GUIContent("Particle dot size"), editor.dotRadiusScale, 0, 5);
                editor.currentProperty.VisualizationOptions();
           
            EditorGUILayout.EndVertical();
        }

        public override void OnSceneGUI(SceneView sceneView)
        {
            if (Camera.current != null)
                selectionBrush.DoBrush(editor.blueprint.positions);
        }

        protected void UpdateSelection()
        {
            editor.selectedCount = 0;
            mixedPropertyValue = false;

            // Find out how many selected particles we have, and whether they all have the same value for the current property:
            for (int i = 0; i < editor.selectionStatus.Length; i++)
            {
                if (editor.blueprint.IsParticleActive(i) && editor.selectionStatus[i])
                {
                    editor.selectedCount++;

                    if (editor.activeParticle >= 0)
                    {
                        if (!editor.currentProperty.Equals(editor.activeParticle, i))
                            mixedPropertyValue = true;
                    }
                    else
                        editor.activeParticle = i;
                }
                else if (editor.activeParticle == i)
                    editor.activeParticle = -1;
            }

            // Set initial property value:
            if (!mixedPropertyValue && editor.activeParticle >= 0)
                editor.currentProperty.GetDefaultFromIndex(editor.activeParticle);

            editor.Repaint();
            SceneView.RepaintAll();

        }

    }
}