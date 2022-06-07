using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using MirageXR;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Canvas))]
public class RootView_v2 : BaseView
{
    public static RootView_v2 Instance { get; private set; }
    
    [SerializeField] private Toggle _toggleHome;
    [SerializeField] private Toggle _toggleProfile;
    [SerializeField] private Toggle _toggleNewActivity;
    [SerializeField] private Button _btnAddAugmentation;
    
    [SerializeField] private PageView_v2 _pageView;
    [SerializeField] private CalibrationGuideView _calibrationGuideViewPrefab;

    [SerializeField] private SearchView _searchPrefab;
    [SerializeField] private HelpView _helpPrefab;
    [SerializeField] private SettingsView_v2 _activitySettingsPrefab;
    [SerializeField] private MoodleServersView _moofleServersPrefab;

    [SerializeField] private GameObject newActivityGameObject;
    [SerializeField] private RectTransform  bottomPanel;
    
    private Vector3 _currentPanelPosition;
    float moveTime=1;
    float currentTime=0;
    private float normalizedValue;
    private bool panelMoving = false;
    
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
    public override void Initialization(BaseView parentView)
    {
        base.Initialization(parentView);
        EventManager.OnWorkplaceLoaded += OnWorkplaceLoaded;
        
        _toggleProfile.interactable = true;
        _toggleNewActivity.interactable = true;
        _toggleHome.onValueChanged.AddListener(OnStepsClick);
        _toggleProfile.onValueChanged.AddListener(OnViewClick);
        _toggleNewActivity.onValueChanged.AddListener(OnHomeClick);
        _btnAddAugmentation.onClick.AddListener(AddAugmentation);
        _pageView.OnPageChanged.AddListener(OnPageChanged);

        _currentPanelPosition = bottomPanel.anchoredPosition;
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

    private void OnPageChanged(int index)
    {
        switch (index)
        {
            case 0: 
                _toggleHome.isOn = true;
                newActivityGameObject.SetActive(true);
                break;
            case 1:
                _toggleProfile.isOn = true;
                newActivityGameObject.SetActive(true);
                break;
            case 2: 
                _toggleNewActivity.isOn = true;
                newActivityGameObject.SetActive(false);
                break;
        }
    }

    private void OnStepsClick(bool value)
    {
        if (value) _pageView.currentPageIndex = 0;
    }

    private void OnViewClick(bool value)
    {
        if (value) _pageView.currentPageIndex = 1;
    }

    private void OnHomeClick(bool value)
    {
        if (value) _pageView.currentPageIndex = 2;
    }

    private void AddAugmentation()
    {
        _pageView.currentPageIndex = 3;
    }

    public void OnSearchClick()
    {
        PopupsViewer.Instance.Show(_searchPrefab);
    }

    public void OnInfoClick()
    {
        PopupsViewer.Instance.Show(_helpPrefab);
    }
    
    public void OnMoodleServersClick()
    {
        PopupsViewer.Instance.Show(_moofleServersPrefab);
    }
    
    public void OnActivitySettingsClick()
    {
        PopupsViewer.Instance.Show(_activitySettingsPrefab);
    }

    public void OnBackToHome()
    {
        _pageView.currentPageIndex = 0;
    }

    public void OnBackToStep()
    {
        _pageView.currentPageIndex = 2;
    }
    
    public void OnBeginDrag()
    {
        if (!panelMoving)
        {
            StartCoroutine(LerpObject(_currentPanelPosition, new Vector3(0,-200,0)));
        }
    }
    
    public void OnDrag()
    {

    }
        
    public void EndDrag()
    {
        StartCoroutine(Delay(5f));
        if (!panelMoving)
        {
            StartCoroutine(LerpObject(new Vector3(0,-200,0), _currentPanelPosition));
        }
    }

    IEnumerator LerpObject(Vector3 from, Vector3 to)
    {
        panelMoving = true;
        currentTime = 0;
        while (currentTime  <= moveTime) {
           currentTime += Time.deltaTime; 
            normalizedValue=currentTime/moveTime;
            bottomPanel.anchoredPosition=Vector3.Lerp(from,to, normalizedValue);
            
            yield return null; 
           StartCoroutine(Delay(3f));
            panelMoving = false;
        }
    }
    
    IEnumerator Delay(float t)
    {
        yield return new WaitForSeconds(t);
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
        Screen.orientation = ScreenOrientation.Landscape;
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
