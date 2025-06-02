using MirageXR;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileView : PopupBase
{
    private const string VERSION_TEXT = "Version {0}";
    private const string CUSTOM_SERVER_TEXT = "Other";

    [SerializeField] private Button _btnClose;
    [SerializeField] private Button _btnLogin;
    [SerializeField] private Button _btnOicdLogin;
    [SerializeField] private Button _btnRegister;
    [SerializeField] private Button _btnPrivacyPolicy;
    [SerializeField] private GameObject _btnDevelopMode;
    [SerializeField] private GameObject LoginObjects;
    [SerializeField] private Button _btnLogout;
    [SerializeField] private TMP_Text _txtUserName;
    [SerializeField] private GameObject LogOutObjects;
    [SerializeField] private Button _btnServerAddress;
    [SerializeField] private Button _btnServerPort;
    [SerializeField] private Button _btnWebsocketAddress;
    [SerializeField] private Button _btnWebsocketPort;
    [SerializeField] private TMP_Text _txtServerAddress;
    [SerializeField] private TMP_Text _txtServerPort;
    [SerializeField] private TMP_Text _txtWebsocketAddress;
    [SerializeField] private TMP_Text _txtWebsocketPort;
    [SerializeField] private Button _btnSelectLRS;
    [SerializeField] private TMP_Text _txtConnectedLRS;
    [SerializeField] private TMP_Text _txtVersion;
    [SerializeField] private ClickCounter _versionClickCounter;
    [SerializeField] private Button _btnGrid;
    [SerializeField] private Button _btnDev;
    [SerializeField] private ScrollRect _scrollRect;

    [Space]
    [SerializeField] private Button btnSketchfab;
    [SerializeField] private TMP_Text txtSketchfab;

    [Space]
    [SerializeField] private LoginView_v2 _loginViewPrefab;
    [SerializeField] private GridView _gridViewPrefab;
    [SerializeField] private DevelopView _developViewPrefab;
    [SerializeField] private SketchfabLoginView sketchfabLoginViewPrefab;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);

        _btnClose.onClick.AddListener(Close);
        _btnRegister.onClick.AddListener(OnClickRegister);
        _btnPrivacyPolicy.onClick.AddListener(OnClickPrivacyPolicy);
        _btnLogin.onClick.AddListener(OnClickLogin);
        _btnOicdLogin.onClick.AddListener(OnOidcLogin);
        _btnLogout.onClick.AddListener(OnClickLogout);
        _btnGrid.onClick.AddListener(OnClickGrid);
        _btnDev.onClick.AddListener(OnClickDev);
        _btnServerAddress.onClick.AddListener(ShowChangeServerAddressPanel);
        _btnServerPort.onClick.AddListener(ShowChangeServerPortPanel);
        _btnWebsocketAddress.onClick.AddListener(ShowChangeWebsocketAddressPanel);
        _btnWebsocketPort.onClick.AddListener(ShowChangeWebsocketPortPanel);
        _btnSelectLRS.onClick.AddListener(ShowLRSPanel);
        btnSketchfab.onClick.AddListener(OnSketchfabLogin);
        _versionClickCounter.onClickAmountReached.AddListener(OnVersionClickAmountReached);

        var configManager = RootObject.Instance.LEE.ConfigManager;
        configManager.OnNetworkServerAddressChanged += OnNetworkServerAddressChanged;
        configManager.OnNetworkServerPortChanged += OnNetworkServerPortChanged;
        configManager.OnNetworkWebsocketAddressChanged += OnNetworkWebsocketAddressChanged;
        configManager.OnNetworkWebsocketPortChanged += OnNetworkWebsocketPortChanged;

        //EventManager.MoodleDomainChanged += UpdateConnectedServerText;
        //EventManager.XAPIChanged += UpdateConnectedLRS;
        //EventManager.MoodleDomainChanged += UpdatePrivacyPolicyButtonActive;

        _txtVersion.text = string.Format(VERSION_TEXT, Application.version);

        // only show link to develop mode settings if developMode is active
        _btnDevelopMode.SetActive(LearningExperienceEngine.UserSettings.developMode);

        RootObject.Instance.LEE.SketchfabManager.OnSketchfabLoggedIn += OnSketchfabLoggedIn;
        RootObject.Instance.LEE.SketchfabManager.OnSketchfabUserDataChanged += OnSketchfabUserDataChanged;

        UpdatePrivacyPolicyButtonActive();

        ResetValues();
    }

    private void OnDestroy()
    {
        var configManager = RootObject.Instance?.LEE?.ConfigManager;
        if (configManager == null)
        {
            return;       
        }
        configManager.OnNetworkServerAddressChanged -= OnNetworkServerAddressChanged;
        configManager.OnNetworkServerPortChanged -= OnNetworkServerPortChanged;
        configManager.OnNetworkWebsocketAddressChanged -= OnNetworkWebsocketAddressChanged;
        configManager.OnNetworkWebsocketPortChanged -= OnNetworkWebsocketPortChanged;
    }

    private void OnSketchfabLoggedIn(bool value)
    {
        if (!value)
        {
            txtSketchfab.text = "Login required";
        }
    }

    private void OnSketchfabUserDataChanged(SketchfabUserInfo info)
    {
        txtSketchfab.text = $"You signed as {info.Username}";
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        return true;
    }

    private void OnVersionClickAmountReached(int count)
    {
        if (!LearningExperienceEngine.UserSettings.developMode)
        {
            EnterDevMode();
        }
    }

    private void EnterDevMode()
    {
        LearningExperienceEngine.UserSettings.developMode = true;
        _btnDevelopMode.SetActive(true);
		Toast.Instance.Show($"Developer mode has been activated.");
        StartCoroutine(ScrollToBottom());
	}

    private IEnumerator ScrollToBottom()
    {
		yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        _scrollRect.verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();
	}

    private void ShowLogin()
    {
        LoginObjects.SetActive(true);
        LogOutObjects.SetActive(false);
    }

    private void OnSketchfabLogin()
    {
        PopupsViewer.Instance.Show(sketchfabLoginViewPrefab);
    }

    private void ShowLogout()
    {
        LoginObjects.SetActive(false);
        LogOutObjects.SetActive(true);
    }

    private void ResetValues()
    {
        if (LearningExperienceEngine.UserSettings.LoggedIn)
        {
            ShowLogout();
        }
        else
        {
            ShowLogin();
        }

        _txtUserName.text = LearningExperienceEngine.UserSettings.username;
        UpdateConnectedLRS(LearningExperienceEngine.UserSettings.publicCurrentLearningRecordStore);
    }

    private void OnClickRegister()
    {
        Application.OpenURL(LearningExperienceEngine.UserSettings.registerPage);
    }

    private void OnClickLogin()
    {
        var dontShowLoginMenu = true;
        PopupsViewer.Instance.Show(_loginViewPrefab, dontShowLoginMenu, (Action)ResetValues);
    }

    private void OnOidcLogin()
    {
        RootObject.Instance.LEE.AuthorizationManager.OnLoginCompleted += OnOidcLoginCompleted;
        RootObject.Instance.LEE.AuthorizationManager.Login().Forget();
    }

    private void OnOidcLoginCompleted(string accessToken)
    {
        RootObject.Instance.LEE.AuthorizationManager.OnLoginCompleted -= OnOidcLoginCompleted;
        ShowLogout();
    }

    private void OnClickLogout()
    {
        if (LearningExperienceEngine.LearningExperienceEngine.Instance.AuthorizationManager.LoggedIn())
        {
            LearningExperienceEngine.LearningExperienceEngine.Instance.AuthorizationManager.Logout();
        }
        LearningExperienceEngine.UserSettings.ClearLoginData();
        RootView_v2.Instance.activityListView.FetchAndUpdateView().Forget();
        ShowLogin();
    }

    private void OnClickGrid()
    {
        PopupsViewer.Instance.Show(_gridViewPrefab);
    }

    private void OnClickDev()
    {
        PopupsViewer.Instance.Show(_developViewPrefab);
    }

    private static bool IsValidPort(string port)
    {
        return int.TryParse(port, out _);
    }

    private static bool IsValidUrl(string urlString)
    {
        const string regexExpression = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
        var regex = new Regex(regexExpression);
        return regex.IsMatch(urlString);
    }

    private static bool IsValidWebsocketUrl(string urlString)
    {
        const string regexExpression = @"^(?:ws(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
        var regex = new Regex(regexExpression);
        return regex.IsMatch(urlString);
    }

    private static void ShowChangeServerAddressPanel()
    {
        const string other = "Other";
        const string label = "Server address:";
        
        var configManager = RootObject.Instance.LEE.ConfigManager;
        var defaultConfig = configManager.GetDefaultNetworkServerAddress();
        var currentConfig = configManager.GetNetworkServerAddress();

        var isDefault = currentConfig == defaultConfig;

        if (isDefault)
        {
            RootView_v2.Instance.dialog.ShowBottomMultilineToggles(label,
                (defaultConfig, null, false, true),
                (other, ShowServerAddressPanel, false, false)
            );
        }
        else
        {
            RootView_v2.Instance.dialog.ShowBottomMultilineToggles(label,
                (defaultConfig, ResetNetworkServerAddress, false, false),
                (currentConfig, null, false, true),
                (other, ShowServerAddressPanel, false, false)
            );
        }
    }

    private static void ResetNetworkServerAddress()
    {
        RootObject.Instance.LEE.ConfigManager.ResetNetworkServerAddress();
        RootObject.Instance.LEE.ActivityManager.FetchActivitiesAsync();
    }

    private static void ShowServerAddressPanel()
    {
        RootView_v2.Instance.dialog.ShowBottomInputField(
            "Custom server address:",
            "Enter address",
            "Cancel", null,
            "Save", ChangeServerAddress);
    }

    private static void ChangeServerAddress(string address)
    {
        if (!IsValidUrl(address))
        {
            Toast.Instance.Show("Server address is invalid!");
            return;
        }

        RootObject.Instance.LEE.ConfigManager.SetNetworkServerAddress(address);
        RootObject.Instance.LEE.ActivityManager.FetchActivitiesAsync();
    }

    private static void ShowChangeServerPortPanel()
    {
        const string other = "Other";
        const string label = "Server port:";

        var configManager = RootObject.Instance.LEE.ConfigManager;
        var defaultConfig = configManager.GetDefaultNetworkServerPort();
        var currentConfig = configManager.GetNetworkServerPort();

        var isDefault = currentConfig == defaultConfig;

        if (isDefault)
        {
            RootView_v2.Instance.dialog.ShowBottomMultilineToggles(label,
                (defaultConfig, null, false, true),
                (other, ShowServerPortPanel, false, false)
            );
        }
        else
        {
            RootView_v2.Instance.dialog.ShowBottomMultilineToggles(label,
                (defaultConfig, ResetNetworkServerPort, false, false),
                (currentConfig, null, false, true),
                (other, ShowServerPortPanel, false, false)
            );
        }
    }

    private static void ResetNetworkServerPort()
    {
        RootObject.Instance.LEE.ConfigManager.ResetNetworkServerPort();
        RootObject.Instance.LEE.ActivityManager.FetchActivitiesAsync();
    }

    private static void ShowServerPortPanel()
    {
        RootView_v2.Instance.dialog.ShowBottomInputField(
            "Custom server port:",
            "Enter port",
            "Cancel", null,
            "Save", ChangeServerPort);
    }

    private static void ChangeServerPort(string port)
    {
        if (!IsValidPort(port))
        {
            Toast.Instance.Show("Server port is invalid!");
            return;
        }

        RootObject.Instance.LEE.ConfigManager.SetNetworkServerPort(port);
        RootObject.Instance.LEE.ActivityManager.FetchActivitiesAsync();
    }

    private static void ShowChangeWebsocketAddressPanel()
    {
        const string other = "Other";
        const string label = "Websocket address:";
        
        var configManager = RootObject.Instance.LEE.ConfigManager;
        var defaultConfig = configManager.GetDefaultNetworkWebsocketAddress();
        var currentConfig = configManager.GetNetworkWebsocketAddress();

        var isDefault = currentConfig == defaultConfig;

        if (isDefault)
        {
            RootView_v2.Instance.dialog.ShowBottomMultilineToggles(label,
                (defaultConfig, null, false, true),
                (other, ShowWebsocketAddressPanel, false, false)
            );
        }
        else
        {
            RootView_v2.Instance.dialog.ShowBottomMultilineToggles(label,
                (defaultConfig, ResetNetworkWebsocketAddress, false, false),
                (currentConfig, null, false, true),
                (other, ShowWebsocketAddressPanel, false, false)
            );
        }
    }

    private static void ResetNetworkWebsocketAddress()
    {
        RootObject.Instance.LEE.ConfigManager.ResetNetworkWebsocketAddress();
    }

    private static void ShowWebsocketAddressPanel()
    {
        RootView_v2.Instance.dialog.ShowBottomInputField(
            "Custom websocket address:",
            "Enter address",
            "Cancel", null,
            "Save", ChangeWebsocketAddress);
    }

    private static void ChangeWebsocketAddress(string address)
    {
        if (!IsValidWebsocketUrl(address))
        {
            Toast.Instance.Show("Websocket address is invalid!");
            return;
        }

        RootObject.Instance.LEE.ConfigManager.SetNetworkWebsocketAddress(address);
    }

    private static void ShowChangeWebsocketPortPanel()
    {
        const string other = "Other";
        const string label = "Websocket port:";

        var configManager = RootObject.Instance.LEE.ConfigManager;
        var defaultConfig = configManager.GetDefaultNetworkWebsocketPort();
        var currentConfig = configManager.GetNetworkWebsocketPort();

        var isDefault = currentConfig == defaultConfig;

        if (isDefault)
        {
            RootView_v2.Instance.dialog.ShowBottomMultilineToggles(label,
                (defaultConfig, null, false, true),
                (other, ShowWebsocketPortPanel, false, false)
            );
        }
        else
        {
            RootView_v2.Instance.dialog.ShowBottomMultilineToggles(label,
                (defaultConfig, ResetNetworkWebsocketPort, false, false),
                (currentConfig, null, false, true),
                (other, ShowWebsocketPortPanel, false, false)
            );
        }
    }

    private static void ResetNetworkWebsocketPort()
    {
        RootObject.Instance.LEE.ConfigManager.ResetNetworkWebsocketPort();
    }

    private static void ShowWebsocketPortPanel()
    {
        RootView_v2.Instance.dialog.ShowBottomInputField(
            "Custom server port:",
            "Enter port",
            "Cancel", null,
            "Save", ChangeWebsocketPort);
    }

    private static void ChangeWebsocketPort(string port)
    {
        if (!IsValidPort(port))
        {
            Toast.Instance.Show("Websocket port is invalid!");
            return;
        }

        RootObject.Instance.LEE.ConfigManager.SetNetworkWebsocketPort(port);
    }

    private void ShowLRSPanel()
    {
        RootView_v2.Instance.dialog.ShowBottomMultiline("Select Learning Record Store:",
            ("WEKIT", () => ChangeRecordStore(LearningExperienceEngine.UserSettings.LearningRecordStores.WEKIT)));
    }

    private static void ChangeRecordStore(LearningExperienceEngine.UserSettings.LearningRecordStores recordStores)
    {
        EventManager.NotifyxAPIChanged(recordStores);
        LearningExperienceEngine.UserSettings.publicCurrentLearningRecordStore = recordStores;
    }

    private void OnNetworkServerAddressChanged(string address)
    {
        _txtServerAddress.text = address;
    }

    private void OnNetworkServerPortChanged(string port)
    {
        _txtServerPort.text = port;
    }

    private void OnNetworkWebsocketAddressChanged(string address)
    {
        _txtWebsocketAddress.text = address;
    }

    private void OnNetworkWebsocketPortChanged(string port)
    {
        _txtWebsocketPort.text = port;
    }

    private static void ChangeServerDomain(string domain)
    {
        if (LearningExperienceEngine.UserSettings.domain != domain)
        {
            LearningExperienceEngine.UserSettings.domain = domain;
            LearningExperienceEngine.UserSettings.ClearLoginData();
            RootView_v2.Instance.activityListView.FetchAndUpdateView().Forget();
        }

        EventManager.NotifyMoodleDomainChanged();
    }

    private static void ChangeServerAndPrivacyPolicyDomain(string domain, string privacyPolicyDomain)
    {
        LearningExperienceEngine.UserSettings.privacyPolicyDomain = privacyPolicyDomain;
        ChangeServerDomain(domain);
    }

    private void OnClickPrivacyPolicy()
    {
        Application.OpenURL(LearningExperienceEngine.UserSettings.privacyPolicyDomain);
    }

    private void UpdatePrivacyPolicyButtonActive()
    {
        var isWekitSelected = LearningExperienceEngine.UserSettings.domain == LearningExperienceEngine.UserSettings.WEKIT_URL;
        var isAreteSelected = LearningExperienceEngine.UserSettings.domain == LearningExperienceEngine.UserSettings.ARETE_URL;
        var isCarateSelected = LearningExperienceEngine.UserSettings.domain == LearningExperienceEngine.UserSettings.CARATE_URL;

        var setActive = !string.IsNullOrEmpty(LearningExperienceEngine.UserSettings.privacyPolicyDomain) &&
                (isWekitSelected || isAreteSelected || isCarateSelected);
        _btnPrivacyPolicy.gameObject.SetActive(setActive);

    }

    private void UpdateConnectedLRS(LearningExperienceEngine.UserSettings.LearningRecordStores publicCurrentLearningRecordStore)
    {
        switch (publicCurrentLearningRecordStore)
        {
            case LearningExperienceEngine.UserSettings.LearningRecordStores.WEKIT:
                _txtConnectedLRS.text = "WEKIT";
                break;
        }
    }

    /*private void OnDisable()
    {
        EventManager.MoodleDomainChanged -= OnNetworkServerAddressChanged;
        EventManager.XAPIChanged -= UpdateConnectedLRS;
        EventManager.MoodleDomainChanged -= UpdatePrivacyPolicyButtonActive;
    }*/
}
