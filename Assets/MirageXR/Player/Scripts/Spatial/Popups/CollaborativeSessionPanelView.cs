using Fusion.Addons.ConnectionManagerAddon;
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
        [SerializeField] private TMP_InputField _roomNameField;
        protected override bool TryToGetArguments(params object[] args)
        {
            return true;
        }
        
        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);

            _btnClose.SafeSetListener(Close);
            _btnStart.SafeSetListener(OnStartClicked);
            _roomNameField.text = "Default Room";
        }

        private async void OnStartClicked()
        {
            ConnectionManager.Instance.roomName = _roomNameField.text;
            bool successful = await ConnectionManager.Instance.Connect();

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
