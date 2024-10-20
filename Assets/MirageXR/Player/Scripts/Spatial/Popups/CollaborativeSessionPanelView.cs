using System;
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

        private async void OnStartClicked()
        {
            CollaborationManager.Instance.SessionName = _sessionNameField.text;
            bool successful = await CollaborationManager.Instance.StartNewSession();

            if (!successful)
            {
                // TODO: an error message in the UI would be nice to inform the user that something has gone wrong
                return;
            }
            
            Close();
            MenuManager.Instance.ShowCollaborativeSessionSettingsPanelView();
        }
    }
}
