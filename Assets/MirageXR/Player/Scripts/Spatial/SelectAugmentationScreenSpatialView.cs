using System;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class SelectAugmentationScreenSpatialView : PopupBase
    {
        [SerializeField] private Button _buttonBack;
        [SerializeField] private Transform _listContent;
        [SerializeField] private ContentSelectorListItem _contentSelectorListItemPrefab;
        
        protected override bool TryToGetArguments(params object[] args)
        {
            return true;
        }
        
        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);

            _buttonBack.onClick.AddListener(Close);
            UpdateView();
        }

        private void UpdateView()
        {
            // TODO
        }
    }
}
