using System;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class ObjectSettingsPanelSpatialView : PopupBase
    {
        [Header("Buttons")]
        [SerializeField] private Button _buttonBack;
        [SerializeField] private Button _buttonRename;
        [SerializeField] private Button _buttonDelete;
        
        protected override bool TryToGetArguments(params object[] args)
        {
            return true;
        }
        
        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);
            
            _buttonBack.onClick.AddListener(OnClickBackButton);
            _buttonRename.onClick.AddListener(OnClickRenameButton);
            _buttonDelete.onClick.AddListener(OnClickDeleteButton);
        }

        private void OnClickDeleteButton()
        {
            // TODO
        }

        private void OnClickRenameButton()
        {
            // TODO
        }

        private void OnClickBackButton()
        {
            Close();
        }
    }
}
