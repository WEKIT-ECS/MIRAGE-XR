using MirageXR;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileView : PopupBase
{
    private const string VERSION_TEXT = "Version {0}";
    private const string CUSTOM_SERVER_TEXT = "Other";

    [SerializeField] private Button _btnClose;
    [SerializeField] private Button _btnLogin;
    [SerializeField] private Button _btnRegister;
    [SerializeField] private Button _btnPrivacyPolicy;
    [SerializeField] private GameObject _btnDevelopMode;
    [SerializeField] private GameObject LoginObjects;
    [SerializeField] private Button _btnLogout;
    [SerializeField] private TMP_Text _txtUserName;
    [SerializeField] private GameObject LogOutObjects;
    [SerializeField] private Button _btnSelectServer;
    [SerializeField] private TMP_Text _txtConnectedServer;
    [SerializeField] private Button _btnSelectLRS;
    [SerializeField] private TMP_Text _txtConnectedLRS;
    [SerializeField] private TMP_Text _txtVersion;
    [SerializeField] private ClickCounter _versionClickCounter;
    [SerializeField] private Button _btnGrid;
    [SerializeField] private Button _btnDev;
    [SerializeField] private ScrollRect _scrollRect;

    [Space]
    [SerializeField] private LoginView_v2 _loginViewPrefab;
    [SerializeField] private GridView _gridViewPrefab;
    [SerializeField] private DevelopView _developViewPrefab;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);

        _btnClose.onClick.AddListener(Close);
        _btnRegister.onClick.AddListener(OnClickRegister);
        _btnPrivacyPolicy.onClick.AddListener(OnClickPrivacyPolicy);
        _btnLogin.onClick.AddListener(OnClickLogin);
        _btnLogout.onClick.AddListener(OnClickLogout);
        _btnGrid.onClick.AddListener(OnClickGrid);
        _btnDev.onClick.AddListener(OnClickDev);
        _btnSelectServer.onClick.AddListener(ShowChangeServerPanel);
        _btnSelectLRS.onClick.AddListener(ShowLRSPanel);
        _versionClickCounter.onClickAmountReached.AddListener(OnVersionClickAmountReached);

        EventManager.MoodleDomainChanged += UpdateConnectedServerText;
        EventManager.XAPIChanged += UpdateConectedLRS;
        EventManager.MoodleDomainChanged += UpdatePrivacyPolicyButtonActive;

        _txtVersion.text = string.Format(VERSION_TEXT, Application.version);

        // only show link to develop mode settings if developMode is active
        _btnDevelopMode.SetActive(DBManager.developMode);

        UpdateConnectedServerText();
        UpdatePrivacyPolicyButtonActive();

        ResetValues();
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        return true;
    }

    private void OnVersionClickAmountReached(int count)
    {
        if (!DBManager.developMode)
        {
            EnterDevMode();
        }
    }

    private void EnterDevMode()
    {
        DBManager.developMode = true;
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

    private void ShowLogout()
    {
        LoginObjects.SetActive(false);
        LogOutObjects.SetActive(true);
    }

    private void ResetValues()
    {
        if (DBManager.LoggedIn)
        {
            ShowLogout();
        }
        else
        {
            ShowLogin();
        }

        _txtUserName.text = DBManager.username;
        UpdateConectedLRS(DBManager.publicCurrentLearningRecordStore);
    }

    private void OnClickRegister()
    {
        Application.OpenURL(DBManager.registerPage);
    }

    private async void OnClickLogin()
    {
        var dontShowLoginMenu = true;
        PopupsViewer.Instance.Show(_loginViewPrefab, dontShowLoginMenu, (System.Action)ResetValues);
    }

    private void OnClickLogout()
    {
        DBManager.LogOut();
        RootView_v2.Instance.activityListView.FetchAndUpdateView();
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

    private static bool IsValidUrl(string urlString)
    {
        const string regexExpression = "^(?:http(s)?:\\/\\/)?[\\w.-]+(?:\\.[\\w\\.-]+)+[\\w\\-\\._~:/?#[\\]@!\\$&'\\(\\)\\*\\+,;=.]+$";
        var regex = new Regex(regexExpression);
        return regex.IsMatch(urlString);
    }

    private void ShowChangeServerPanel()
    {
        var isWekitSelected = DBManager.domain == DBManager.WEKIT_URL;
        var isAreteSelected = DBManager.domain == DBManager.ARETE_URL;

        RootView_v2.Instance.dialog.ShowBottomMultilineToggles("Moodle servers:",
            (DBManager.WEKIT_URL, () => ChangeServerAndPrivacyPolicyDomain(DBManager.WEKIT_URL, DBManager.WEKIT_PRIVACY_POLICY_URL), false, isWekitSelected),
            (DBManager.ARETE_URL, () => ChangeServerAndPrivacyPolicyDomain(DBManager.ARETE_URL, DBManager.ARETE_PRIVACY_POLICY_URL), false, isAreteSelected),
            (CUSTOM_SERVER_TEXT, ShowServerPanel, false, !(isWekitSelected || isAreteSelected)));
    }

    private void ShowServerPanel()
    {
        RootView_v2.Instance.dialog.ShowBottomInputField(
            "Custom server:",
            "Enter address",
            "Cancel", null,
            "Save", OnCustomServerSave);
    }

    private void ShowLRSPanel()
    {
        RootView_v2.Instance.dialog.ShowBottomMultiline("Select Learning Record Store:",
            ("WEKIT", () => ChangeRecordStore(DBManager.LearningRecordStores.WEKIT)));
    }

    private static void ChangeRecordStore(DBManager.LearningRecordStores recordStores)
    {
        EventManager.NotifyxAPIChanged(recordStores);
        DBManager.publicCurrentLearningRecordStore = recordStores;
    }

    private void UpdateConnectedServerText()
    {
        _txtConnectedServer.text = DBManager.domain;
    }

    private void OnCustomServerSave(string address)
    {
        if (!IsValidUrl(address))
        {
            Toast.Instance.Show("Server address is invalid!");
            return;
        }

        DBManager.privacyPolicyDomain = string.Empty;
        ChangeServerDomain(address);
    }

    private static void ChangeServerDomain(string domain)
    {
        if (DBManager.domain != domain)
        {
            DBManager.domain = domain;
            DBManager.LogOut();
            RootView_v2.Instance.activityListView.FetchAndUpdateView();
        }

        EventManager.NotifyMoodleDomainChanged();
    }

    private static void ChangeServerAndPrivacyPolicyDomain(string domain, string privacyPolicyDomain)
    {
        DBManager.privacyPolicyDomain = privacyPolicyDomain;
        ChangeServerDomain(domain);
    }

    private void OnClickPrivacyPolicy()
    {
        Application.OpenURL(DBManager.privacyPolicyDomain);
    }

    private void UpdatePrivacyPolicyButtonActive()
    {
        var isWekitSelected = DBManager.domain == DBManager.WEKIT_URL;
        var isAreteSelected = DBManager.domain == DBManager.ARETE_URL;

        var setActive = !string.IsNullOrEmpty(DBManager.privacyPolicyDomain) &&
                (isWekitSelected || isAreteSelected);
        _btnPrivacyPolicy.gameObject.SetActive(setActive);

    }

    private void UpdateConectedLRS(DBManager.LearningRecordStores publicCurrentLearningRecordStore)
    {
        switch (publicCurrentLearningRecordStore)
        {
            case DBManager.LearningRecordStores.WEKIT:
                _txtConnectedLRS.text = "WEKIT";
                break;
        }
    }

    private void OnDisable()
    {
        EventManager.MoodleDomainChanged -= UpdateConnectedServerText;
        EventManager.XAPIChanged -= UpdateConectedLRS;
        EventManager.MoodleDomainChanged -= UpdatePrivacyPolicyButtonActive;
    }
}
