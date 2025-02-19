using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Action = System.Action;

public class ContentListView : BaseView
{
    private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;
    private const float HIDE_HEIGHT = 250f;
    private const float BASE_CONTROLS_COOLDOWN = 0.3f;

    [SerializeField] private TMP_InputField _txtStepName;
    [SerializeField] private TMP_InputField _txtDescription;
    [SerializeField] private Button _btnShowHide;
    [SerializeField] private Button _btnAddContent;
    [SerializeField] private Button _btnDeleteStep;
    [SerializeField] private Button _btnAddStep;
    [SerializeField] private Button _btnNextStep;
    [SerializeField] private Button _btnPreviousStep;
    [SerializeField] private RectTransform _panel;
    [SerializeField] private RectTransform _listContent;
    [SerializeField] private ContentListItem _contentListItemPrefab;
    [SerializeField] private ContentSelectorView _contentSelectorViewPrefab;
    [SerializeField] private AnimationCurve _animationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [SerializeField] private float _animationTime = 0.3f;

    [SerializeField] private PopupEditorBase[] _editors;

    public PopupEditorBase[] editors => _editors;
    public LearningExperienceEngine.Action currentStep => _currentStep;
    public RootView rootView => (RootView)_parentView;

    public TMP_InputField TxtStepName => _txtStepName;
    public TMP_InputField TxtStepDescription => _txtDescription;
    public Button BtnShowHide => _btnShowHide;
    public Button BtnAddContent => _btnAddContent;

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
    private readonly List<ContentListItem> _list = new List<ContentListItem>();
    private RectTransform _imdShowHideRectTransform;
    private bool _isShown = true;
    private readonly Quaternion _rotationHide = Quaternion.Euler(new Vector3(0, 0, -90));
    private readonly Quaternion _rotationShow = Quaternion.Euler(new Vector3(0, 0, 90));
    private Coroutine _coroutineSizeTo;
    private Coroutine _coroutineRotateTo;
    private float _showHeight;
    private LearningExperienceEngine.Action _currentStep;

    public override void Initialization(BaseView parentView)
    {
        base.Initialization(parentView);
        _txtStepName.onEndEdit.AddListener(OnStepNameChanged);
        _txtDescription.onEndEdit.AddListener(OnStepDescriptionChanged);
        _btnShowHide.onClick.AddListener(OnShowHideClick);
        _btnAddContent.onClick.AddListener(OnAddContent);
        _btnDeleteStep.onClick.AddListener(OnDeleteStep);
        _btnAddStep.onClick.AddListener(OnAddStep);
        _btnNextStep.onClick.AddListener(OnNextStep);
        _btnPreviousStep.onClick.AddListener(OnPreviousStep);
        _imdShowHideRectTransform = (RectTransform)_btnShowHide.targetGraphic.transform;

        Canvas.ForceUpdateCanvases();
        _showHeight = _panel.rect.height;

        HideContentListImmediate();
        OnEditModeChanged(false);

        LearningExperienceEngine.EventManager.OnActionCreated += OnActionCreated;
        LearningExperienceEngine.EventManager.OnActivateAction += OnActionActivated;
        LearningExperienceEngine.EventManager.OnEditModeChanged += OnEditModeChanged;
        LearningExperienceEngine.EventManager.OnActionModified += OnActionChanged;
    }

    private void OnDestroy()
    {
        LearningExperienceEngine.EventManager.OnActionCreated -= OnActionCreated;
        LearningExperienceEngine.EventManager.OnActivateAction -= OnActionActivated;
        LearningExperienceEngine.EventManager.OnEditModeChanged -= OnEditModeChanged;
        LearningExperienceEngine.EventManager.OnActionModified -= OnActionChanged;
    }

    private void OnStepNameChanged(string newTitle)
    {
        _currentStep.instruction.title = newTitle;
        LearningExperienceEngine.EventManager.NotifyOnActionStepTitleChanged();
        LearningExperienceEngine.EventManager.NotifyActionModified(_currentStep);
    }

    private void OnStepDescriptionChanged(string newDescription)
    {
        _currentStep.instruction.description = newDescription;
        LearningExperienceEngine.EventManager.NotifyOnActionStepDescriptionChanged();
        LearningExperienceEngine.EventManager.NotifyActionModified(_currentStep);
    }

    private void OnActionActivated(string actionId)
    {
        var action = activityManager.ActiveAction ?? activityManager.ActionsOfTypeAction.FirstOrDefault(t => t.id == actionId);
        if (action != null) _currentStep = action;
        UpdateView();
    }

    private void OnActionCreated(LearningExperienceEngine.Action action)
    {
        _currentStep = action;
        UpdateView();
    }

    private void OnActionChanged(LearningExperienceEngine.Action action)
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

    private void OnAddContent()
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

    private void OnShowHideClick()
    {
        _isShown = !_isShown;
        if (_isShown) ShowContentList();
        else HideContentList();
    }

    private void ShowContentList()
    {
        StopCoroutines();
        StartCoroutines(_rotationShow, _showHeight);
    }

    private void HideContentList()
    {
        StopCoroutines();
        StartCoroutines(_rotationHide, HIDE_HEIGHT);
    }

    private void ShowContentListImmediate()
    {
        _isShown = true;
        StopCoroutines();
        ChangeSizeAndRotation(_rotationShow, _showHeight);
    }

    private void HideContentListImmediate()
    {
        _isShown = false;
        StopCoroutines();
        ChangeSizeAndRotation(_rotationHide, HIDE_HEIGHT);
    }

    private void StartCoroutines(Quaternion rotation, float height, Action callback = null)
    {
        _coroutineRotateTo = StartCoroutine(RotateTo(_imdShowHideRectTransform, rotation, _animationTime, _animationCurve));
        _coroutineSizeTo = StartCoroutine(SizeTo(_panel, RectTransform.Axis.Vertical, height, _animationTime, _animationCurve, callback));
    }

    private void ChangeSizeAndRotation(Quaternion rotation, float height)
    {
        _imdShowHideRectTransform.localRotation = rotation;
        _panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }

    private void StopCoroutines()
    {
        if (_coroutineRotateTo != null)
        {
            StopCoroutine(_coroutineRotateTo);
            _coroutineRotateTo = null;
        }

        if (_coroutineSizeTo != null)
        {
            StopCoroutine(_coroutineSizeTo);
            _coroutineSizeTo = null;
        }
    }

    private static IEnumerator SizeTo(RectTransform rectTransform, RectTransform.Axis axis, float sizeEnd, float time, AnimationCurve curve = null, Action callback = null)
    {
        if (curve == null) curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        var rect = rectTransform.rect;
        var sizeStart = axis == RectTransform.Axis.Horizontal ? rect.width : rect.height;
        var timer = 0.0f;
        while (timer < 1.0f)
        {
            timer = Mathf.Min(1.0f, timer + Time.deltaTime / time);
            var value = curve.Evaluate(timer);
            var size = Mathf.Lerp(sizeStart, sizeEnd, value);
            rectTransform.SetSizeWithCurrentAnchors(axis, size);

            yield return null;
        }

        EventManager.NotifyOnMobileStepContentExpanded();
        callback?.Invoke();
    }

    private static IEnumerator RotateTo(Transform transform, Quaternion rotateEnd, float time, AnimationCurve curve = null, Action callback = null)
    {
        if (curve == null) curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        var rotateStart = transform.localRotation;
        var timer = 0.0f;
        while (timer < 1.0f)
        {
            timer = Mathf.Min(1.0f, timer + Time.deltaTime / time);
            var value = curve.Evaluate(timer);
            transform.localRotation = Quaternion.Lerp(rotateStart, rotateEnd, value);

            yield return null;
        }

        callback?.Invoke();
    }
}
