using System;
using System.Linq;
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

    public static event System.Action<bool> OnMenuStateChanged;
    public bool IsMenuMinimized => _arrowUp != null && _arrowUp.activeSelf;
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
        RootObject.Instance.LEE.ActivityManager.OnActivityUpdated += OnActivityUpdated;

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
        UpdateEditToggleState();
    }

    private bool CheckIsCreator()
    {
        if (_activity == null) return false;
        var auth = RootObject.Instance.LEE.AuthorizationManager;
        if (auth == null) return false;

        return (!string.IsNullOrEmpty(auth.UserSub) &&
            ((_activity.Creator != null && string.Equals(_activity.Creator.Id.ToString(), auth.UserSub, StringComparison.OrdinalIgnoreCase)) ||
             (!string.IsNullOrEmpty(_activity.Owner) && string.Equals(_activity.Owner, auth.UserSub, StringComparison.OrdinalIgnoreCase)))) ||
            (!string.IsNullOrEmpty(auth.UserName) && !string.IsNullOrEmpty(_activity.Owner) && string.Equals(_activity.Owner, auth.UserName, StringComparison.OrdinalIgnoreCase)) ||
            (!string.IsNullOrEmpty(auth.UserFullName) && _activity.Creator != null && string.Equals(_activity.Creator.Name, auth.UserFullName, StringComparison.OrdinalIgnoreCase));
    }

    private bool HasScopePrivilege(string privilege)
    {
        var auth = RootObject.Instance.LEE.AuthorizationManager;
        if (auth == null) return false;
        if (auth.HasScopePrivilege("*", privilege)) return true;
        if (auth.UserPrivileges != null && auth.UserPrivileges.Any(p => string.Equals(p, privilege, StringComparison.OrdinalIgnoreCase) || string.Equals(p, "admin", StringComparison.OrdinalIgnoreCase))) return true;
        string scope = string.IsNullOrWhiteSpace(_activity?.CustomerScope) ? "wekit" : _activity.CustomerScope;
        return auth.HasScopePrivilege(scope, privilege);
    }

    private void UpdateEditToggleState()
    {
        if (_toggleEdit == null) return;
        if (_activity == null)
        {
            _toggleEdit.gameObject.SetActive(true);
            return;
        }

        var auth = RootObject.Instance.LEE.AuthorizationManager;
        if (auth == null)
        {
            _toggleEdit.gameObject.SetActive(true);
            return;
        }

        bool isCreator = CheckIsCreator();
        bool isSystemAdmin = auth.HasScopePrivilege("*", "update");
        bool hasCustomerUpdate = HasScopePrivilege("update");

        // ND restriction: If NoDerivatives is true, only creator, system admin, or users with scope update privilege can edit.
        bool blockedByND = (_activity.License != null && _activity.License.NoDerivatives) && !isCreator && !isSystemAdmin && !hasCustomerUpdate;

        bool canUpdate = isCreator || hasCustomerUpdate || _activity.EffectivePermissions == null || _activity.EffectivePermissions.Update;
        bool canShare = isCreator || HasScopePrivilege("share") || HasScopePrivilege("admin") || _activity.EffectivePermissions == null || _activity.EffectivePermissions.Share;
        bool canCreate = auth.CanCreateActivity || HasScopePrivilege("create") || _activity.EffectivePermissions == null || _activity.EffectivePermissions.Create;

        bool canEdit = (canUpdate || canShare || canCreate) && !blockedByND;

        _toggleEdit.gameObject.SetActive(canEdit);
        if (!canEdit && RootObject.Instance.LEE.ActivityManager.IsEditorMode)
        {
            RootObject.Instance.LEE.ActivityManager.IsEditorMode = false;
        }
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
        if (value)
        {
            ExpandMenu();
        }
    }

    private void OnEditModeChanged(bool value)
    {
        _toggleEdit.SetIsOnWithoutNotify(value);
        _toggleEdit.ForceUpdateView();
        /*_toggleEdit.onValueChanged.RemoveListener(OnEditToggleValueChanged);
        _toggleEdit.isOn = activityManager.EditModeActive;
        _toggleEdit.onValueChanged.AddListener(OnEditToggleValueChanged);*/
        if (value)
        {
            ExpandMenu();
        }
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

    public void MinimizeMenu()
    {
        if (PlatformManager.GetDeviceFormat() == DeviceFormat.Tablet)
        {
            return;
        }

        if (_arrowDown != null && _arrowDown.activeSelf)
        {
            OnArrowButtonPressed();
        }
    }

    public void ExpandMenu()
    {
        if (_arrowUp != null && _arrowUp.activeSelf)
        {
            OnArrowButtonPressed();
        }
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
            OnMenuStateChanged?.Invoke(true);
        }
        else
        {
            _panel.DOSizeDelta(_panelSize, HIDE_ANIMATION_TIME);
            _arrowDown.SetActive(true);
            _arrowUp.SetActive(false);
            rootView.bottomPanelView.Show();
            rootView.bottomNavigationArrowsView.Hide();
            StartCoroutine(ShowHorizontalScroll(0.1f, false));
            OnMenuStateChanged?.Invoke(false);
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
