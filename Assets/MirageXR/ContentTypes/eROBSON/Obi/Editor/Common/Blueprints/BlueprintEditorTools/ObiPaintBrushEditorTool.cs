using UnityEngine;
using UnityEditor;

namespace Obi
{
    public class ObiPaintBrushEditorTool : ObiBlueprintEditorTool
    {
        public ObiRaycastBrush paintBrush;
        public bool selectionMask = false;
        public int sourcePropertyIndex = 0; /**<index of the property to copy from*/

        public ObiMeshBasedActorBlueprintEditor meshBasedEditor
        {
            get { return editor as ObiMeshBasedActorBlueprintEditor; }
        }

        public ObiPaintBrushEditorTool(ObiMeshBasedActorBlueprintEditor editor) : base(editor)
        {

            m_Icon = Resources.Load<Texture2D>("BrushIcon");
            m_Name = "Property painting";

            paintBrush = new ObiRaycastBrush(editor.sourceMesh,
                                     () =>
                                     {
                                         // As RecordObject diffs with the end of the current frame,
                                         // and this is a multi-frame operation, we need to use RegisterCompleteObjectUndo instead.
                                         Undo.RegisterCompleteObjectUndo(editor.blueprint, "Paint particles");
                                     },
                                     () =>
                                     {
                                         editor.Refresh();
                                     },
                                     () =>
                                     {
                                         EditorUtility.SetDirty(editor.blueprint);
                                     });

        }

        public override string GetHelpString()
        {
            return "Paint particle properties directly on the mesh. Most brushes have an alternate mode, accesed by holding 'shift' while painting.";
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
            EditorGUILayout.Space();

            // toolbar with available brush modes  for the current property:
            editor.currentProperty.BrushModes(paintBrush);

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            editor.currentPropertyIndex = editor.PropertySelector(editor.currentPropertyIndex);
            if (EditorGUI.EndChangeCheck())
            {
                editor.Refresh();
                editor.currentProperty.OnSelect(paintBrush);
            }

            if (paintBrush.brushMode is ObiFloatCopyBrushMode)
            {
                EditorGUI.BeginChangeCheck();
                sourcePropertyIndex = editor.PropertySelector(sourcePropertyIndex, "Copy from");
                var sourceProperty = editor.GetProperty(sourcePropertyIndex) as ObiBlueprintFloatProperty; 
                if (EditorGUI.EndChangeCheck())
                {
                    (paintBrush.brushMode as ObiFloatCopyBrushMode).source = sourceProperty;
                }
                if (sourceProperty == null)
                    EditorGUILayout.HelpBox("You can't copy value from this property.", MessageType.Error);
            }

            if (paintBrush.brushMode.needsInputValue)
                editor.currentProperty.PropertyField();

            paintBrush.radius = EditorGUILayout.Slider("Brush size", paintBrush.radius, 0.0001f, 0.5f);
            paintBrush.innerRadius = EditorGUILayout.Slider("Brush inner size", paintBrush.innerRadius, 0, 1);
            paintBrush.opacity = EditorGUILayout.Slider("Brush opacity", paintBrush.opacity, 0, 1);
            paintBrush.mirror.axis = (ObiBrushMirrorSettings.MirrorAxis)EditorGUILayout.EnumPopup("Brush mirror axis", paintBrush.mirror.axis);
            paintBrush.mirror.space = (ObiBrushMirrorSettings.MirrorSpace)EditorGUILayout.EnumPopup("Brush mirror space", paintBrush.mirror.space);

            EditorGUI.BeginChangeCheck();
            meshBasedEditor.particleCulling = (ObiMeshBasedActorBlueprintEditor.ParticleCulling)EditorGUILayout.EnumPopup("Culling", meshBasedEditor.particleCulling);
            if (editor.selectedCount == 0)
            {
                EditorGUILayout.HelpBox("Select at least one particle to use selection mask.", MessageType.Info);
                selectionMask = false;
                GUI.enabled = false;
            }
            selectionMask = EditorGUILayout.Toggle("Selection mask", selectionMask);
            if (EditorGUI.EndChangeCheck())
                SceneView.RepaintAll();
            GUI.enabled = true;

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            GUILayout.Box(GUIContent.none, ObiEditorUtils.GetSeparatorLineStyle());

            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);

                editor.RenderModeSelector();
                editor.currentProperty.VisualizationOptions();
           
            EditorGUILayout.EndVertical();
        }

        public override bool Editable(int index)
        {
            return editor.visible[index] && (!selectionMask || editor.selectionStatus[index]);
        }

        public override void OnSceneGUI(SceneView view)
        {
            if (Camera.current != null)
            {
                paintBrush.raycastTarget = meshBasedEditor.sourceMesh;
                paintBrush.DoBrush(editor.blueprint.positions);
            }
        }

    }
}
