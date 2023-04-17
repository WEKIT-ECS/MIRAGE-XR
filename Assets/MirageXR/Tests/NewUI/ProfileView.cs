using MirageXR;
using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileView : PopupBase
{
    private const string VERSION_TEXT = "Version {0}";

    [SerializeField] private Button _btnClose;
    [SerializeField] private Button _btnLogin;
    [SerializeField] private Button _btnRegister;
    [SerializeField] private Button _btnPrivacyPolicy;
    [SerializeField] private Toggle _developToggle;
    [SerializeField] private GameObject _developTogglePanel;
    [SerializeField] private GameObject LoginObjects;
    [SerializeField] private Button _btnLogout;
    [SerializeField] private TMP_Text _txtLogout;
    [SerializeField] private GameObject LogOutObjects;
    [SerializeField] private Button _btnSelectServer;
    [SerializeField] private TMP_Text _txtConnectedServer;
    [SerializeField] private Button _btnSelectLRS;
    [SerializeField] private TMP_Text _txtConnectedLRS;
    [SerializeField] private TMP_Text _txtVersion;
    [SerializeField] private ClickCounter _versionClickCounter;
    [Space]
    [SerializeField] private PopupBase _loginViewPrefab;

    private bool _isShownDevelopModeMessage;

    public static bool dontShowLoginMenu { get; set; }

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);

        _developToggle.isOn = DBManager.developMode;

        _btnClose.onClick.AddListener(Close);
        _btnRegister.onClick.AddListener(OnClickRegister);
        _btnPrivacyPolicy.onClick.AddListener(OnClickPrivacyPolicy);
        _btnLogin.onClick.AddListener(OnClickLogin);
        _btnLogout.onClick.AddListener(OnClickLogout);
        _developToggle.onValueChanged.AddListener(OnDevelopToggleValueChanged);
        _btnSelectServer.onClick.AddListener(ShowChangeServerPanel);
        _btnSelectLRS.onClick.AddListener(ShowLRSPanel);
        _versionClickCounter.onClickAmountReached.AddListener(OnVersionClickAmountReached);

        EventManager.MoodleDomainChanged += UpdateConnectedServerText;
        EventManager.XAPIChanged += UpdateConectedLRS;
        EventManager.MoodleDomainChanged += UpdatePrivacyPolicyButtonActive;

        _txtVersion.text = string.Format(VERSION_TEXT, Application.version);

        _developTogglePanel.SetActive(DBManager.developMode);

        UpdateConnectedServerText();
        UpdatePrivacyPolicyButtonActive();

        ResetValues();
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        return true;
    }

    private void OnDevelopToggleValueChanged(bool value)
    {
        DBManager.developMode = value;

        if (_isShownDevelopModeMessage)
        {
            return;
        }

        var valueString = value ? "enabled" : "disabled";
        Toast.Instance.Show($"Developer mode has been {valueString}.Restart the application to activate it.");
        _isShownDevelopModeMessage = true;
    }

    private void OnVersionClickAmountReached(int count)
    {
        ShowDevelopToggle();
    }

    private void ShowDevelopToggle()
    {
        _developTogglePanel.SetActive(true);
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

        _txtLogout.text = $"You are already logged in,\n<b>{DBManager.username}</b>";
        UpdateConectedLRS(DBManager.publicCurrentLearningRecordStore);
    }

    private void OnClickRegister()
    {
        Application.OpenURL(DBManager.registerPage);
    }

    private async void OnClickLogin()
    {
        dontShowLoginMenu = true;
        PopupsViewer.Instance.Show(_loginViewPrefab);
    }

    private void OnClickLogout()
    {
        DBManager.LogOut();
        RootView_v2.Instance.activityListView.FetchAndUpdateView();
        ShowLogin();
    }

    private static bool IsValidUrl(string urlString)
    {
        const string regexExpression = "^(?:http(s)?:\\/\\/)?[\\w.-]+(?:\\.[\\w\\.-]+)+[\\w\\-\\._~:/?#[\\]@!\\$&'\\(\\)\\*\\+,;=.]+$";
        var regex = new Regex(regexExpression);
        return regex.IsMatch(urlString);
    }

    private void ShowChangeServerPanel()
    {
        RootView_v2.Instance.dialog.ShowBottomMultiline("Select Learning Record Store:",
            (DBManager.WEKIT_URL, () => ChangeServerAndPrivacyPolicyDomain(DBManager.WEKIT_URL, DBManager.WEKIT_PRIVACY_POLICY_URL)),
            (DBManager.ARETE_URL, () => ChangeServerAndPrivacyPolicyDomain(DBManager.ARETE_URL, DBManager.ARETE_PRIVACY_POLICY_URL)),
            ("Other", ShowServerPanel));
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
            ("WEKIT", () => ChangeRecordStore(DBManager.LearningRecordStores.WEKIT)),
            ("ARETE", () => ChangeRecordStore(DBManager.LearningRecordStores.ARETE)));
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
        var setActive = (DBManager.privacyPolicyDomain != string.Empty) ? true : false;

        _btnPrivacyPolicy.gameObject.SetActive(setActive);
    }

    private void UpdateConectedLRS(DBManager.LearningRecordStores publicCurrentLearningRecordStore)
    {
        switch (publicCurrentLearningRecordStore)
        {
            case DBManager.LearningRecordStores.WEKIT:
                _txtConnectedLRS.text = "WEKIT";
                break;
            case DBManager.LearningRecordStores.ARETE:
                _txtConnectedLRS.text = "ARETE";
                break;
        }
    }
}
