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
        [SerializeField] private Toggle _toggleOcclusion;
        
        private static RoomTwinManager roomTwinManager => RootObject.Instance.RoomTwinManager;
        private bool _isUpdatingToggles;

        protected override bool TryToGetArguments(params object[] args)
        {
            return true;
        }
        
        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);

            _btnClose.onClick.AddListener(Close);

            UpdateToggleStates(roomTwinManager.GetRoomTwinStyle());

            _toggleTwinVignette.onValueChanged.AddListener(ToggleTwinVignetteValueChanged);
            _toggleFullTwin.onValueChanged.AddListener(ToggleFullTwinValueChanged);
            if (_toggleOcclusion != null)
            {
                _toggleOcclusion.onValueChanged.AddListener(ToggleOcclusionValueChanged);
            }
        }

        private void UpdateToggleStates(RoomTwinStyle style)
        {
            _isUpdatingToggles = true;
            if (_toggleTwinVignette != null)
            {
                _toggleTwinVignette.SetIsOnWithoutNotify(style == RoomTwinStyle.TwinVignette);
            }
            if (_toggleFullTwin != null)
            {
                _toggleFullTwin.SetIsOnWithoutNotify(style == RoomTwinStyle.FullTwin);
            }
            if (_toggleOcclusion != null)
            {
                _toggleOcclusion.SetIsOnWithoutNotify(style == RoomTwinStyle.Occlusion);
            }
            _isUpdatingToggles = false;
        }

        private void ToggleFullTwinValueChanged(bool value)
        {
            if (_isUpdatingToggles) return;
            if (value)
            {
                roomTwinManager.SetRoomTwinStyle(RoomTwinStyle.FullTwin);
                UpdateToggleStates(RoomTwinStyle.FullTwin);
            }
            else
            {
                _toggleFullTwin.SetIsOnWithoutNotify(true);
            }
        }

        private void ToggleTwinVignetteValueChanged(bool value)
        {
            if (_isUpdatingToggles) return;
            if (value)
            {
                roomTwinManager.SetRoomTwinStyle(RoomTwinStyle.TwinVignette);
                UpdateToggleStates(RoomTwinStyle.TwinVignette);
            }
            else
            {
                _toggleTwinVignette.SetIsOnWithoutNotify(true);
            }
        }

        private void ToggleOcclusionValueChanged(bool value)
        {
            if (_isUpdatingToggles) return;
            if (value)
            {
                roomTwinManager.SetRoomTwinStyle(RoomTwinStyle.Occlusion);
                UpdateToggleStates(RoomTwinStyle.Occlusion);
            }
            else
            {
                _toggleOcclusion.SetIsOnWithoutNotify(true);
            }
        }
    }
}
