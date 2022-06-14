using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginView_v2 : PopupBase
{
    [SerializeField] private ExtendedInputField _inputFieldUserName;
    [SerializeField] private ExtendedInputField _inputFieldPassword;
    [SerializeField] private Toggle _toggleRemember;
    [SerializeField] private Button _btnLogin;
  
    [SerializeField] private GameObject[] _loginObjects;
    [SerializeField] private GameObject registerPrefab;
    
    
    [SerializeField] private GameObject enter2Prefab; //temp
    void Update() 
    {
#if UNITY_EDITOR // delete that!!!
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            enter2Prefab.SetActive(false);
        }
#endif
    }

    public override void Init(Action<PopupBase> onClose, params object[] args)
    {
        base.Init(onClose, args);
        
        _inputFieldUserName.SetValidator(IsValidUsername);
        _inputFieldPassword.SetValidator(IsValidPassword);
        _btnLogin.onClick.AddListener(OnClickLogin);
        _toggleRemember.onValueChanged.AddListener(OnToggleRememberValueChanged);
            
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
        RootView.Instance.activityListView.UpdateListView();
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
        foreach (var obj in _loginObjects) obj.SetActive(true);
    }
    
    private void ResetValues()
    {
        ShowLogin();
        _toggleRemember.isOn = DBManager.rememberUser;
        _inputFieldUserName.text = string.Empty;
        _inputFieldPassword.text = string.Empty;
        _inputFieldUserName.ResetValidation();
        _inputFieldPassword.ResetValidation();
    }

    private async void OnClickLogin()
    {
        if (!_inputFieldUserName.Validate()) return;
        if (!_inputFieldPassword.Validate()) return;

        await Login(_inputFieldUserName.text, _inputFieldPassword.text);
    }
    
    private static bool IsValidUsername(string urlString)
    {
        const string regexExpression = "^(?=[a-zA-Z0-9._@!#$%^&]{4,}$)(?!.*[_.]{2})[^_.].*[^_.]$";
        var regex = new Regex(regexExpression);
        return regex.IsMatch(urlString);
    }

    private static bool IsValidPassword(string urlString)
    {
        const string regexExpression = "^(?=.*[A-Za-z])(?=.*\\d)[A-Za-z\\d@$!%*#?&]{8,}$";
        var regex = new Regex(regexExpression);
        return regex.IsMatch(urlString);
    }

    public void OnClickRegister()
    {
        Instantiate(registerPrefab);
    }
}
