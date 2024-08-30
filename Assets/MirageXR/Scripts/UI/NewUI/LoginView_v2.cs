﻿using i5.Toolkit.Core.OpenIDConnectClient;
using i5.Toolkit.Core.ServiceCore;
using LearningExperienceEngine;
using MirageXR;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LoginView_v2 : PopupBase
{
    [SerializeField] private ExtendedInputField _inputFieldUserName;
    [SerializeField] private ExtendedInputField _inputFieldPassword;
    [SerializeField] private GameObject _pnlMenu;
    [SerializeField] private GameObject _pnlFields;
    [SerializeField] private Toggle _toggleRemember;
    [SerializeField] private Button _btnRegister;
    [SerializeField] private Button _btnPasswordForgotten;
    [SerializeField] private Button _btnGoToLogin;
    [SerializeField] private Button _btnSkipLogin;
    [SerializeField] private Button _btnOidcLogin;
    [SerializeField] private Button _btnLogin;
    [SerializeField] private Button _btnBack;
    [SerializeField] private OnboardingTutorialView _onboardingTutorialViewPrefab;

    private System.Action _onLoginStatusChanged;
    private bool _dontShowLoginMenu;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);

        _inputFieldUserName.SetValidator(IsValidUsername);
        _inputFieldPassword.SetValidator(IsValidPassword);
        _btnRegister.onClick.AddListener(OnClickRegister);
        _btnPasswordForgotten.onClick.AddListener(OnClickPasswordForgotten);
        _btnOidcLogin.onClick.AddListener(OnOidcLogin);
        _btnLogin.onClick.AddListener(OnClickLogin);
        _btnGoToLogin.onClick.AddListener(OnGoToLoginPressed);
        _btnSkipLogin.onClick.AddListener(OnEnterAsGuest);
        _btnBack.onClick.AddListener(OnClickBack);
        _toggleRemember.onValueChanged.AddListener(OnToggleRememberValueChanged);

        ResetValues();

        LearningExperienceEngine.OidcLogin oidcLogin = GetComponent<LearningExperienceEngine.OidcLogin>();
        if (oidcLogin.LoggedIn())
        {
            var service = ServiceManager.GetService<OpenIDConnectService>();
            service.LoginCompleted += OnOidcLoginCompleted;

            oidcLogin.Login();
        }
        else
        {
            Debug.Log("Continuing init to offer all the options in the view");
        }

    }

    protected override bool TryToGetArguments(params object[] args)
    {
        try
        {
            _dontShowLoginMenu = (bool)args[0];
            _onLoginStatusChanged = (System.Action)args[1];
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task Login(string username, string password)
    {
        LoadView.Instance.Show();
        var result = await LearningExperienceEngine.LearningExperienceEngine.Instance.moodleManager.Login(username, password);
        LoadView.Instance.Hide();
        if (result)
        {
            OnLoginSucceed(username, password);
            Close();
            PopupsViewer.Instance.Show(_onboardingTutorialViewPrefab);
        }
        else
        {
            _inputFieldUserName.SetInvalid();
            _inputFieldPassword.SetInvalid();
            Toast.Instance.Show("Check your login/password");
        }
    }

    private void OnToggleRememberValueChanged(bool value)
    {
        LearningExperienceEngine.UserSettings.rememberUser = value;
    }

    private void OnEnable()
    {
        ResetValues();
    }

    private void OnEnterAsGuest()
    {
        PopupsViewer.Instance.Show(_onboardingTutorialViewPrefab);
        Close();
    }

    private void OnOidcLogin()
    {
        var service = ServiceManager.GetService<OpenIDConnectService>();
        service.LoginCompleted += OnOidcLoginCompleted;

        LearningExperienceEngine.OidcLogin oidcLogin = GetComponent<LearningExperienceEngine.OidcLogin>();
        oidcLogin.Login();
    }

    private void OnOidcLoginCompleted(object sender, EventArgs e)
    {
        Toast.Instance.Show("Login succeeded");

        var service = ServiceManager.GetService<OpenIDConnectService>();
        service.LoginCompleted -= OnOidcLoginCompleted;

        Close();

        // this is evaluated by UserInfo.LoggedIn, which is used to determine if the logout button is displayed in the profile view
        UserSettings.username = "via Open ID connect"; // temp until callback received to fetch real username
        LearningExperienceEngine.OidcLogin oidcLogin = GetComponent<LearningExperienceEngine.OidcLogin>();
        oidcLogin.FetchUsername();

        // if the activity view is loading activities already in the background, this might also require:
        // RootView_v2.Instance.activityListView.FetchAndUpdateView();

    }

    private void OnGoToLoginPressed()
    {
        _pnlMenu.SetActive(false);
        _pnlFields.SetActive(true);
        _btnBack.gameObject.SetActive(true);
    }

    private void OnClickBack()
    {
        _pnlMenu.SetActive(true);
        _pnlFields.SetActive(false);
        _btnBack.gameObject.SetActive(false);
    }

    private void OnLoginSucceed(string username, string password)
    {
        Toast.Instance.Show("Login succeeded");
        if (LearningExperienceEngine.UserSettings.rememberUser)
        {
            LearningExperienceEngine.UserSettings.SaveUsernameAndPassword(username, password);
        }
        else
        {
            LearningExperienceEngine.UserSettings.RemoveUsernameAndPassword();
        }

        _onLoginStatusChanged?.Invoke();
    }

    private void ResetValues()
    {
        _toggleRemember.isOn = LearningExperienceEngine.UserSettings.rememberUser;
        _inputFieldUserName.text = string.Empty;
        _inputFieldPassword.text = string.Empty;
        if (_dontShowLoginMenu)
        {
            _pnlMenu.SetActive(false);
            _pnlFields.SetActive(true);
        }
        else
        {
            _pnlMenu.SetActive(true);
            _pnlFields.SetActive(false);
        }
        _inputFieldUserName.ResetValidation();
        _inputFieldPassword.ResetValidation();
    }

    private void OnClickRegister()
    {
        Application.OpenURL(LearningExperienceEngine.UserSettings.registerPage);
    }

    private void OnClickPasswordForgotten()
    {
        Application.OpenURL(LearningExperienceEngine.UserSettings.passwordForgottenPage);
    }

    private async void OnClickLogin()
    {
        if (!_inputFieldUserName.Validate())
        {
            return;
        }

        if (!_inputFieldPassword.Validate())
        {
            return;
        }

        await Login(_inputFieldUserName.text, _inputFieldPassword.text);
    }

    private static bool IsValidUsername(string value)
    {
        const string regexExpression = "^\\S{3,}$";
        var regex = new Regex(regexExpression);
        var isValid = regex.IsMatch(value);

        const string MatchEmailPattern =
                            @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
                            + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
                            + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
                            + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";

        regex = new Regex(MatchEmailPattern);

        var isEmail = regex.IsMatch(value);

        if (isEmail || !isValid)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private static bool IsValidPassword(string value)
    {
        const string regexExpression = "^\\S{8,}$";
        var regex = new Regex(regexExpression);
        return regex.IsMatch(value);
    }

    private static bool IsAnEmail(string email)
    {
        const string MatchEmailPattern =
        @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
        + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
        + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
        + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";

        var regex = new Regex(MatchEmailPattern);

        if (email != null)
        {
            return regex.IsMatch(email);
        }
        else
        {
            return false;
        }
    }
}
