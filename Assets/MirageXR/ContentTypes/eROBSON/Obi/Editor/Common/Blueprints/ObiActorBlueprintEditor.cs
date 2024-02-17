using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Obi
{

    [CustomEditor(typeof(ObiActorBlueprint), true)]
    public class ObiActorBlueprintEditor : Editor, IObiSelectableParticleProvider
    {
        protected IEnumerator routine;

        public List<ObiBlueprintEditorTool> tools = new List<ObiBlueprintEditorTool>();
        public int currentToolIndex = 0;

        protected List<ObiBlueprintPropertyBase> properties = new List<ObiBlueprintPropertyBase>();
        public int currentPropertyIndex = 0;

        protected List<ObiBlueprintRenderMode> renderModes = new List<ObiBlueprintRenderMode>();
        public int renderModeFlags = 0;
        BooleanPreference showRenderModes;

        public bool editMode = false;
        public bool isEditing = false;
        protected List<SceneStateCache> m_SceneStates;
        protected SceneSetup[] oldSetup;
        protected UnityEngine.Object oldSelection;

        //Additional status info for all particles:
        public float dotRadiusScale = 1;
        public int selectedCount = 0;
        public int activeParticle = -1;
        public bool[] selectionStatus = new bool[0];
        public bool[] visible = new bool[0];
        public Color[] tint = new Color[0];
        protected float[] sqrDistanceToCamera = new float[0];
        public int[] sortedIndices = new int[0];

        public ObiActorBlueprint blueprint
        {
            get { return target as ObiActorBlueprint; }
        }

        public ObiBlueprintPropertyBase currentProperty
        {
            get { return GetProperty(currentPropertyIndex); }
        }

        public ObiBlueprintEditorTool currentTool
        {
            get { return GetTool(currentToolIndex); }
        }

        public override bool UseDefaultMargins()
        {
            return false;
        }

        public ObiBlueprintPropertyBase GetProperty(int index)
        {
            return (properties.Count > index && index >= 0) ? properties[index] : null;
        }

        public ObiBlueprintEditorTool GetTool(int index)
        {
            return (tools.Count > index && index >= 0) ? tools[index] : null;
        }

#if (UNITY_2019_1_OR_NEWER)
        System.Action<ScriptableRenderContext, Camera> renderCallback;
#endif

        public virtual void OnEnable()
        {
            properties.Add(new ObiBlueprintMass(this));
            properties.Add(new ObiBlueprintRadius(this));
            properties.Add(new ObiBlueprintFilterCategory(this));
            properties.Add(new ObiBlueprintFilterMask(this));

            renderModes.Add(new ObiBlueprintRenderModeParticles(this));
            showRenderModes = new BooleanPreference($"{target.GetType()}.showRenderModes", false);

#if (UNITY_2019_1_OR_NEWER)
            renderCallback = new System.Action<ScriptableRenderContext, Camera>((cntxt, cam) => { DrawWithCamera(cam); });
            RenderPipelineManager.beginCameraRendering -= renderCallback;
            RenderPipelineManager.beginCameraRendering += renderCallback;
#endif
            Camera.onPreCull -= DrawWithCamera;
            Camera.onPreCull += DrawWithCamera;
        }

        public virtual void OnDisable()
        {
            ExitBlueprintEditMode();

#if (UNITY_2019_1_OR_NEWER)
            RenderPipelineManager.beginCameraRendering -= renderCallback;
#endif
            Camera.onPreCull -= DrawWithCamera;

            ObiParticleEditorDrawing.DestroyParticlesMesh();

            foreach (var tool in tools)
            {
                tool.OnDisable();
                tool.OnDestroy();
            }

            foreach (var renderMode in renderModes)
            {
                renderMode.OnDestroy();
            }

            properties.Clear();
            renderModes.Clear();
        }

        protected void Generate()
        {
            if (blueprint.empty)
            {
                EditorUtility.SetDirty(target);
                CoroutineJob job = new CoroutineJob();
                routine = job.Start(blueprint.Generate());
                EditorCoroutine.ShowCoroutineProgressBar("Generating blueprint...", ref routine);
                Refresh();
                EditorGUIUtility.ExitGUI();
            }
            else
            {
                if (EditorUtility.DisplayDialog("Blueprint generation", "This blueprint already contains data. Are you sure you want to re-generate this blueprint from scratch?", "Ok", "Cancel"))
                {
                    EditorUtility.SetDirty(target);
                    CoroutineJob job = new CoroutineJob();
                    routine = job.Start(blueprint.Generate());
                    EditorCoroutine.ShowCoroutineProgressBar("Generating blueprint...", ref routine);
                    Refresh();
                    EditorGUIUtility.ExitGUI();
                }
            }
        }

        protected virtual bool ValidateBlueprint() { return true; }

        public override void OnInspectorGUI()
        {

            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);

            DrawBlueprintProperties();

            GUILayout.Space(10);
            GUI.enabled = ValidateBlueprint();
            if (GUILayout.Button("Generate", GUI.skin.FindStyle("LargeButton"), GUILayout.Height(32)))
                Generate();

            GUI.enabled = (blueprint != null && !blueprint.empty && !Application.isPlaying);
            EditorGUI.BeginChangeCheck();
            editMode = GUILayout.Toggle(editMode, editMode ? "Done" : "Edit", "Button");
            if (EditorGUI.EndChangeCheck())
            {
                if (editMode)
                    EditorApplication.delayCall += EnterBlueprintEditMode;
                else
                    EditorApplication.delayCall += ExitBlueprintEditMode;
            }
            EditorGUILayout.EndVertical();
            GUI.enabled = true;

            if (isEditing)
                DrawTools();

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();

                // There might be blueprint editing operations that have no undo entry, so do this to 
                // ensure changes are serialized to disk by Unity.
                EditorUtility.SetDirty(target);
            }

        }

        protected virtual void DrawBlueprintProperties()
        {
            Editor.DrawPropertiesExcluding(serializedObject, "m_Script");
        }

        private void DrawWithCamera(Camera camera)
        {
            if (editMode)
            {
                for (int i = 0; i < renderModes.Count; ++i)
                {
                    if ((1 << i & renderModeFlags) != 0)
                        renderModes[i].DrawWithCamera(camera);
                }
            }
        }


        [System.Serializable]
        protected class SceneStateCache
        {
            public SceneView view;
            public SceneView.SceneViewState state;
        }

        void EnterBlueprintEditMode()
        {
            if (!isEditing)
            {
#if (UNITY_2019_1_OR_NEWER)
                SceneView.duringSceneGui -= this.OnSceneGUI;
                SceneView.duringSceneGui += this.OnSceneGUI;
#else
                SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
                SceneView.onSceneGUIDelegate += this.OnSceneGUI;
#endif

                oldSelection = Selection.activeObject;
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    ActiveEditorTracker.sharedTracker.isLocked = true;

                    oldSetup = EditorSceneManager.GetSceneManagerSetup();
                    EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);

                    // Set properties for all scene views:
                    m_SceneStates = new List<SceneStateCache>();
                    foreach (SceneView s in SceneView.sceneViews)
                    {
                        m_SceneStates.Add(new SceneStateCache { state = new SceneView.SceneViewState(s.sceneViewState), view = s });
                        s.sceneViewState.showFlares = false;
                        s.sceneViewState.alwaysRefresh = false;
                        s.sceneViewState.showFog = false;
                        s.sceneViewState.showSkybox = false;
                        s.sceneViewState.showImageEffects = false;
                        s.sceneViewState.showParticleSystems = false;
                        s.Frame(blueprint.bounds);
                    }

                    isEditing = true;
                    Repaint();
                }
            }
        }

        void ExitBlueprintEditMode()
        {
            if (isEditing)
            {

                isEditing = false;

                AssetDatabase.SaveAssets();

                // Reset all scene views:
                foreach (var state in m_SceneStates)
                {
                    if (state.view == null)
                        continue;

                    state.view.sceneViewState.showFog = state.state.showFog;
                    state.view.sceneViewState.showFlares = state.state.showFlares;
                    state.view.sceneViewState.alwaysRefresh = state.state.alwaysRefresh;
                    state.view.sceneViewState.showSkybox = state.state.showSkybox;
                    state.view.sceneViewState.showImageEffects = state.state.showImageEffects;
                    state.view.sceneViewState.showParticleSystems = state.state.showParticleSystems;
                }

                ActiveEditorTracker.sharedTracker.isLocked = false;

                if (SceneManager.GetActiveScene().path.Length <= 0)
                {
                    if (oldSetup != null && oldSetup.Length > 0)
                    {
                        EditorSceneManager.RestoreSceneManagerSetup(oldSetup);
                        oldSetup = null;
                    }
                    else
                    {
                        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
                    }
                }

                Selection.activeObject = oldSelection;

#if (UNITY_2019_1_OR_NEWER)
                SceneView.duringSceneGui -= this.OnSceneGUI;
#else
                SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
#endif

                Repaint();
            }
        }

        public virtual void OnSceneGUI(SceneView sceneView)
        {

            if (!isEditing || sceneView.camera == null)
                return;

            ResizeParticleArrays();

            Event e = Event.current;

            if (e.type == EventType.Repaint)
            {

                // Update camera facing status and world space positions array:
                UpdateParticleVisibility();

                // Generate sorted indices for back-to-front rendering:
                for (int i = 0; i < sortedIndices.Length; i++)
                    sortedIndices[i] = i;
                Array.Sort<int>(sortedIndices, (a, b) => sqrDistanceToCamera[b].CompareTo(sqrDistanceToCamera[a]));

                // render modes OnSceneRepaint:
                for (int i = 0; i < renderModes.Count; ++i)
                {
                    if ((1 << i & renderModeFlags) != 0)
                        renderModes[i].OnSceneRepaint(sceneView);
                }

                // property OnSceneRepaint:
                currentProperty.OnSceneRepaint();

                // update particle color based on visiblity, etc.
                UpdateTintColor();

                // Draw particle handles:
                ObiParticleEditorDrawing.DrawParticles(sceneView.camera, blueprint, activeParticle, visible, tint, sortedIndices, dotRadiusScale);

            }

            if (currentTool != null)
                currentTool.OnSceneGUI(sceneView);

        }

        protected virtual void UpdateTintColor()
        {
            Color regularColor = ObiEditorSettings.GetOrCreateSettings().particleColor;
            Color selectedColor = ObiEditorSettings.GetOrCreateSettings().selectedParticleColor;
            Color activeColor = ObiEditorSettings.GetOrCreateSettings().activeParticleColor;

            for (int i = 0; i < blueprint.positions.Length; i++)
            {
                // get particle color:
                if (activeParticle == i)
                    tint[i] = activeColor;
                else
                    tint[i] = selectionStatus[i] ? selectedColor : regularColor;

                tint[i].a = visible[i] ? 1 : 0.15f;
            }
        }

        protected void ResizeParticleArrays()
        {
            if (blueprint.positions != null)
            {
                Array.Resize(ref selectionStatus, blueprint.positions.Length);
                Array.Resize(ref visible, blueprint.positions.Length);
                Array.Resize(ref tint, blueprint.positions.Length);
                Array.Resize(ref sqrDistanceToCamera, blueprint.positions.Length);
                Array.Resize(ref sortedIndices, blueprint.positions.Length);
            }

        }

        public int PropertySelector(int propertyIndex, string label = "Property")
        {
            // get all particle properties:
            string[] propertyNames = new string[properties.Count];
            for (int i = 0; i < properties.Count; ++i)
                propertyNames[i] = properties[i].name;

            // Draw a selection dropdown:
            return EditorGUILayout.Popup(label, propertyIndex, propertyNames);
        }

        public virtual void RenderModeSelector()
        {
            showRenderModes.value = EditorGUILayout.BeginFoldoutHeaderGroup(showRenderModes, "Render modes");
            if (showRenderModes)
            {
                EditorGUI.BeginChangeCheck();
                for (int i = 0; i < renderModes.Count; ++i)
                {
                    int value = 1 << i;

                    if (EditorGUILayout.Toggle(renderModes[i].name, (value & renderModeFlags) != 0))
                        renderModeFlags |= value;
                    else
                        renderModeFlags &= ~value;
                }
                if (EditorGUI.EndChangeCheck())
                    Refresh();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        public void Refresh()
        {
            currentProperty.RecalculateMinMax();

            // refresh render modes:
            for (int i = 0; i < renderModes.Count; ++i)
            {
                if ((1 << i & renderModeFlags) != 0)
                    renderModes[i].Refresh();
            }

            SceneView.RepaintAll();
        }

        public virtual void UpdateParticleVisibility()
        {

            for (int i = 0; i < blueprint.positions.Length; i++)
            {
                if (blueprint.IsParticleActive(i))
                {
                    visible[i] = true;

                    if (Camera.current != null)
                    {
                        Vector3 camToParticle = Camera.current.transform.position - blueprint.positions[i];
                        sqrDistanceToCamera[i] = camToParticle.sqrMagnitude;
                    }
                }
            }

            if ((renderModeFlags & 1) != 0)
                Refresh();
        }

        protected void DrawTools()
        {

            GUIContent[] contents = new GUIContent[tools.Count];

            for (int i = 0; i < tools.Count; ++i)
                contents[i] = new GUIContent(tools[i].icon, tools[i].name);

            EditorGUILayout.Space();
            GUILayout.Box(GUIContent.none, ObiEditorUtils.GetSeparatorLineStyle());
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
            EditorGUI.BeginChangeCheck();
            int newSelectedTool = ObiEditorUtils.DoToolBar(currentToolIndex, contents);
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                if (currentTool != null)
                    currentTool.OnDisable();

                currentToolIndex = newSelectedTool;

                if (currentTool != null)
                    currentTool.OnEnable();

                SceneView.RepaintAll();
            }

            if (currentTool != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
                EditorGUILayout.LabelField(currentTool.name, EditorStyles.boldLabel);

                string help = currentTool.GetHelpString();
                if (!help.Equals(string.Empty))
                    EditorGUILayout.LabelField(help, EditorStyles.helpBox);
                EditorGUILayout.EndVertical();

                currentTool.OnInspectorGUI();
            }

        }

        public void SetSelected(int particleIndex, bool selected)
        {
            selectionStatus[particleIndex] = selected;
        }

        public bool IsSelected(int particleIndex)
        {
            return selectionStatus[particleIndex];
        }

        public bool Editable(int particleIndex)
        {
            return currentTool.Editable(particleIndex) && blueprint.IsParticleActive(particleIndex);
        }
    }

}