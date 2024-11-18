using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class AudioDeviceSpatialView : PopupBase
    {
        [SerializeField] private Button btnClose;
        [SerializeField] private Transform container;
        [SerializeField] private ToggleGroup toggleGroup;
        [SerializeField] private AudioDeviceListItemView listItemPrefab;
        
        private List<AudioDeviceListItemView> _list = new();

        protected override bool TryToGetArguments(params object[] args)
        {
            return true;
        }

        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);

            toggleGroup.allowSwitchOff = true;
            btnClose.onClick.AddListener(Close);
            UpdateView();
        }

        private void UpdateView()
        {
            foreach (var item in _list)
            {
                Destroy(item.gameObject);
            }
            _list.Clear();

            var audioDevice = RootObject.Instance.LEE.AudioManager.AudioDevice;
            var devices = RootObject.Instance.LEE.AudioManager.GetRecordingDevices();
            foreach (var device in devices)
            {
                var item = Instantiate(listItemPrefab, container);
                item.Initialize(device, toggleGroup, OnToggleActivated);
                _list.Add(item);
            }

            foreach (var item in _list)
            {
                if (item.AudioDeviceName == audioDevice)
                {
                    //item.Toggle.isOn = true;
                    item.SetToggleIsOnWithoutNotify(true);
                }
            }
        }

        private void OnToggleActivated(string deviceName)
        {
            RootObject.Instance.LEE.AudioManager.SetRecordingDevice(deviceName);
            Close();
        }
    }
}
