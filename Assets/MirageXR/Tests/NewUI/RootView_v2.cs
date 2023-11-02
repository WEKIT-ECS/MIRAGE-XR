using System.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
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
    [SerializeField] private SearchView _searchPrefab;
    [SerializeField] private ProfileView _profilePrefab;
    [SerializeField] private LoginView_v2 _loginViewPrefab;
    [SerializeField] private ActivityListView_v2 _activityListView;
    [SerializeField] private ActivityView_v2 _activityView;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private ViewCamera _viewCamera;
    [Space]
    [SerializeField] private Dialog _dialog;
    [SerializeField] private Tutorial _tutorial;

    public enum HelpPage
    {
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

    public ViewCamera viewCamera => _viewCamera;

    public Canvas canvas => _canvas;

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
        await _viewCamera.SetupFormat(PlatformManager.GetDeviceFormat());
        Initialization(null);
        EventManager.OnEditModeChanged += EditModeChangedForHelp;
    }

    public override async void Initialization(BaseView parentView)
    {
        base.Initialization(parentView);

        _bottomPanelView.Initialization(this);
        _bottomNavigationArrowsView.Initialization(this);
        _activityView.Initialization(this);
        _activityListView.Initialization(this);

        _bottomNavigationArrowsView.HideImmediate();

        _pageView.OnPageChanged.AddListener(OnPageChanged);

        EventManager.OnWorkplaceLoaded += OnWorkplaceLoaded;
        EventManager.OnActivityStarted += OnActivityLoaded;
        EventManager.OnMobileHelpPageChanged += UpdateHelpPage;

        if (!DBManager.LoggedIn && DBManager.rememberUser)
        {
            await AutoLogin();
        }

        if (!DBManager.LoggedIn)
        {
            var dontShowLoginMenu = false;
            PopupsViewer.Instance.Show(_loginViewPrefab, dontShowLoginMenu, null);
        }

        RootObject.Instance.cameraCalibrationChecker.onAnchorLost.AddListener(ShowCalibrationAlert);
    }

    private void OnDestroy()
    {
        EventManager.OnWorkplaceLoaded -= OnWorkplaceLoaded;
        EventManager.OnActivityStarted -= OnActivityLoaded;
        EventManager.OnMobileHelpPageChanged -= UpdateHelpPage;
        RootObject.Instance.cameraCalibrationChecker.onAnchorLost.RemoveListener(ShowCalibrationAlert);
    }

    private void OnWorkplaceLoaded()
    {
        if (!DBManager.dontShowCalibrationGuide && !_tutorial.isActivated)
        {
            PopupsViewer.Instance.Show(_calibrationGuideViewPrefab);
        }
    }

    private void ShowCalibrationAlert(float distance)
    {
        Toast.Instance.Show("The space anchor may have shifted.\nPlease check the position of the anchor.");
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
        activityListView.FetchAndUpdateView();
        _pageView.currentPageIndex = 0;
    }

    public void ReturnToHomePage()
    {
        // select Home icon/text and deselect others
        _bottomPanelView.OnHomeClicked(true);
        _bottomPanelView.OnProfileClicked(false);
        _bottomPanelView.OnSearchClicked(false);
        _bottomPanelView.OnHelpClicked(false);
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
    }

    private void UpdateHelpPage(HelpPage page)
    {
        helpPage = page;
    }

    private void EditModeChangedForHelp(bool editModeOn)
    {
        if (!editModeOn)
        {
            if (helpPage == HelpPage.ActionAugmentations ||
                helpPage == HelpPage.ActionInfo ||
                helpPage == HelpPage.ActionMarker)
            {
                helpPage = HelpPage.ActivitySteps;
            }
        }
    }
}
