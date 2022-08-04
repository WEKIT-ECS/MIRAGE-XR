using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MirageXR;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class ProfileView : PopupBase
{
    [SerializeField] private Button _btnLogin;
    [SerializeField] private Button _btnRegister;
    [SerializeField] private ExtendedInputField _inputFieldUserName;
    [SerializeField] private ExtendedInputField _inputFieldPassword;
    [SerializeField] private Toggle _toggleRemember;
    [SerializeField] private GameObject LoginObjects;

    [SerializeField] private Button _btnLogout;
    [SerializeField] private TMP_Text _txtLogout;
    [SerializeField] private GameObject LogOutObjects;

    [SerializeField] private MoodleServersView _moodlePrefab;
    [SerializeField] private Button _btnSelectServer;
    [SerializeField] private TMP_Text _txtConnectedServer;



    public override void Init(Action<PopupBase> onClose, params object[] args)
    {
        base.Init(onClose, args);

        _inputFieldUserName.SetValidator(IsValidUsername);
        _inputFieldPassword.SetValidator(IsValidPassword);
        _btnRegister.onClick.AddListener(OnClickRegister);
        _btnLogin.onClick.AddListener(OnClickLogin);
        _btnLogout.onClick.AddListener(OnClickLogout);
        _toggleRemember.onValueChanged.AddListener(OnToggleRememberValueChanged);
        _btnSelectServer.onClick.AddListener(ShowServerPanel);

        UpdateConnectedServerText();

        ResetValues();
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        return true;
    }

    private async Task Login(string username, string password)
    {
        LoadView.Instance.Show();
        var result = await RootObject.Instance.moodleManager.Login(username, password);
        LoadView.Instance.Hide();
        if (result)
        {
            OnLoginSucceed(username, password);
            Close();
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
        DBManager.rememberUser = value;
    }

    private void OnEnable()
    {
        ResetValues();
    }

    private void OnLoginSucceed(string username, string password)
    {
        Toast.Instance.Show("Login succeeded");
        RootView_v2.Instance.activityListView_V2.UpdateListView();
        if (DBManager.rememberUser)
        {
            LocalFiles.SaveUsernameAndPassword(username, password);
        }
        else
        {
            LocalFiles.RemoveUsernameAndPassword();
        }
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
        _toggleRemember.isOn = DBManager.rememberUser;
        _inputFieldUserName.text = string.Empty;
        _inputFieldPassword.text = string.Empty;
        _inputFieldUserName.ResetValidation();
        _inputFieldPassword.ResetValidation();
    }

    private void OnClickRegister()
    {
        Application.OpenURL(DBManager.registerPage);
    }

    private async void OnClickLogin()
    {
        if (!_inputFieldUserName.Validate()) return;
        if (!_inputFieldPassword.Validate()) return;

        await Login(_inputFieldUserName.text, _inputFieldPassword.text);
    }

    private void OnClickLogout()
    {
        DBManager.LogOut();
        RootView_v2.Instance.activityListView_V2.UpdateListView();
        ShowLogin();
    }

    private static bool IsValidUsername(string value)
    {
        const string regexExpression = "^\\S{3,}$";
        var regex = new Regex(regexExpression);
        return regex.IsMatch(value);
    }

    private static bool IsValidPassword(string value)
    {
        const string regexExpression = "^\\S{8,}$";
        var regex = new Regex(regexExpression);
        return regex.IsMatch(value);
    }

    private void ShowServerPanel()
    {
        PopupsViewer.Instance.Show(_moodlePrefab);
    }

    private void UpdateConnectedServerText() 
    {
        _txtConnectedServer.text = DBManager.domain;
    }

}
