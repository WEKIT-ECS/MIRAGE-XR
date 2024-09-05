using System;
using System.Text.RegularExpressions;
using MirageXR;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class DirectLoginPopup : PopupBase
{
    [SerializeField] private ExtendedInputField _usernameInputField;
    [SerializeField] private ExtendedInputField _passwordInputField;
    [SerializeField] private Toggle _toggleRememberMe;
    [SerializeField] private Button _btnLogin;
    [SerializeField] private Button _btnClose;
    
    private Action<bool, string> _onClose;
    private bool _result;
    private string _json;
    private string _clientId;
    private string _appSecret;
    
    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);
        
        _usernameInputField.SetValidator(IsValidUsername);
        _passwordInputField.SetValidator(IsValidPassword);
        
        _btnLogin.onClick.AddListener(OnClickLogin);
        _btnClose.onClick.AddListener(Close);

        _toggleRememberMe.isOn = LearningExperienceEngine.UserSettings.rememberSketchfabUser;
        _toggleRememberMe.onValueChanged.AddListener(value => LearningExperienceEngine.UserSettings.rememberSketchfabUser = value);

        if (LearningExperienceEngine.UserSettings.rememberSketchfabUser && LearningExperienceEngine.UserSettings.TryGetPassword("direct_login_sketchfab", out var username, out var password))
        {
            _usernameInputField.text = username;
            _passwordInputField.text = password;
        }
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        try
        {
            _onClose = (Action<bool, string>)args[0];
            _clientId = (string)args[1];
            _appSecret = (string)args[2];
        }
        catch (Exception)
        {
            return false;
        }
        
        return true;
    }

    private void OnClickLogin()
    {
        if (!_usernameInputField.Validate()) return;
        if (!_passwordInputField.Validate()) return;

        Login(_usernameInputField.text, _passwordInputField.text);
    }

    private async void Login(string username, string password)
    {
        LoadView.Instance.Show();
        (_result, _json) = await MirageXR.Sketchfab.GetTokenAsync(username, password, _clientId, _appSecret);
        LoadView.Instance.Hide();
        var response = JsonConvert.DeserializeObject<MirageXR.Sketchfab.SketchfabResponse>(_json);
        if (_result)
        {
            if (response.error != null || response.access_token == null)
            {
                _usernameInputField.SetInvalid();
                _passwordInputField.SetInvalid();
                Toast.Instance.Show("Please, check your login/password");

                _result = false;
                return;
            } 
            if (_toggleRememberMe.isOn)
            {
                LearningExperienceEngine.UserSettings.SaveLoginDetails("direct_login_sketchfab", username, password);
            }
            Close();
            return;
        }

        _result = false;
        Toast.Instance.Show("Please, check your internet connection");
    }

    public override void Close()
    {
        _onClose(_result, _json);
        base.Close();
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
}