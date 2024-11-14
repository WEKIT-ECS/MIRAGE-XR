using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class SketchfabSignInPopupView : PopupBase
    {
        [SerializeField] private TMP_InputField inputFieldEmail;
        [SerializeField] private TMP_InputField inputFieldPassword;
        [SerializeField] private Button buttonLogin;
        [SerializeField] private Button buttonForgotPassword;
        [SerializeField] private Button buttonHelp;
        [SerializeField] private Button buttonClose;

        private string _email;
        private string _password;

        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);

            inputFieldEmail.onValueChanged.AddListener(OnInputFieldEmailValueChanged);
            inputFieldPassword.onValueChanged.AddListener(OnInputFieldPasswordValueChanged);
            buttonLogin.onClick.AddListener(OnButtonLoginClick);
            buttonForgotPassword.onClick.AddListener(OnButtonForgotPasswordClick);
            buttonHelp.onClick.AddListener(OnButtonHelpClick);
            buttonClose.onClick.AddListener(OnButtonCloseClick);
        }

        private async UniTask SignInToSketchfabAsync()
        {
            var response = await RootObject.Instance.LEE.SketchfabManager.AuthorizeWithLoginPasswordAsync(_email, _password);
            if (response.Success)
            {
                Toast.Instance.Show("Logged in successfully");
                Close();
            }
            else
            {
                Toast.Instance.Show(response.ErrorMessage);
            }
        }

        private void OnInputFieldEmailValueChanged(string text)
        {
            _email = text;
        }

        private void OnInputFieldPasswordValueChanged(string text)
        {
            _password = text;
        }

        private void OnButtonLoginClick()
        {
            SignInToSketchfabAsync().Forget();
        }

        private void OnButtonForgotPasswordClick()
        {
        }

        private void OnButtonHelpClick()
        {
        }

        private void OnButtonCloseClick()
        {
            Close();
        }

        protected override bool TryToGetArguments(params object[] args)
        {
            return true;
        }
    }
}
