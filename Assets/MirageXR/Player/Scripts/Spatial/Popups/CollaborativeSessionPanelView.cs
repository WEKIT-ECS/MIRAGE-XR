using System;
using UnityEngine;
using UnityEngine.UI;
using Utility.UiKit.Runtime.Extensions;

namespace MirageXR
{
    public class CollaborativeSessionPanelView : PopupBase
    {
        [SerializeField] private Button _btnClose;
        [SerializeField] private Button _btnStart;
        protected override bool TryToGetArguments(params object[] args)
        {
            return true;
        }
        
        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);

            _btnClose.SafeSetListener(Close);
            _btnStart.SafeSetListener(OnStartClicked);
        }

        private void OnStartClicked()
        {
            // TODO
            Close();
            MenuManager.Instance.ShowCollaborativeSessionSettingsPanelView();
        }
    }
}
