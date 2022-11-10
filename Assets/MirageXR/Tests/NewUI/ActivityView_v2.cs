using DG.Tweening;
using MirageXR;
using UnityEngine;
using UnityEngine.UI;

public class ActivityView_v2 : BaseView
{
    private static ActivityManager activityManager => RootObject.Instance.activityManager;

    [Space]
    [SerializeField] private Button _btnArrow;
    [SerializeField] private RectTransform _panel;
    [SerializeField] private GameObject _arrowDown;
    [SerializeField] private GameObject _arrowUp;
    [SerializeField] private Toggle _toggleEdit;
    [SerializeField] private StepsListView_v2 _stepsListView;
    [SerializeField] private ContentListView_v2 _contentListView;

    private int _infoStepNumber;

    public StepsListView_v2 stepsListView => _stepsListView;

    private RootView_v2 rootView => (RootView_v2)_parentView;

    public override void Initialization(BaseView parentView)
    {
        base.Initialization(parentView);

        _stepsListView.gameObject.SetActive(true);
        _contentListView.gameObject.SetActive(false);

        _toggleEdit.onValueChanged.AddListener(OnEditToggleValueChanged);

        _btnArrow.onClick.AddListener(ArrowBtnPressed);
        _arrowDown.SetActive(true);
        _arrowUp.SetActive(false);

        _contentListView.Initialization(this);
        _stepsListView.Initialization(this);

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

    //private void OnStartCalibrationClick()
    //{
    //    PopupsViewer.Instance.Show(_startCalibrationPanel);
    //}

    private void ArrowBtnPressed()
    {
        if (_arrowDown.activeSelf)
        {
            _panel.DOAnchorPos(new Vector2(0, -1100), 0.25f);
            _arrowDown.SetActive(false);
            _arrowUp.SetActive(true);
        }
        else
        {
            _panel.DOAnchorPos(new Vector2(0, -120), 0.25f);
            _arrowDown.SetActive(true);
            _arrowUp.SetActive(false);
        }
    }
}
