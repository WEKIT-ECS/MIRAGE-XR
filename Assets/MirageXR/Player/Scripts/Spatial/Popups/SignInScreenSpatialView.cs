using System;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class SignInScreenSpatialView : PopupBase
    {
        [SerializeField] private Button _btnSignInScreenClose;
        [SerializeField] private Button _btnCopmanyScreenClose;
        protected override bool TryToGetArguments(params object[] args)
        {
            return true;
        }
        
        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);

            _btnSignInScreenClose.onClick.AddListener(Close);
            _btnCopmanyScreenClose.onClick.AddListener(Close);
        }
    }
}
