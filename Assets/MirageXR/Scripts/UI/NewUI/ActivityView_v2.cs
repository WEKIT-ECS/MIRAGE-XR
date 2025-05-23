using DG.Tweening;
using MirageXR;
using System.Collections;
using System.Collections.Generic;
using LearningExperienceEngine.DataModel;
using UnityEngine;
using UnityEngine.UI;

public class ActivityView_v2 : BaseView
{
    private const float HIDED_SIZE = 100f;
    private const float HIDED_SIZE_FOR_HORIZONTAL_SCROLL = 230f;
    private const float HIDE_ANIMATION_TIME = 0.5f;

    private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;

    [SerializeField] private Button _btnArrow;
    [SerializeField] private RectTransform _panel;
    [SerializeField] private GameObject _arrowDown;
    [SerializeField] private GameObject _arrowUp;
    [SerializeField] private SwitchToggleWithText _toggleEdit;
    [SerializeField] private StepsListView_v2 _stepsListView;
    [SerializeField] private ContentListView_v2 _contentListView;
    [Space]
    [SerializeField] private GameObject _stepsVertical;
    [SerializeField] private GameObject _stepsHorizontal;
    [SerializeField] private GameObject _header;
    [SerializeField] private RectTransform _tabs;
    [SerializeField] private Toggle _toggleSteps;
    [SerializeField] private RectTransform _contentHorizontal;

    private LearningExperienceEngine.SessionContainer _container;
    private int _infoStepNumber;
    private Vector2 _panelSize;
    private Activity _activity;

    public StepsListView_v2 stepsListView => _stepsListView;

    public LearningExperienceEngine.SessionContainer container => _container;

    private RootView_v2 rootView => (RootView_v2)_parentView;

    public RectTransform Tabs => _tabs;

    public Button BtnArrow => _btnArrow;

    public override void Initialization(BaseView parentView)
    {
        base.Initialization(parentView);

        _stepsListView.gameObject.SetActive(true);
        _contentListView.gameObject.SetActive(false);

        _toggleEdit.onValueChanged.AddListener(OnEditToggleValueChanged);

        _btnArrow.onClick.AddListener(OnArrowButtonPressed);

        _arrowDown.SetActive(true);
        _arrowUp.SetActive(false);

        _contentListView.Initialization(this);
        _stepsListView.Initialization(this);
        _panelSize = _panel.sizeDelta;

        RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged += OnEditModeChanged;
        RootObject.Instance.LEE.ContentManager.OnContentActivated += OnContentActivated;
        RootObject.Instance.LEE.ActivityManager.OnActivityLoaded += OnActivityUpdated;

        _stepsVertical.SetActive(true);
        _stepsHorizontal.SetActive(false);

        UpdateView();
    }

    /*private void OnDestroy()
    {
        RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged -= OnEditModeChanged;
        RootObject.Instance.LEE.ContentManager.OnContentActivated -= OnContentActivated;
    }*/

    private void OnActivityUpdated(Activity activity)
    {
        _activity = activity;
    }

    private void UpdateView()
    {
    }

    private void OnContentActivated(List<Content> contents)
    {
        
    }

    public void OnBackToHomePressed()
    {
        rootView.ShowHomeView();
        rootView.bottomPanelView.Show();
        rootView.bottomNavigationArrowsView.Hide();
    }

    private void OnEditToggleValueChanged(bool value)
    {
        RootObject.Instance.LEE.ActivityManager.IsEditorMode = value;
        //activityManager.EditModeActive = value;
        ShowStepsList();
    }

    private void OnEditModeChanged(bool value)
    {
        _toggleEdit.SetIsOnWithoutNotify(value);
        _toggleEdit.ForceUpdateView();
        /*_toggleEdit.onValueChanged.RemoveListener(OnEditToggleValueChanged);
        _toggleEdit.isOn = activityManager.EditModeActive;
        _toggleEdit.onValueChanged.AddListener(OnEditToggleValueChanged);*/
    }

    public void SetSessionInfo(LearningExperienceEngine.SessionContainer info)
    {
        _container = info;
    }

    public void ShowStepContent()
    {
        _contentListView.gameObject.SetActive(true);
        _stepsListView.gameObject.SetActive(false);
    }

    public void ShowStepsList()
    {
        _contentListView.gameObject.SetActive(false);
        _stepsListView.gameObject.SetActive(true);
    }

    private void OnArrowButtonPressed()
    {
        if (_arrowDown.activeSelf)
        {
            var hidedSize = HIDED_SIZE;
            if (_toggleSteps.isOn)
            {
                hidedSize = HIDED_SIZE_FOR_HORIZONTAL_SCROLL;
            }

            _panel.DOSizeDelta(new Vector2(_panelSize.x, -_panel.rect.height + hidedSize), HIDE_ANIMATION_TIME);
            _arrowDown.SetActive(false);
            _arrowUp.SetActive(true);
            rootView.bottomPanelView.Hide();
            rootView.bottomNavigationArrowsView.Show();
            StartCoroutine(ShowHorizontalScroll(HIDE_ANIMATION_TIME, true));
        }
        else
        {
            _panel.DOSizeDelta(_panelSize, HIDE_ANIMATION_TIME);
            _arrowDown.SetActive(true);
            _arrowUp.SetActive(false);
            rootView.bottomPanelView.Show();
            rootView.bottomNavigationArrowsView.Hide();
            StartCoroutine(ShowHorizontalScroll(0.1f, false));
        }
    }

    private IEnumerator ShowHorizontalScroll(float delay, bool value)
    {
        if (_toggleSteps.isOn && _stepsListView.gameObject.activeInHierarchy)
        {
            yield return new WaitForSeconds(delay);

            _stepsVertical.SetActive(!value);
            _stepsHorizontal.SetActive(value);
            _header.SetActive(!value);

            if (value)
            {
                _stepsListView.MoveStepsToHorizontalScroll();
                _tabs.offsetMax = new Vector2(0f, 0f);
                _contentHorizontal.anchoredPosition = Vector3.zero; // the fix when horizontal content becomes NaN with minimazing
            }
            else
            {
                _stepsListView.MoveStepsToVerticalScroll();
                _tabs.offsetMax = new Vector2(0, -300f);
            }
        }
    }

    public void ActivateNextAction()
    {
        RootObject.Instance.LEE.StepManager.GoToNextStep();
    }

    public void ActivatePreviousAction()
    {
        RootObject.Instance.LEE.StepManager.GoToPreviousStep();
    }
}
