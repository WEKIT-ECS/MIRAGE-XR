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

            _toggleTwinVignette.isOn = (roomTwinManager.GetRoomTwinStyle() == RoomTwinStyle.TwinVignette);
            _toggleFullTwin.isOn = (roomTwinManager.GetRoomTwinStyle() == RoomTwinStyle.FullTwin);

            _toggleTwinVignette.onValueChanged.AddListener(ToggleTwinVignetteValueChanged);
            _toggleFullTwin.onValueChanged.AddListener(ToggleFullTwinValueChanged);

        }

        private void ToggleFullTwinValueChanged(bool value)
        {
            if (!value)
            {
                Debug.LogInfo("ToggleFullTwinValueChange: setting TwinVignette to on");
                _toggleTwinVignette.isOn = true;
                roomTwinManager.SetRoomTwinStyle(RoomTwinStyle.TwinVignette);
                return;
            }
            roomTwinManager.SetRoomTwinStyle(RoomTwinStyle.FullTwin);
            _toggleTwinVignette.isOn = false;
        }

        private void ToggleTwinVignetteValueChanged(bool value)
        {
            if (!value)
            {
                Debug.LogInfo("ToggleTwinVignetteValueChange: setting FullTwin to on");
                _toggleFullTwin.isOn = true;
                roomTwinManager.SetRoomTwinStyle(RoomTwinStyle.FullTwin);
                return;
            }
            roomTwinManager.SetRoomTwinStyle(RoomTwinStyle.TwinVignette);
            _toggleFullTwin.isOn = false;
        }
    }
}
