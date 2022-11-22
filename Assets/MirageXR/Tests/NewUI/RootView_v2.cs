using System.Threading.Tasks;
using MirageXR;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class RootView_v2 : BaseView
{
    public static RootView_v2 Instance { get; private set; }

    [SerializeField] private Toggle _toggleHome;
    [SerializeField] private Button _btnProfile;
    [SerializeField] private Button _btnAddAugmentation;
    [SerializeField] private PageView_v2 _pageView;
    [SerializeField] private CalibrationGuideView _calibrationGuideViewPrefab;
    [SerializeField] public SearchView _searchPrefab;
    [SerializeField] private ProfileView _profilePrefab;
    [SerializeField] private HelpView _helpPrefab;
    [SerializeField] private SettingsView_v2 _activitySettingsPrefab;
    [SerializeField] private RectTransform _bottomPanel;
    [SerializeField] private LoginView_v2 _loginViewPrefab;
    [SerializeField] private GameObject _newActivityPanel;
    [SerializeField] private ActivityListView_v2 _activityListView;
    [SerializeField] private ActivityView_v2 _activityView;
    [Space]
    [SerializeField] private Dialog _dialog;

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

    public Dialog dialog => _dialog;

    public GameObject newActivityPanel => _newActivityPanel;

    public RectTransform bottomPanel => _bottomPanel;

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
        EventManager.onMobilePageNumberChanged += updatePageNumber;

        _toggleHome.isOn = true;
        _toggleHome.onValueChanged.AddListener(OnStepsClick);
        _btnProfile.onClick.AddListener(OnProfileClick);
        _btnAddAugmentation.onClick.AddListener(AddAugmentation);
        _pageView.OnPageChanged.AddListener(OnPageChanged);

        EventManager.OnActivityStarted += OnActivityLoaded;

        _activityView.Initialization(this);
        _activityListView.Initialization(this);

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
        //_toggleView.interactable = true;
        //_toggleSteps.interactable = true;
        //_toggleView.isOn = true;

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
                _toggleHome.isOn = true;
                _btnAddAugmentation.gameObject.SetActive(true);
                EventManager.NotifyMobilePageNumberChanged(HelpPage.Home);
                break;
            case 1:
                _toggleHome.isOn = true;
                _btnAddAugmentation.gameObject.SetActive(false);
                EventManager.NotifyMobilePageNumberChanged(HelpPage.ActivitySteps);
                break;
        }
    }

    public void OnActivityLoaded()
    {
        _pageView.currentPageIndex = 1;
    }

    private void OnStepsClick(bool value)
    {
        if (value)
        {
            _pageView.currentPageIndex = 0;
        }
    }

    private void OnProfileClick()
    {
        PopupsViewer.Instance.Show(_profilePrefab);
    }

    public void OnHomeClick(bool value)
    {
        if (value)
        {
            _pageView.currentPageIndex = 1;
        }
    }

    private void AddAugmentation()
    {
        _pageView.currentPageIndex = 1;
    }

    public void OnSearchClick()
    {
        PopupsViewer.Instance.Show(_searchPrefab, _activityListView);
    }

    public void OnInfoClick()
    {
        Debug.Log("PAGE INDEX = " +_pageView.currentPageIndex);
        TutorialManager.Instance.showHelpSelection(helpPage);
    }

    public void OnActivitySettingsClick()
    {
        PopupsViewer.Instance.Show(_activitySettingsPrefab);
    }

    public void OnBackToHome()
    {
        _pageView.currentPageIndex = 0;
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

    private void updatePageNumber(HelpPage page)
    {
        helpPage = page;
    }
}
