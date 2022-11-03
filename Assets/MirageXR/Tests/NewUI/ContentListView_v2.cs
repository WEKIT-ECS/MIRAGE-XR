using System.Collections.Generic;
using System.Linq;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContentListView_v2 : BaseView
{
    private static ActivityManager activityManager => RootObject.Instance.activityManager;
    private const float BASE_CONTROLS_COOLDOWN = 0.3f;

    [SerializeField] private TMP_InputField _txtStepName;
    [SerializeField] private TMP_InputField _txtDescription;
    [SerializeField] private Button _btnAddContent;
    [SerializeField] private Button _btnDeleteStep;
    [SerializeField] private Button _btnAddStep;
    [SerializeField] private Button _btnNextStep;
    [SerializeField] private Button _btnPreviousStep;
    [SerializeField] private RectTransform _listContent;
    [SerializeField] private ContentListItem_v2 _contentListItemPrefab;
    [SerializeField] private ContentSelectorView_v2 _contentSelectorViewPrefab;

    [SerializeField] private PopupEditorBase[] _editors;

    public PopupEditorBase[] editors => _editors;
    public MirageXR.Action currentStep => _currentStep;
    public RootView_v2 rootView => (RootView_v2)_parentView;

    public string navigatorId
    {
        get => _navigatorIds.ContainsKey(_currentStep.id) ? _navigatorIds[_currentStep.id] : null;
        set
        {
            if (!_navigatorIds.ContainsKey(_currentStep.id))
            {
                _navigatorIds.Add(_currentStep.id, value);
            }
            else
            {
                _navigatorIds[_currentStep.id] = value;
            }
            UpdateView();
        }
    }

    private readonly Dictionary<string, string> _navigatorIds = new Dictionary<string, string>();
    private readonly List<ContentListItem_v2> _list = new List<ContentListItem_v2>();
    private bool _isShown = true;
    private Coroutine _coroutineSizeTo;
    private Coroutine _coroutineRotateTo;
    private MirageXR.Action _currentStep;

    public override void Initialization(BaseView parentView)
    {
        base.Initialization(parentView);
        _txtStepName.onEndEdit.AddListener(OnStepNameChanged);
        _txtDescription.onEndEdit.AddListener(OnStepDescriptionChanged);
        _btnAddContent.onClick.AddListener(OnAddContent);
        _btnDeleteStep.onClick.AddListener(OnDeleteStep);
        _btnAddStep.onClick.AddListener(OnAddStep);
        _btnNextStep.onClick.AddListener(OnNextStep);
        _btnPreviousStep.onClick.AddListener(OnPreviousStep);

        Canvas.ForceUpdateCanvases();

        OnEditModeChanged(false);

        EventManager.OnActionCreated += OnActionCreated;
        EventManager.OnActivateAction += OnActionActivated;
        EventManager.OnEditModeChanged += OnEditModeChanged;
        EventManager.OnActionModified += OnActionChanged;
    }

    private void OnDestroy()
    {
        EventManager.OnActionCreated -= OnActionCreated;
        EventManager.OnActivateAction -= OnActionActivated;
        EventManager.OnEditModeChanged -= OnEditModeChanged;
        EventManager.OnActionModified -= OnActionChanged;
    }

    private void OnStepNameChanged(string newTitle)
    {
        _currentStep.instruction.title = newTitle;
        EventManager.NotifyOnActionStepTitleChanged();
        EventManager.NotifyActionModified(_currentStep);
    }

    private void OnStepDescriptionChanged(string newDescription)
    {
        _currentStep.instruction.description = newDescription;
        EventManager.NotifyOnActionStepDescriptionInputChanged();
        EventManager.NotifyActionModified(_currentStep);
    }

    private void OnActionActivated(string actionId)
    {
        var action = activityManager.ActiveAction ?? activityManager.ActionsOfTypeAction.FirstOrDefault(t => t.id == actionId);
        if (action != null) _currentStep = action;
        UpdateView();
    }

    private void OnActionCreated(MirageXR.Action action)
    {
        _currentStep = action;
        UpdateView();
    }

    private void OnActionChanged(MirageXR.Action action)
    {
        UpdateView();
    }

    private void OnEditModeChanged(bool value)
    {
        _txtStepName.interactable = value;
        _txtDescription.interactable = value;
        _btnAddContent.gameObject.SetActive(value);
        _btnDeleteStep.gameObject.SetActive(value);
        _btnAddStep.gameObject.SetActive(value);
        _list.ForEach(item => item.OnEditModeChanged(value));
    }

    private void UpdateView()
    {
        _txtStepName.text = _currentStep.instruction.title;
        _txtDescription.text = _currentStep.instruction.description;

        var contents = _currentStep.enter.activates;

        var detailMenu = TaskStationDetailMenu.Instance;
        if (detailMenu)
        {
            detailMenu.NavigatorTarget = null;
        }

        _list.ForEach(t => t.gameObject.SetActive(false));
        for (var i = 0; i < contents.Count; i++)
        {
            if (_list.Count <= i)
            {
                var obj = Instantiate(_contentListItemPrefab, _listContent);
                obj.Init(this);
                _list.Add(obj);
            }
            _list[i].gameObject.SetActive(true);
            _list[i].UpdateView(contents[i]);
        }

        OnEditModeChanged(activityManager.EditModeActive);
    }

    public void OnAddContent()
    {
        PopupsViewer.Instance.Show(_contentSelectorViewPrefab, _editors, _currentStep);
        EventManager.NotifyOnMobileAddStepContentPressed();
    }

    private void EnableBaseControl()
    {
        _btnDeleteStep.interactable = true;
        _btnAddStep.interactable = true;
        _btnNextStep.interactable = true;
        _btnPreviousStep.interactable = true;
    }

    private void DisableBaseControl()
    {
        _btnDeleteStep.interactable = false;
        _btnAddStep.interactable = false;
        _btnNextStep.interactable = false;
        _btnPreviousStep.interactable = false;
    }

    private void OnDeleteStep()
    {
        DisableBaseControl();
        rootView.stepsListView.OnDeleteStepClick(_currentStep);
        Invoke(nameof(EnableBaseControl), BASE_CONTROLS_COOLDOWN);
    }

    private void OnAddStep()
    {
        DisableBaseControl();
        rootView.stepsListView.AddStep();
        Invoke(nameof(EnableBaseControl), BASE_CONTROLS_COOLDOWN);
    }

    private void OnNextStep()
    {
        DisableBaseControl();
        rootView.stepsListView.NextStep();
        Invoke(nameof(EnableBaseControl), BASE_CONTROLS_COOLDOWN);
    }

    private void OnPreviousStep()
    {
        DisableBaseControl();
        rootView.stepsListView.PreviousStep();
        Invoke(nameof(EnableBaseControl), BASE_CONTROLS_COOLDOWN);
    }
}
