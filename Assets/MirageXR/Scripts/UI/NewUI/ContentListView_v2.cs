using System;
using System.Collections.Generic;
using System.Linq;
using LearningExperienceEngine.DataModel;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ContentType = LearningExperienceEngine.ContentType;
using Step = LearningExperienceEngine.Action;

public class ContentListView_v2 : BaseView
{
    private const string STEP_NAME_MASK = "{0}/{1} {2}";

    private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;

    [SerializeField] private Button _btnAddContent;
    [SerializeField] private RectTransform _listContent;
    [SerializeField] private TMP_Text _txtStepTitle;
    [SerializeField] private TMP_InputField _inputFieldStepName;
    [SerializeField] private TMP_InputField _inputFieldDescription;
    [SerializeField] private Button _btnBack;
    [SerializeField] private Button _btnSettings;
    [SerializeField] private Button _btnMarker;
    [SerializeField] private Toggle _toggleAugmentations;
    [SerializeField] private Toggle _toggleInfo;
    [SerializeField] private Toggle _toggleMarker;
    [SerializeField] private Toggle _toggleDiamondVisibility;
    [SerializeField] private GameObject _augmentations;
    [SerializeField] private GameObject _info;
    [SerializeField] private GameObject _marker;
    [SerializeField] private ContentListItem_v2 _contentListItemPrefab;
    [SerializeField] private ContentSelectorView_v2 _contentSelectorViewPrefab;
    [SerializeField] private PopupEditorBase[] _editors;

    public ActivityStep currentStep => _currentStep;

    private ActivityView_v2 _activityView => (ActivityView_v2)_parentView;

    public string navigatorId
    {
        get => _navigatorIds.ContainsKey(_currentStep.Id) ? _navigatorIds[_currentStep.Id] : null;
        set
        {
            if (!_navigatorIds.ContainsKey(_currentStep.Id))
            {
                _navigatorIds.Add(_currentStep.Id, value);
            }
            else
            {
                _navigatorIds[_currentStep.Id] = value;
            }

            UpdateView();
        }
    }

    private readonly Dictionary<Guid, string> _navigatorIds = new Dictionary<Guid, string>();
    private readonly List<ContentListItem_v2> _list = new List<ContentListItem_v2>();
    private bool _isShown = true;
    private Coroutine _coroutineSizeTo;
    private Coroutine _coroutineRotateTo;
    private ActivityStep _currentStep;

    public override void Initialization(BaseView parentView)
    {
        base.Initialization(parentView);

        _augmentations.SetActive(true);
        _info.SetActive(false);
        _marker.SetActive(false);

        _btnAddContent.onClick.AddListener(OnAddContent);
        _btnMarker.onClick.AddListener(OnAddMarkerPressed);
        _btnBack.onClick.AddListener(OnBackPressed);
        _btnSettings.onClick.AddListener(OnSettingsPressed);

        _toggleAugmentations.onValueChanged.AddListener(OnToggleAugmentationsValueChanged);
        _toggleInfo.onValueChanged.AddListener(OnToggleInfoValueChanged);
        _toggleMarker.onValueChanged.AddListener(OnToggleMarkerValueChanged);

        _inputFieldStepName.onEndEdit.AddListener(OnStepNameChanged);
        _inputFieldDescription.onEndEdit.AddListener(OnStepDescriptionChanged);
        _toggleDiamondVisibility.onValueChanged.AddListener(OnDiamondVisibilityChanged);

        RootObject.Instance.LEE.ContentManager.OnContentActivated += OnContentActivated;
        RootObject.Instance.LEE.ContentManager.OnContentUpdated += OnContentUpdated;
        RootObject.Instance.LEE.StepManager.OnStepChanged += OnStepChanged;
        RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged += OnEditModeChanged;

        /*LearningExperienceEngine.EventManager.OnActionCreated += OnActionCreated;
        LearningExperienceEngine.EventManager.OnActionModified += OnActionChanged;

        LearningExperienceEngine.EventManager.OnActivateAction += OnActionActivated;
        LearningExperienceEngine.EventManager.OnEditModeChanged += OnEditModeChanged;*/

    }
    private void OnStepChanged(ActivityStep step)
    {
        _currentStep = step;
        _inputFieldStepName.text = _currentStep.Name;
        _inputFieldDescription.text = _currentStep.Description;
    }

    private void OnContentActivated(List<Content> contents)
    {
        foreach (var item in _list)
        {
            Destroy(item.gameObject);
        }

        _list.Clear();

        foreach (var contentItemView in contents)
        {
            var obj = Instantiate(_contentListItemPrefab, _listContent);
            obj.Initialization(this, OnAnnotationSelected);
            obj.UpdateView(contentItemView);
            _list.Add(obj);
        }
    }

    private void OnContentUpdated(List<Content> contents)
    {
        //TODO:
    }

    private void OnDiamondVisibilityChanged(bool value)
    {
        var taskStation = GameObject.Find(activityManager.ActiveAction.id); //temp
        if (taskStation != null)
        {
            var taskStationEditor = taskStation.GetComponentInChildren<TaskStationEditor>();
            if (taskStationEditor != null)
            {
                taskStationEditor.OnVisibilityChanged(value);
            }
        }
    }

    /*private void OnDestroy()
    {
        LearningExperienceEngine.EventManager.OnActionCreated -= OnActionCreated;
        LearningExperienceEngine.EventManager.OnActionModified -= OnActionChanged;

        LearningExperienceEngine.EventManager.OnActivateAction -= OnActionActivated;
        LearningExperienceEngine.EventManager.OnEditModeChanged -= OnEditModeChanged;
        
    }*/

    /*private void OnActionActivated(string actionId)
    {
        var action = activityManager.ActiveAction ?? activityManager.ActionsOfTypeAction.FirstOrDefault(t => t.id == actionId);
        if (action != null)
        {
            _currentStep = action;
        }

        UpdateView();
    }*/

    /*private void OnActionCreated(Step action)
    {
        _currentStep = action;
        UpdateView();
    }*/

    private void OnActionChanged(Step action)
    {
        UpdateView();
    }

    private void OnBackPressed()
    {
        _activityView.ShowStepsList();
        _augmentations.SetActive(true);
        _info.SetActive(false);
        _marker.SetActive(false);
        //MirageXR.EventManager.NotifyMobileHelpPageChanged(RootView_v2.HelpPage.ActivitySteps);
    }

    private void OnAddMarkerPressed()
    {
        /*var editors = RootView_v2.Instance.editors;
        var editor = editors.FirstOrDefault(t => t.editorForType == ContentType.IMAGEMARKER);
        if (editor == null)
        {
            Debug.LogError("there is no editor for the type ContentType.IMAGEMARKER");
            return;
        }
        PopupsViewer.Instance.Show(editor, _currentStep);

        _toggleAugmentations.isOn = true;*/
    }

    private void OnSettingsPressed()
    {
        RootView_v2.Instance.dialog.ShowBottomMultiline("Settings", ("Delete", OnCurrentStepDelete));
    }

    private void OnCurrentStepDelete()
    {
        _activityView.stepsListView.OnDeleteStepClick(_currentStep, OnBackPressed);
    }

    private void OnToggleAugmentationsValueChanged(bool value)
    {
        _augmentations.SetActive(value);
        _info.SetActive(!value);
        _marker.SetActive(!value);
        MirageXR.EventManager.NotifyMobileHelpPageChanged(RootView_v2.HelpPage.ActionAugmentations);
    }

    private void OnToggleInfoValueChanged(bool value)
    {
        _augmentations.SetActive(!value);
        _info.SetActive(value);
        _marker.SetActive(!value);
        MirageXR.EventManager.NotifyMobileHelpPageChanged(RootView_v2.HelpPage.ActionInfo);
    }

    private void OnToggleMarkerValueChanged(bool value)
    {
        _augmentations.SetActive(!value);
        _info.SetActive(!value);
        _marker.SetActive(value);
        MirageXR.EventManager.NotifyMobileHelpPageChanged(RootView_v2.HelpPage.ActionMarker);
    }

    private void OnEditModeChanged(bool value)
    {
        _btnAddContent.gameObject.SetActive(value);
        //_list.ForEach(item => item.OnEditModeChanged(value));
    }

    public void UpdateView()
    {
        /*_toggleDiamondVisibility.isOn = _currentStep.isDiamondVisible ?? true;
        _toggleDiamondVisibility.onValueChanged.Invoke(_currentStep.isDiamondVisible ?? true);
*/
        /*int currentIndex = activityManager.ActionsOfTypeAction.IndexOf(_currentStep) + 1;
        int maxIndex = activityManager.ActionsOfTypeAction.Count;
*/
        /*_txtStepTitle.text = string.Format(STEP_NAME_MASK, currentIndex, maxIndex, _currentStep.instruction.title);
        _inputFieldStepName.text = _currentStep.instruction.title;
        _inputFieldDescription.text = _currentStep.instruction.description;
*/
        //var contents = _currentStep.enter.activates;

        /*var detailMenu = TaskStationDetailMenu.Instance;
        if (detailMenu)
        {
            detailMenu.NavigatorTarget = null;
        }*/

        /*_list.ForEach(t => t.gameObject.SetActive(false));
        for (var i = 0; i < contents.Count; i++)
        {
            if (_list.Count <= i)
            {
                var obj = Instantiate(_contentListItemPrefab, _listContent);
                obj.Initialization(this, OnAnnotationSelected);
                _list.Add(obj);
            }

            _list[i].gameObject.SetActive(true);
            _list[i].UpdateView(contents[i]);
        }*/
    }

    public void OnAddContent()
    {
        PopupsViewer.Instance.Show(_contentSelectorViewPrefab, RootView_v2.Instance.editors, _currentStep);
    }

    private void OnStepNameChanged(string newTitle)
    {
        RootObject.Instance.LEE.StepManager.SetStepName(_currentStep.Id, newTitle);
    }

    private void OnStepDescriptionChanged(string newDescription)
    {
        RootObject.Instance.LEE.StepManager.SetStepDescription(_currentStep.Id, newDescription);
    }

    private void OnAnnotationSelected(Content content)
    {
        //ActionEditor.Instance.CapturePickArrowTarget(content, ActionEditor.Instance.pickArrowModelCapturing.Item2);
    }
}
