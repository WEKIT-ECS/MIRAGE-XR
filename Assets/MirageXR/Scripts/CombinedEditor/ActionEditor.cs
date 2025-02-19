using LearningExperienceEngine;
using MirageXR;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ActionDetailView))]
public class ActionEditor : MonoBehaviour
{
    private static BrandManager brandManager => LearningExperienceEngine.LearningExperienceEngine.Instance.BrandManager;
    private static ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;

    [SerializeField] private GameObject TaskStationMenuPanel;
    [SerializeField] private GameObject TaskStationOpenButton;
    [SerializeField] private InputField titleField;
    [SerializeField] private InputField descriptionField;
    [SerializeField] private Button addButton;
    [SerializeField] private Button navigationTargetButton;
    [SerializeField] private GameObject poiList;
    [SerializeField] private GameObject annotationAddMenu;
    [SerializeField] private GameObject poiAddItemPrefab;
    [SerializeField] private Button thumbnailButton;
    [SerializeField] private Text instructionText;
    [SerializeField] private Text helpText;

    [SerializeField] private Transform DefaultAugmentationStartingPoint;
    [SerializeField] private Transform editorsSpwnpoint;
    [SerializeField] private GameObject spawnPointVisualPart;

    [SerializeField] private GameObject TextEditorPrefab;
    [SerializeField] private GameObject AudioEditorPrefab;
    [SerializeField] private GameObject ImageEditorPrefab;
    [SerializeField] private GameObject VideoEditorPrefab;
    [SerializeField] private GameObject GhostTrackEditorPrefab;
    [SerializeField] private GameObject GlyphSelectorPrefab;
    [SerializeField] private GameObject VfxEditorPrefab;

    [SerializeField] private GameObject modelSelectorPrefab;
    [SerializeField] private GameObject characterAugmentationPrefab;
    [SerializeField] private GameObject pickAndPlaceEditorPrefab;
    [SerializeField] private GameObject imageMarkerPrefab;
    [SerializeField] private GameObject pluginPrefab;
    [SerializeField] private GameObject drawingEditorPrefab;

    private LabelEditor labelEditor;
    private AudioEditor audioEditor;
    private ImageEditor imageEditor;
    private VideoEditor videoEditor;
    private GhosttrackEditor ghostTrackEditor;
    private GlyphEditor glyphEditor;
    private VFXEditor vfxEditor;
    private DrawingEditor drawingEditor;

    private ModelEditor modelEditor;
    private CharacterAugmentation characterAug;
    private PickAndPlaceEditor pickAndPlaceEditor;
    private ImageMarkerEditor imageMarkerEditor;
    private PluginEditor pluginEditor;

    private ActionDetailView detailView;
    private GameObject[] augmentationsButtons;

    private static ActionEditor instance;

    public static ActionEditor Instance => instance;

    public ActionDetailView DetailView => detailView;


    public bool AddMenuVisible
    {
        get
        {
            return annotationAddMenu.gameObject.activeInHierarchy;
        }
        set
        {
            annotationAddMenu.SetActive(value && activityManager.EditModeActive);
            poiList.SetActive(!value);

            if (TaskStationDetailMenu.Instance && value)
            {
                StepNavigationTargetCapturing = false;
                TaskStationDetailMenu.Instance.SelectedButton = null;
            }

        }
    }

    public bool StepNavigationTargetCapturing
    {
        get; set;
    }

    public (bool, Pick) pickArrowModelCapturing
    {
        get; set;
    }

    private void Awake()
    {
        detailView = GetComponent<ActionDetailView>();

    }

    private void OnEnable()
    {
        LearningExperienceEngine.EventManager.OnEditModeChanged += SetEditModeState;
        LearningExperienceEngine.EventManager.OnDisableAllPoiEditors += DisableAllPoiEditors;
        titleField.onValueChanged.AddListener(OnTitleChanged);
        descriptionField.onValueChanged.AddListener(OnDescriptionChanged);

        if (activityManager != null)
        {
            SetEditModeState(activityManager.EditModeActive);
        }

        if (!RootObject.Instance.PlatformManager.WorldSpaceUi)
        {
            CloseTaskStationMenu();
        }
    }

    private void OnDisable()
    {
        titleField.onValueChanged.RemoveListener(OnTitleChanged);
        descriptionField.onValueChanged.RemoveListener(OnDescriptionChanged);
        LearningExperienceEngine.EventManager.OnEditModeChanged -= SetEditModeState;
        LearningExperienceEngine.EventManager.OnDisableAllPoiEditors -= DisableAllPoiEditors;
    }

    private void Start()
    {
        if (!instance)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        // Get the list of augmentations from txt file depends on platform
        var listOfAugmentations = brandManager.GetListOfAugmentations();


        augmentationsButtons = new GameObject[listOfAugmentations.Count];
        for (var i = 0; i < listOfAugmentations.Count; i++)
        {
            var type = listOfAugmentations[i];
            var addItemInstance = Instantiate(poiAddItemPrefab, annotationAddMenu.transform);
            var listItem = addItemInstance.GetComponent<PoiAddItem>();
            listItem.Initialize(type, i, type.GetName(), type.GetIcon());
            listItem.OnPoiAddItemClicked += OnAnnotationAddItemSelected;
            listItem.OnPoiHover += OnAnnotationHover;

            listItem.GetComponent<ToolTipCaster>().SetTooltipText($"Add {type.GetName()}");

            augmentationsButtons[i] = listItem.gameObject;
        }
        SetEditModeState(activityManager.EditModeActive);

        instructionText.gameObject.SetActive(false);

        navigationTargetButton.onClick.AddListener(OnToggleActionTargetCapture);
        addButton.onClick.AddListener(OnAddButtonToggle);

        if (!RootObject.Instance.PlatformManager.WorldSpaceUi)
        {
            CloseTaskStationMenu();
        }
    }

    public Transform GetDefaultAugmentationStartingPoint()
    {
        return DefaultAugmentationStartingPoint;
    }

    private void OnToggleActionTargetCapture()
    {
        //if there is no annotations
        if (activityManager.ActiveAction.enter.activates.Count == 0)
        {
            StartCoroutine(NavigatorNotification("There is no annotation in this step yet", 5));
            return;
        }

        StepNavigationTargetCapturing = !StepNavigationTargetCapturing;
        navigationTargetButton.GetComponent<Image>().color = StepNavigationTargetCapturing ? Color.red : Color.white;

        if (StepNavigationTargetCapturing)
        {
            StartCoroutine(NavigatorNotification("Now click on an augmentation button", 5));
        }
    }

    /// <summary>
    /// Control the helper text under the target button
    /// </summary>
    /// <param name="msg">The text</param>
    /// <param name="sec">Duration (not used)</param>
    /// <returns>Co-Routine values</returns>
    private IEnumerator NavigatorNotification(string msg, int sec)
    {
        navigationTargetButton.GetComponentInChildren<Text>().enabled = true;
        navigationTargetButton.GetComponentInChildren<Text>().text = msg;
        yield return new WaitForSeconds(sec);
        navigationTargetButton.GetComponentInChildren<Text>().text = "";
        navigationTargetButton.GetComponentInChildren<Text>().enabled = false;
    }

    private void CaptureNavigationTarget(LearningExperienceEngine.ToggleObject annotation)
    {
        if (StepNavigationTargetCapturing)
        {
            // remove target from other annotation's state
            activityManager.ActiveAction.enter.activates.ForEach(b =>
            {
                b.state = string.Empty;
            });
            activityManager.ActiveAction.exit.deactivates.ForEach(b =>
            {
                b.state = string.Empty;
            });

            // set the new target
            annotation.state = "target";
            navigationTargetButton.GetComponent<Image>().color = Color.white;
            navigationTargetButton.GetComponentInChildren<Text>().enabled = false;
            StepNavigationTargetCapturing = false;
            TaskStationDetailMenu.Instance.NavigatorTarget = ActionListMenu.CorrectTargetObject(annotation);
            TaskStationDetailMenu.Instance.TargetPredicate = annotation.predicate;

            // show the target icon if annotation is the target of this action
            foreach (var listItem in FindObjectsOfType<AnnotationListItem>())
                listItem.TargetIconVisibility(listItem.DisplayedAnnotation.state == "target");
        }
    }

    public void CapturePickArrowTarget(LearningExperienceEngine.ToggleObject annotation = null, Pick pick = null)
    {
        if (annotation == null || !pick) return;

        if (pickArrowModelCapturing.Item1)
        {
            pick.ChangeModelButton.GetComponent<Image>().color = Color.white;
            pickArrowModelCapturing = (false, null);
            pick.MyModelID = annotation.poi;

            var newModel = GameObject.Find(pick.MyModelID);
            StartCoroutine(SpawnNewPickModel(pick, newModel));
        }
    }

    // TODO: shouldn't this move to the pick&place augmentation, and be called via an event?
    public IEnumerator SpawnNewPickModel(Pick pick, GameObject newModel)
    {
        if (!newModel || !pick) yield break;

        // Delete the old model if exist
        var oldModel = GameObject.Find("ArrowModel_" + pick.GetComponentInParent<PickAndPlaceController>().MyPoi.poi); // TODO: possible NRE
        if (oldModel) Destroy(oldModel);

        // hide the original arrow mesh and colliders
        pick.ArrowRenderer.enabled = false;
        foreach (var collider in pick.GetComponents<Collider>())
        {
            collider.enabled = false;
        }

        // clone the selected model
        var sketchfabModel = newModel.GetComponentInChildren<Model>();

        var modelLoadingCompleted = sketchfabModel.LoadingCompleted;

        // wait until the model is loaded
        while (!modelLoadingCompleted)
        {
            modelLoadingCompleted = sketchfabModel.LoadingCompleted;
            yield return null;
        }

        var newModelClone = Instantiate(sketchfabModel.gameObject, pick.transform.position, pick.transform.rotation);
        newModelClone.transform.SetParent(pick.transform);
        newModelClone.name = "ArrowModel_" + sketchfabModel.MyToggleObject.poi;
        newModelClone.GetComponentInParent<PoiEditor>().EnableBoxCollider(false);

        // TODO: why not?
        // move the model augmentation somewhere invisible(Cannot deactivate it)
        newModel.transform.position = new Vector3(9999, 9999, 9999);

        // destroy all component on the spawn object
        foreach (var comp in newModelClone.GetComponents<Component>())
        {
            if (!(comp is Transform))
            {
                Destroy(comp);
            }
        }
    }

    public void OnTitleChanged(string newTitle)
    {
        if (detailView.DisplayedAction != null)
        {
            detailView.DisplayedAction.instruction.title = newTitle;
            LearningExperienceEngine.EventManager.NotifyActionModified(detailView.DisplayedAction);
            LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.SaveData();
        }
    }

    public void OnDescriptionChanged(string newDescription)
    {
        if (detailView.DisplayedAction != null)
        {
            detailView.DisplayedAction.instruction.description = newDescription;
            LearningExperienceEngine.EventManager.NotifyActionModified(detailView.DisplayedAction);
            LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.SaveData();
        }
    }

    public void OnAddButtonToggle()
    {
        AddMenuVisible = !AddMenuVisible;
    }

    public void OnAnnotationAddItemSelected(ContentType type)
    {
        DisableAllPoiEditors();

        AddMenuVisible = false;

        switch (type)
        {
            case ContentType.LABEL:
                labelEditor = LoadEditorPanel<LabelEditor>(TextEditorPrefab);
                labelEditor.SetAnnotationStartingPoint(DefaultAugmentationStartingPoint);
                labelEditor.Open(detailView.DisplayedAction, null);
                break;
            case ContentType.AUDIO:
                audioEditor = LoadEditorPanel<AudioEditor>(AudioEditorPrefab);
                audioEditor.Open(detailView.DisplayedAction, null);
                break;
            case ContentType.IMAGE:
                imageEditor = LoadEditorPanel<ImageEditor>(ImageEditorPrefab);
                imageEditor.IsThumbnail = false;
                imageEditor.SetAnnotationStartingPoint(DefaultAugmentationStartingPoint);
                imageEditor.Open(detailView.DisplayedAction, null);
                break;
            case ContentType.VIDEO:
                videoEditor = LoadEditorPanel<VideoEditor>(VideoEditorPrefab);
                videoEditor.SetAnnotationStartingPoint(DefaultAugmentationStartingPoint);
                videoEditor.Open(detailView.DisplayedAction, null);
                break;
            case ContentType.GHOST:
                ghostTrackEditor = LoadEditorPanel<GhosttrackEditor>(GhostTrackEditorPrefab);
                ghostTrackEditor.Open(detailView.DisplayedAction, null);
                break;
            case ContentType.ACT:
                glyphEditor = LoadEditorPanel<GlyphEditor>(GlyphSelectorPrefab);
                glyphEditor.SetAnnotationStartingPoint(DefaultAugmentationStartingPoint);
                glyphEditor.Open(detailView.DisplayedAction, null);
                break;
            case ContentType.EFFECTS:
                vfxEditor = LoadEditorPanel<VFXEditor>(VfxEditorPrefab);
                vfxEditor.SetAnnotationStartingPoint(DefaultAugmentationStartingPoint);
                vfxEditor.Open(detailView.DisplayedAction, null);
                break;
            case ContentType.MODEL:
                modelEditor = LoadEditorPanel<ModelEditor>(modelSelectorPrefab);
                modelEditor.SetAnnotationStartingPoint(DefaultAugmentationStartingPoint);
                modelEditor.Open(detailView.DisplayedAction, null);
                break;
            case ContentType.CHARACTER:
                characterAug = LoadEditorPanel<CharacterAugmentation>(characterAugmentationPrefab);
                characterAug.Open(detailView.DisplayedAction, null);
                break;
            case ContentType.PICKANDPLACE:
                pickAndPlaceEditor = LoadEditorPanel<PickAndPlaceEditor>(pickAndPlaceEditorPrefab);
                pickAndPlaceEditor.SetAnnotationStartingPoint(DefaultAugmentationStartingPoint);
                pickAndPlaceEditor.Open(detailView.DisplayedAction, null);
                break;
            case ContentType.IMAGEMARKER:
                imageMarkerEditor = LoadEditorPanel<ImageMarkerEditor>(imageMarkerPrefab);
                imageMarkerEditor.SetAnnotationStartingPoint(DefaultAugmentationStartingPoint);
                imageMarkerEditor.Open(detailView.DisplayedAction, null);
                break;
            case ContentType.PLUGIN:
                pluginEditor = LoadEditorPanel<PluginEditor>(pluginPrefab);
                pluginEditor.SetAnnotationStartingPoint(DefaultAugmentationStartingPoint);
                pluginEditor.Open(detailView.DisplayedAction, null);
                break;
            case ContentType.DRAWING:
                if (!InstanceOfAugmentationExist(ContentType.DRAWING))
                {
                    drawingEditor = LoadEditorPanel<DrawingEditor>(drawingEditorPrefab);
                    drawingEditor.SetAnnotationStartingPoint(DefaultAugmentationStartingPoint);
                    drawingEditor.Open(detailView.DisplayedAction, null);
                }
                break;
        }
    }

    /// <summary>
    /// This function instantiates the relevant prefab and, depending on the augmentation type,
    /// allows the controller component to be returned after configuration.
    /// </summary>
    /// <typeparam name="T">The component which should be searched on the instantiated prefab and which is returned</typeparam>
    /// <param name="prefabToLoad"></param>
    /// <returns>A component of the type specified during the function call.</returns>
    private T LoadEditorPanel<T>(GameObject prefabToLoad) where T : Component
    {
        var newPanel = Instantiate(prefabToLoad);
        newPanel.transform.SetParent(transform.Find("Pivot"));
        newPanel.transform.SetAsLastSibling();
        newPanel.transform.localPosition = editorsSpwnpoint.localPosition;
        newPanel.transform.localRotation = Quaternion.identity;
        newPanel.transform.localEulerAngles = new Vector3(0, 30, 0);
        newPanel.transform.localScale = Vector3.one;
        return newPanel.GetComponent<T>();
    }

    public object CreateEditorView(ContentType type)
    {
        switch (type)
        {
            case ContentType.IMAGE:
                return LoadEditorPanel<ImageEditor>(ImageEditorPrefab);
            case ContentType.AUDIO:
                return LoadEditorPanel<AudioEditor>(AudioEditorPrefab);
            default:
                return null;
        }
    }

    public void OnAnnotationHover(ContentType type, int index)
    {
        helpText.gameObject.SetActive(false);
        instructionText.gameObject.SetActive(true);

        var taskStation = detailView.GetCurrentTaskStation();
        if (!taskStation) return;

        TaskStationDetailMenu.Instance.BindPoiToTaskStation(taskStation.transform, augmentationsButtons[index].transform);

        instructionText.text = type.GetHint();
    }

    public void ShowHelpText()
    {
        instructionText.gameObject.SetActive(false);
        helpText.gameObject.SetActive(true);
    }

    public void EditAnnotation(LearningExperienceEngine.ToggleObject annotation)
    {
        DisableAllPoiEditors();

        CaptureNavigationTarget(annotation);

        // pick and place can only use the model augmentation as it's arrow
        if (annotation.predicate.StartsWith("3d"))
            CapturePickArrowTarget(annotation, pickArrowModelCapturing.Item2);

        switch (annotation.predicate)
        {
            case "label":
                labelEditor = LoadEditorPanel<LabelEditor>(TextEditorPrefab);
                labelEditor.Open(detailView.DisplayedAction, annotation);
                break;
            case "audio":
                audioEditor = LoadEditorPanel<AudioEditor>(AudioEditorPrefab);
                audioEditor.Open(detailView.DisplayedAction, annotation);
                break;
            case "image":
                imageEditor = LoadEditorPanel<ImageEditor>(ImageEditorPrefab);
                imageEditor.Open(detailView.DisplayedAction, annotation);
                break;
            case "video":
                videoEditor = LoadEditorPanel<VideoEditor>(VideoEditorPrefab);
                videoEditor.Open(detailView.DisplayedAction, annotation);
                break;
            case "ghosttracks":
                ghostTrackEditor = LoadEditorPanel<GhosttrackEditor>(GhostTrackEditorPrefab);
                ghostTrackEditor.Open(detailView.DisplayedAction, annotation);
                break;
            case string anno when anno.StartsWith("act"):
                glyphEditor = LoadEditorPanel<GlyphEditor>(GlyphSelectorPrefab);
                glyphEditor.Open(detailView.DisplayedAction, annotation);
                break;
            case string anno when anno.StartsWith("effect"):
                vfxEditor = LoadEditorPanel<VFXEditor>(VfxEditorPrefab);
                vfxEditor.Open(detailView.DisplayedAction, annotation);
                break;
            case string anno when anno.StartsWith("3d"):
                modelEditor = LoadEditorPanel<ModelEditor>(modelSelectorPrefab);
                modelEditor.Open(detailView.DisplayedAction, annotation);
                break;
            case string anno when anno.StartsWith("pick"):
                pickAndPlaceEditor = LoadEditorPanel<PickAndPlaceEditor>(pickAndPlaceEditorPrefab);
                pickAndPlaceEditor.Open(detailView.DisplayedAction, annotation);
                break;
            case string anno when anno.StartsWith("char"):
                characterAug = LoadEditorPanel<CharacterAugmentation>(characterAugmentationPrefab);
                characterAug.Open(detailView.DisplayedAction, annotation);
                break;
            case "image marker":
                imageMarkerEditor = LoadEditorPanel<ImageMarkerEditor>(imageMarkerPrefab);
                imageMarkerEditor.Open(detailView.DisplayedAction, annotation);
                break;
            case "plugins":
                pluginEditor = LoadEditorPanel<PluginEditor>(pluginPrefab);
                pluginEditor.Open(detailView.DisplayedAction, annotation);
                break;
            default:
                Debug.LogWarning("Unknown annotation predicate");
                break;
        }
    }

    public void DisableAllPoiEditors()
    {

        if (labelEditor)
            labelEditor.Close();
        if (audioEditor)
            audioEditor.Close();
        if (imageEditor)
            imageEditor.Close();
        if (videoEditor)
            videoEditor.Close();
        if (ghostTrackEditor)
            ghostTrackEditor.Close();
        if (glyphEditor)
            glyphEditor.Close();
        if (vfxEditor)
            vfxEditor.Close();
        if (modelEditor)
            modelEditor.Close();
        if (characterAug)
            characterAug.Close();
        if (pickAndPlaceEditor)
            pickAndPlaceEditor.Close();
        if (imageMarkerEditor)
            imageMarkerEditor.Close();
        if (pluginEditor)
            pluginEditor.Close();
        if (drawingEditor)
            drawingEditor.Close();


        ShowHelpText();
    }

    private void SetEditModeState(bool editModeActive)
    {
        titleField.interactable = editModeActive;
        titleField.GetComponent<Image>().enabled = editModeActive;
        descriptionField.readOnly = !editModeActive;
        // active the editor views only if the edit mode is on
        gameObject.GetComponent<Canvas>().enabled = editModeActive;
        addButton.gameObject.SetActive(editModeActive);
        navigationTargetButton.gameObject.SetActive(editModeActive);
        thumbnailButton.interactable = editModeActive;
        // always deactivate menu so that it starts closed and ends closed
        AddMenuVisible = false;

        spawnPointVisualPart.SetActive(editModeActive);

        DisableAllPoiEditors();

        if (!RootObject.Instance.PlatformManager.WorldSpaceUi)
        {
            CloseTaskStationMenu();
        }
    }

    public void OpenTaskStationMenu()
    {
        gameObject.GetComponent<Canvas>().enabled = activityManager.EditModeActive;
        detailView.MoveEditorNextToTaskSTation();
        TaskStationOpenButton.SetActive(false);
        TaskStationMenuPanel.SetActive(true);
    }

    public void CloseTaskStationMenu()
    {
        gameObject.GetComponent<Canvas>().enabled = false;
        TaskStationMenuPanel.SetActive(false);
        TaskStationOpenButton.SetActive(RootObject.Instance.PlatformManager.WorldSpaceUi);
    }

    private bool InstanceOfAugmentationExist(ContentType type)
    {
        var predicate = type.GetPredicate();

        foreach (var toggleObject in activityManager.ActiveAction.enter.activates)
        {
            if (toggleObject.predicate == predicate)
            {
                // give the info and close
                DialogWindow.Instance.Show("Info!",
                $"There is already a {predicate} augmentation in this step. Please delete it before adding a new one!",
                new DialogButtonContent("Ok"));
                return true;
            }
        }

        return false;
    }

}
