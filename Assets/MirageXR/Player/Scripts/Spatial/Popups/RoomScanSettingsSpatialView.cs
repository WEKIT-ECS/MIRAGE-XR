using System;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class RoomScanSettingsSpatialView : PopupBase
    {
        [SerializeField] private Button _btnClose;
        [SerializeField] private Toggle _toggleTwinVignette;
        [SerializeField] private Toggle _toggleFullTwin;
        
        private static RoomTwinManager roomTwinManager => RootObject.Instance.RoomTwinManager;
        protected override bool TryToGetArguments(params object[] args)
        {
            return true;
        }
        
        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);

            _btnClose.onClick.AddListener(Close);
            _toggleTwinVignette.onValueChanged.AddListener(ToggleTwinVignetteValueChanged);
            _toggleFullTwin.onValueChanged.AddListener(ToggleFullTwinValueChanged);
        }

        private void ToggleFullTwinValueChanged(bool value)
        {
            if (!value) return;
            roomTwinManager.SetRoomTwinStyle(RoomTwinStyle.FullTwin);
        }

        private void ToggleTwinVignetteValueChanged(bool value)
        {
            if (!value) return;
            roomTwinManager.SetRoomTwinStyle(RoomTwinStyle.TwinVignette);
        }
    }
}
