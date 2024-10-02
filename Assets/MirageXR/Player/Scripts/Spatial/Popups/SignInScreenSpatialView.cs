using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility.UiKit.Runtime.Extensions;

namespace MirageXR
{
    public class SignInScreenSpatialView : PopupBase
    {
        [Header("Buttons")]
        [SerializeField] private Button _btnSignInScreenClose;
        [SerializeField] private Button _btnCopmanyScreenClose;
        [Header("Input Fields")]
        [SerializeField] private TMP_InputField _inputFieldEmail;
        [SerializeField] private TMP_InputField _inputFieldPassword;
        [Header("Toggles")]
        [SerializeField] private Toggle _toggleStaySignedIn;
        protected override bool TryToGetArguments(params object[] args)
        {
            return true;
        }
        
        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);

            _btnSignInScreenClose.SafeSetListener(Close);
            _inputFieldEmail.SafeAddListener(OnEmailInputFieldChanged);
            _inputFieldPassword.SafeAddListener(OnPasswordInputFieldChanged);
            //_btnCopmanyScreenClose.SafeSetListener(Close);
      
        }

        private void OnPasswordInputFieldChanged(string value)
        {
        }

        private void OnEmailInputFieldChanged(string value)
        {
        }
    }
}
