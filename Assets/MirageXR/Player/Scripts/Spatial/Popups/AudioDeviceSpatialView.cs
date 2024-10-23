using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class AudioDeviceSpatialView : PopupBase
    {
        private class StringHolder : ObjectHolder<string> { }
        
        [SerializeField] private Button btnClose;
        [SerializeField] private Transform container;
        [SerializeField] private GameObject listItemPrefab;

        protected override bool TryToGetArguments(params object[] args)
        {
            return true;
        }

        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);

            btnClose.onClick.AddListener(Close);
            UpdateView();
        }

        private void UpdateView()
        {
            foreach (Transform item in container)
            {
                Destroy(item.gameObject);
            }

            var devices = AudioRecorder.GetRecordingDevices();
            foreach (var device in devices)
            {
                var item = Instantiate(listItemPrefab, container);
                var text = item.GetComponentInChildren<TMP_Text>();
                text.text = device;
                var button = item.GetComponent<Button>();
                button.onClick.AddListener(() => OnClick(item));
                var holder = item.AddComponent<StringHolder>();
                holder.item = device;
                item.gameObject.SetActive(true);
            }
        }

        private void OnClick(GameObject item)
        {
            var holder = item.GetComponent<StringHolder>();
            AudioRecorder.SetRecordingDevice(holder.item);
            LearningExperienceEngine.UserSettings.audioDevice = holder.item;
            Close();
        }
    }
}
