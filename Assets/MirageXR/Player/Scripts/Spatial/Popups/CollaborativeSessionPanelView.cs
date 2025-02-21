using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility.UiKit.Runtime.Extensions;

namespace MirageXR
{
	public class CollaborativeSessionPanelView : PopupBase
    {
        [SerializeField] private Button _btnClose;
        [SerializeField] private Button _btnStart;
        [SerializeField] private TMP_InputField _sessionNameField;

        protected override bool TryToGetArguments(params object[] args)
        {
            return true;
        }
        
        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);

            _btnClose.SafeSetListener(Close);
            _btnStart.SafeSetListener(OnStartClicked);
            _sessionNameField.text = "Default Session";
        }

        private void OnStartClicked()
        {
            StartNewSessionAsync().Forget();
        }

        private async UniTask StartNewSessionAsync()
        {
#if FUSION2
            RootObject.Instance.CollaborationManager.SessionName = _sessionNameField.text;
            var successful = await RootObject.Instance.CollaborationManager.StartNewSession();

            if (!successful)
            {
                // TODO: an error message in the UI would be nice to inform the user that something has gone wrong
                return;
            }
#endif 
            Close();
            MenuManager.Instance.ShowCollaborativeSessionSettingsPanelView();
        }
    }
}
