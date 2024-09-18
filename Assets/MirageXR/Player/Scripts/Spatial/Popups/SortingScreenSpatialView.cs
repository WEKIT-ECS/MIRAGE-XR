using System;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class SortingScreenSpatialView : PopupBase
    {
        [SerializeField] private Button _btnClose;
        protected override bool TryToGetArguments(params object[] args)
        {
            return true;
        }
        
        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);

            _btnClose.onClick.AddListener(Close);
        }
    }
}
