using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class SketchfabLoginView : PopupBase
    {
        [SerializeField] private Button btnClose;
        [SerializeField] private Button btnLogin;
        [SerializeField] private Button btnRegister;
        [SerializeField] private Button btnLogout;
        [SerializeField] private TMP_InputField inputEmail;
        [SerializeField] private TMP_InputField inputPassword;
        [SerializeField] private TMP_Text txtEmail;
        [SerializeField] private GameObject panelLogin;
        [SerializeField] private GameObject panelLogout;

        private string _loginEmail;
        private string _loginPassword;

        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);

            btnClose.onClick.AddListener(Close);
            btnLogin.onClick.AddListener(OnLoginClick);
            btnRegister.onClick.AddListener(OnRegisterClick);
            btnLogout.onClick.AddListener(OnLogoutClick);

            inputEmail.onValueChanged.AddListener(OnInputEmailValueChanged);
            inputPassword.onValueChanged.AddListener(OnInputPasswordValueChanged);

            RootObject.Instance.LEE.SketchfabManager.OnSketchfabLoggedIn += SketchfabManagerOnSketchfabLoggedIn; 
            RootObject.Instance.LEE.SketchfabManager.OnSketchfabUserDataChanged += SketchfabManagerOnSketchfabUserDataChanged; 
        }

        private void OnDestroy()
        {
            RootObject.Instance.LEE.SketchfabManager.OnSketchfabLoggedIn -= SketchfabManagerOnSketchfabLoggedIn; 
            RootObject.Instance.LEE.SketchfabManager.OnSketchfabUserDataChanged -= SketchfabManagerOnSketchfabUserDataChanged; 
        }

        private void SketchfabManagerOnSketchfabUserDataChanged(SketchfabUserInfo userData)
        {
            txtEmail.text = userData.Email;
        }

        private void SketchfabManagerOnSketchfabLoggedIn(bool value)
        {
            panelLogin.SetActive(!value);
            panelLogout.SetActive(value);
        }

        private void OnInputEmailValueChanged(string value)
        {
            _loginEmail = value;
        }

        private void OnInputPasswordValueChanged(string value)
        {
            _loginPassword = value;
        }

        private void OnLoginClick()
        {
            OnLoginClickAsync().Forget();
        }

        private async UniTask OnLoginClickAsync()
        {
            var result = await RootObject.Instance.LEE.SketchfabManager.AuthorizeWithLoginPasswordAsync(_loginEmail, _loginPassword);
            if (result.Success)
            {
                Close();
            }
            else
            {
                Toast.Instance.Show(result.ErrorMessage);
            }
        }

        private void OnRegisterClick()
        {
            Application.OpenURL("https://sketchfab.com/signup");
        }

        private void OnLogoutClick()
        {
            RootObject.Instance.LEE.SketchfabManager.Logout();
        }

        protected override bool TryToGetArguments(params object[] args)
        {
            return true;
        }
    }
}
