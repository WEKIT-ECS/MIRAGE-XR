using System.Threading.Tasks;
using MirageXR;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class RootView : BaseView
{
    public static RootView Instance { get; private set; }

    [SerializeField] private ActivityListView _activityListView;
    [SerializeField] private ContentListView _contentListView;
    [SerializeField] private StepsListView _stepsListView;
    [SerializeField] private Toggle _toggleHome;
    [SerializeField] private Toggle _toggleView;
    [SerializeField] private Toggle _toggleSteps;
    [SerializeField] private PageView _pageView;
    [SerializeField] private CalibrationGuideView _calibrationGuideViewPrefab;
    [SerializeField] private TutorialDialog _tutorialDialog;

    public ActivityListView activityListView => _activityListView;
    public ContentListView contentListView => _contentListView;
    public StepsListView stepsListView => _stepsListView;
    public TutorialDialog TutorialDialog => _tutorialDialog;
    public PageView PageView => _pageView;
    public Toggle ToggleSteps => _toggleSteps;
    public Toggle ToggleView => _toggleView;

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
        if (PlatformManager.GetDeviceFormat() == DeviceFormat.Tablet)
        {
            await SetupViewForTablet();
        }

        Initialization(null);
    }

    public override void Initialization(BaseView parentView)
    {
        base.Initialization(parentView);
        EventManager.OnWorkplaceLoaded += OnWorkplaceLoaded;
        _toggleView.interactable = false;
        _toggleSteps.interactable = false;
        _toggleHome.onValueChanged.AddListener(OnHomeClick);
        _toggleView.onValueChanged.AddListener(OnViewClick);
        _toggleSteps.onValueChanged.AddListener(OnStepsClick);
        _pageView.onPageChanged.AddListener(OnPageChanged);

        _activityListView.Initialization(this);
        _contentListView.Initialization(this);
        _stepsListView.Initialization(this);
        _tutorialDialog.Init();
    }

    private void OnDestroy()
    {
        EventManager.OnWorkplaceLoaded -= OnWorkplaceLoaded;
    }

    private void OnWorkplaceLoaded()
    {
        _toggleView.interactable = true;
        _toggleSteps.interactable = true;
        _toggleView.isOn = true;

        if (!DBManager.dontShowCalibrationGuide)
        {
            PopupsViewer.Instance.Show(_calibrationGuideViewPrefab);
        }
    }

    private void OnPageChanged(int index)
    {
        switch (index)
        {
            case 0:
                _toggleHome.isOn = true;
                break;
            case 1:
                _toggleView.isOn = true;
                break;
            case 2:
                _toggleSteps.isOn = true;
                break;
        }

        //EventManager.NotifyOnMobilePageChanged();
    }

    private void OnHomeClick(bool value)
    {
        if (value) _pageView.currentPageIndex = 0;
    }

    private void OnViewClick(bool value)
    {
        if (value) _pageView.currentPageIndex = 1;
        EventManager.NotifyOnViewSelectorClicked();
    }

    private void OnStepsClick(bool value)
    {
        if (value) _pageView.currentPageIndex = 2;
        EventManager.NotifyOnStepsSelectorClicked();
    }

    private async Task SetupViewForTablet()
    {
        const float scale = 0.001f;
        const float zPosition = 1.5f;
        const string layerName = "MobileUI";
        const string cameraName = "ViewCamera";

        var mainCamera = Camera.main;
        if (!mainCamera) return;

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
}
