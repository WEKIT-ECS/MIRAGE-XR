using System;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using MirageXR;
using UnityEngine;
using ContentType = LearningExperienceEngine.DataModel.ContentType;

[RequireComponent(typeof(Canvas))]
public class RootView_v2 : BaseView
{
    [Serializable]
    private class ContentTypeInfo
    {
        public ContentType ContentType;
        public Sprite Sprite;
        public String Label;
    }

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
    [SerializeField] private TutorialHandlerUI _tutorial;

    [SerializeField] private Sprite defaultContentIcon;
    [SerializeField] private String defaultContentLabel;
    [SerializeField] private ContentTypeInfo[] contentTypesIcons;
    [SerializeField] private PopupEditorBase[] _editors;

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

    public TutorialHandlerUI Tutorial => _tutorial;

    public ViewCamera viewCamera => _viewCamera;

    public Canvas canvas => _canvas;

    public PopupEditorBase[] editors => _editors;

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
        LearningExperienceEngine.EventManager.OnEditModeChanged += EditModeChangedForHelp;
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

        RootObject.Instance.LEE.ActivityManager.OnActivityLoaded += OnActivityLoaded; 
        LearningExperienceEngine.EventManager.OnWorkplaceLoaded += OnWorkplaceLoaded;
        LearningExperienceEngine.EventManager.OnStartActivity += OnActivityLoaded;
        EventManager.OnMobileHelpPageChanged += UpdateHelpPage;

        if (!LearningExperienceEngine.UserSettings.LoggedIn && LearningExperienceEngine.UserSettings.rememberUser)
        {
            await AutoLogin();
        }

        if (!LearningExperienceEngine.UserSettings.LoggedIn)
        {
            var dontShowLoginMenu = false;
            PopupsViewer.Instance.Show(_loginViewPrefab, dontShowLoginMenu, null);
        }

        RootObject.Instance.CameraCalibrationChecker.OnAnchorLost.AddListener(ShowCalibrationAlert);
    }

    public Sprite GetContentTypeSprite(ContentType contentType)
    {
        var sprite = contentTypesIcons.FirstOrDefault(t => t.ContentType == contentType);
        if (sprite != null)
        {
            return sprite.Sprite;
        }

        return defaultContentIcon;
    }

    public String GetContentTypeLabel(ContentType contentType)
    {
        var sprite = contentTypesIcons.FirstOrDefault(t => t.ContentType == contentType);
        if (sprite != null)
        {
            return sprite.Label;
        }

        return defaultContentLabel;
    }

    private void OnDestroy()
    {
        LearningExperienceEngine.EventManager.OnWorkplaceLoaded -= OnWorkplaceLoaded;
        LearningExperienceEngine.EventManager.OnStartActivity -= OnActivityLoaded;
        EventManager.OnMobileHelpPageChanged -= UpdateHelpPage;
        if (RootObject.Instance != null)
        {
            RootObject.Instance.CameraCalibrationChecker.OnAnchorLost.RemoveListener(ShowCalibrationAlert);
        }
    }

    private void OnWorkplaceLoaded()
    {
        if (!LearningExperienceEngine.UserSettings.dontShowCalibrationGuide && !_tutorial.IsActivated)
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
        if (!LearningExperienceEngine.UserSettings.TryToGetUsernameAndPassword(out var username, out var password)) return;

        LoadView.Instance.Show();
        await LearningExperienceEngine.LearningExperienceEngine.Instance.MoodleManager.Login(username, password);
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

    private void OnActivityLoaded(Activity activity)
    {
        ShowContentView();
    }

    public void OnActivityLoaded()
    {
        ShowContentView();
    }

    public void ShowContentView()
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
        var baseCamera = RootObject.Instance.BaseCamera;
        RootObject.Instance.LEE.ActivityManager.CreateNewActivity((baseCamera.transform.forward * 0.5f) + baseCamera.transform.position);
        _pageView.currentPageIndex = 1;
        LoadView.Instance.Hide();
    }

    public void ShowHomeView()
    {
        activityListView.FetchAndUpdateView().Forget();
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
