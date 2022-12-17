using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using MirageXR;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class RootView_v2 : BaseView
{
    public static RootView_v2 Instance { get; private set; }

    [SerializeField] private BottomPanelView _bottomPanelView;
    [SerializeField] private BottomNavigationArrowsView _bottomNavigationArrowsView;
    [SerializeField] private PageView_v2 _pageView;
    [SerializeField] private CalibrationGuideView _calibrationGuideViewPrefab;
    [SerializeField] public SearchView _searchPrefab;
    [SerializeField] private ProfileView _profilePrefab;
    [SerializeField] private HelpView _helpPrefab;
    [SerializeField] private SettingsView_v2 _activitySettingsPrefab;
    [SerializeField] private LoginView_v2 _loginViewPrefab;
    [SerializeField] private ActivityListView_v2 _activityListView;
    [SerializeField] private ActivityView_v2 _activityView;
    [Space]
    [SerializeField] private Dialog _dialog;
    [SerializeField] private Tutorial _tutorial;

    public enum HelpPage {
        Home,
        ActivitySteps,
        ActivityInfo,
        ActivityCalibration,
        ActionAugmentations,
        ActionInfo,
        ActionMarker,
    };

    private static HelpPage helpPage;

    public ActivityListView_v2 activityListView => _activityListView;

    public ActivityView_v2 activityView => _activityView;

    public BottomPanelView bottomPanelView => _bottomPanelView;

    public BottomNavigationArrowsView bottomNavigationArrowsView => _bottomNavigationArrowsView;

    public Dialog dialog => _dialog;

    public Tutorial Tutorial => _tutorial;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"{Instance.GetType().FullName} must only be a single copy!");
            return;
        }

        Instance = this;
    }

    private async void Start()
    {
        if (PlatformManager.GetDeviceFormat() == PlatformManager.DeviceFormat.Tablet)
        {
            await SetupViewForTablet();
        }

        Initialization(null);
    }

    public override async void Initialization(BaseView parentView)
    {
        base.Initialization(parentView);
        EventManager.OnWorkplaceLoaded += OnWorkplaceLoaded;
        EventManager.onMobileHelpPageChanged += updateHelpPage;


        _bottomPanelView.Initialization(this);
        _bottomNavigationArrowsView.Initialization(this);
        _activityView.Initialization(this);
        _activityListView.Initialization(this);

        _bottomPanelView.SetHomeActive(true);
        _bottomNavigationArrowsView.HideImmediate();

        _pageView.OnPageChanged.AddListener(OnPageChanged);

        EventManager.OnWorkplaceLoaded += OnWorkplaceLoaded;
        EventManager.OnActivityStarted += OnActivityLoaded;

        if (!DBManager.LoggedIn && DBManager.rememberUser)
        {
            await AutoLogin();
        }

        if (!DBManager.LoggedIn)
        {
            PopupsViewer.Instance.Show(_loginViewPrefab);
        }
    }

    private void OnDestroy()
    {
        EventManager.OnWorkplaceLoaded -= OnWorkplaceLoaded;
    }

    private void OnWorkplaceLoaded()
    {
        if (!DBManager.dontShowCalibrationGuide)
        {
            PopupsViewer.Instance.Show(_calibrationGuideViewPrefab);
        }
    }

    private async Task AutoLogin()
    {
        if (!LocalFiles.TryToGetUsernameAndPassword(out var username, out var password)) return;

        LoadView.Instance.Show();
        await RootObject.Instance.moodleManager.Login(username, password);
        LoadView.Instance.Hide();
    }

    private void OnPageChanged(int index)
    {
        switch (index)
        {
            case 0:
                _bottomPanelView.SetHomeActive(true);
                EventManager.NotifyMobileHelpPageChanged(HelpPage.Home);
                break;
            case 1:
                _bottomPanelView.SetHomeActive(false);
                EventManager.NotifyMobileHelpPageChanged(HelpPage.ActivitySteps);
                break;
        }
    }

    public void OnActivityLoaded()
    {
        _pageView.currentPageIndex = 1;
    }

    public void ShowBaseView()
    {
        _pageView.gameObject.SetActive(true);
        _bottomPanelView.Show();
    }

    public void HideBaseView()
    {
        _pageView.gameObject.SetActive(false);
        _bottomPanelView.Hide();
    }

    public void ShowProfileView()
    {
        PopupsViewer.Instance.Show(_profilePrefab);
    }

    public async void CreateNewActivity()
    {
        LoadView.Instance.Show();
        await RootObject.Instance.editorSceneService.LoadEditorAsync();
        await RootObject.Instance.activityManager.CreateNewActivity();
        _pageView.currentPageIndex = 1;
        LoadView.Instance.Hide();
    }

    public void ShowHomeView()
    {
        _pageView.currentPageIndex = 0;
    }

    public void OnActivityDeleted()
    {
        ShowHomeView();
        _activityListView.HideBackButtons();
    }

    public void ShowSearchView()
    {
        PopupsViewer.Instance.Show(_searchPrefab, _activityListView);
    }

    public void ShowHelpView()
    {

        TutorialManager.Instance.ShowHelpSelection(helpPage);

        //PopupsViewer.Instance.Show(_helpPrefab);
    }

    public void OnActivitySettingsClick()
    {
        PopupsViewer.Instance.Show(_activitySettingsPrefab);
    }

    private async Task SetupViewForTablet()
    {
        const float scale = 0.001f;
        const float zPosition = 1.5f;
        const string layerName = "MobileUI";
        const string cameraName = "ViewCamera";

        var mainCamera = Camera.main;
        if (!mainCamera)
        {
            return;
        }

        var canvas = GetComponent<Canvas>();
        canvas.enabled = false;
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        await WaitForLandscapeOrientation();

        mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer(layerName));
        var obj = new GameObject(cameraName);
        var viewCamera = obj.AddComponent<Camera>();
        viewCamera.clearFlags = CameraClearFlags.Depth;
        viewCamera.cullingMask = LayerMask.GetMask(layerName);
        var rectTransform = (RectTransform)transform;
        rectTransform.SetParent(viewCamera.transform);
        canvas.worldCamera = viewCamera;
        canvas.renderMode = RenderMode.WorldSpace;
        var bottomLeftPosition = viewCamera.ScreenToWorldPoint(new Vector3(0, 0, zPosition));
        var viewSize = viewCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, zPosition)) - bottomLeftPosition;
        var size = new Vector2(viewSize.y * 0.5f / scale, viewSize.y / scale);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
        rectTransform.localScale = new Vector3(scale, scale, scale);
        var position = viewCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height * 0.5f, zPosition));
        position.x -= size.x * 0.5f * scale;
        rectTransform.position = position;
        canvas.enabled = true;
    }

    private static async Task WaitForLandscapeOrientation()
    {
        while (Screen.width < Screen.height)
        {
            await Task.Yield();
        }
    }

    private void updateHelpPage(HelpPage page)
    {
        helpPage = page;
    }
}
