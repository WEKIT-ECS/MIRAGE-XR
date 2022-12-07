using DG.Tweening;
using MirageXR;
using UnityEngine;
using UnityEngine.UI;

public class ActivityView_v2 : BaseView
{
    private const float HIDED_SIZE = 100f;
    private const float HIDE_ANIMATION_TIME = 0.5f;

    private static ActivityManager activityManager => RootObject.Instance.activityManager;

    [SerializeField] private Button _btnArrow;
    [SerializeField] private RectTransform _panel;
    [SerializeField] private GameObject _arrowDown;
    [SerializeField] private GameObject _arrowUp;
    [SerializeField] private GameObject _backToActivity;
    [SerializeField] private Toggle _toggleEdit;
    [SerializeField] private StepsListView_v2 _stepsListView;
    [SerializeField] private ContentListView_v2 _contentListView;

    private int _infoStepNumber;
    private Vector2 _panelSize;

    public StepsListView_v2 stepsListView => _stepsListView;

    private RootView_v2 rootView => (RootView_v2)_parentView;

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

        EventManager.OnEditModeChanged += OnEditModeChanged;

        UpdateView();
    }

    private void OnDestroy()
    {
        EventManager.OnEditModeChanged -= OnEditModeChanged;
    }

    private void UpdateView()
    {
        _toggleEdit.isOn = activityManager.EditModeActive;
    }

    public void OnBackToHomePressed()
    {
        rootView.OnBackToHome();
        _backToActivity.SetActive(true);
        rootView.bottomPanelView.Show();
        rootView.bottomNavigationArrowsView.Hide();
    }

    private void OnEditToggleValueChanged(bool value)
    {
        activityManager.EditModeActive = value;
    }

    private void OnEditModeChanged(bool value)
    {
        UpdateView();
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
            _panel.DOSizeDelta(new Vector2(_panelSize.x, -_panel.rect.height + HIDED_SIZE), HIDE_ANIMATION_TIME);
            _arrowDown.SetActive(false);
            _arrowUp.SetActive(true);
            rootView.bottomPanelView.Hide();
            rootView.bottomNavigationArrowsView.Show();
        }
        else
        {
            _panel.DOSizeDelta(_panelSize, 0.5f);
            _arrowDown.SetActive(true);
            _arrowUp.SetActive(false);
            rootView.bottomPanelView.Show();
            rootView.bottomNavigationArrowsView.Hide();
        }
    }

    public void ActivateNextAction()
    {
        activityManager.ActivateNextAction();
    }

    public void ActivatePreviousAction()
    {
        activityManager.ActivatePreviousAction();
    }
}
