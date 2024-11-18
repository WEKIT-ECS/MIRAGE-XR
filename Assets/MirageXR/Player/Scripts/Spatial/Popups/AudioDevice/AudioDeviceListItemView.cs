using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MirageXR
{
    public class AudioDeviceListItemView : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private TMP_Text text;

        //public Toggle Toggle => Toggle;
        public string AudioDeviceName => _audioDeviceName;

        private string _audioDeviceName;
        private UnityAction<string> _onToggleActivatedAction;

        public void Initialize(string audioDeviceName, ToggleGroup toggleGroup, UnityAction<string> onToggleActivatedAction)
        {
            _audioDeviceName = audioDeviceName;
            _onToggleActivatedAction = onToggleActivatedAction;

            text.text = audioDeviceName;
            toggle.isOn = false;
            toggle.group = toggleGroup;
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        public void SetToggleIsOnWithoutNotify(bool value)
        {
            toggle.SetIsOnWithoutNotify(value);
        }

        private void OnToggleValueChanged(bool value)
        {
            if (!value)
            {
                return;
            }

            _onToggleActivatedAction.Invoke(_audioDeviceName);
        }
    }
}
