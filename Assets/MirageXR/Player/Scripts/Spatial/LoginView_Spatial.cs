using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class LoginView_Spatial : MonoBehaviour
    {
        [SerializeField] private ExtendedInputField _inputFieldUserName;
        [SerializeField] private ExtendedInputField _inputFieldPassword;
        [SerializeField] private Toggle _toggleRemember;
        [SerializeField] private Button _btnRegister;
        [SerializeField] private Button _btnLogin;
        [SerializeField] private Button _btnLogout;
        [SerializeField] private Button _btnOpenLoginPanel;
        [SerializeField] private TMP_Text _textUserName;
        [Space]
        [SerializeField] private GameObject _loginPanel;
        [SerializeField] private GameObject _registerPanel;
        
        private Action _onLoginStatusChanged;

        private void Start()
        {
            _inputFieldUserName.SetValidator(IsValidUsername);
            _inputFieldPassword.SetValidator(IsValidPassword);
            _btnRegister.onClick.AddListener(OnClickRegister);
            _btnLogin.onClick.AddListener(OnClickLogin);
            _btnLogout.onClick.AddListener(OnClickLogout);
            _toggleRemember.onValueChanged.AddListener(OnToggleRememberValueChanged);

            if (LearningExperienceEngine.UserSettings.rememberUser)
            {
                // TODO: AutoLogin();
            }
            
            ResetValues();
        }
        
        private void OnEnable()
        {
            ResetValues();
        }


    private async Task Login(string username, string password)
    {
        var result = await LearningExperienceEngine.LearningExperienceEngine.Instance.MoodleManager.Login(username, password);
        if (result)
        {
            OnLoginSucceed(username, password);
            ResetValues();
        }
        else
        {
            _inputFieldUserName.SetInvalid();
            _inputFieldPassword.SetInvalid();
        }
    }

    private void OnToggleRememberValueChanged(bool value)
    {
        LearningExperienceEngine.UserSettings.rememberUser = value;
    }

    private void OnLoginSucceed(string username, string password)
    {
        UnityEngine.Debug.LogError("Login succeeded");
        // TODO: reload the activity list

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
        if (LearningExperienceEngine.UserSettings.LoggedIn)
        {
            _textUserName.text = "You signed as: " + LearningExperienceEngine.UserSettings.username;
            _loginPanel.SetActive(false);
            _btnLogout.gameObject.SetActive(true);
            _btnRegister.gameObject.SetActive(false);
            _btnOpenLoginPanel.gameObject.SetActive(false);
        }
        else
        {
            _textUserName.text = "You are signed as guest.";
            _btnLogout.gameObject.SetActive(false);
            _btnRegister.gameObject.SetActive(true);
            _btnOpenLoginPanel.gameObject.SetActive(true);
        }
        //_txtLogout.text = $"You are already logged in,\n<b>{LearningExperienceEngine.UserSettings.username}</b>";
        _toggleRemember.isOn = LearningExperienceEngine.UserSettings.rememberUser;
        _inputFieldUserName.text = string.Empty;
        _inputFieldPassword.text = string.Empty;
        _inputFieldUserName.ResetValidation();
        _inputFieldPassword.ResetValidation();
    }

    private void OnClickRegister()
    {
        Application.OpenURL(LearningExperienceEngine.UserSettings.registerPage);
    }

    private async void OnClickLogin()
    {
        if (!_inputFieldUserName.Validate()) return;
        if (!_inputFieldPassword.Validate()) return;

        await Login(_inputFieldUserName.text, _inputFieldPassword.text);
    }

    private void OnClickLogout()
    {
        LearningExperienceEngine.UserSettings.ClearLoginData();
        // TODO: reload the activity list
        //RootView.Instance.activityListView.UpdateListView();
        
        ResetValues();
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
}
