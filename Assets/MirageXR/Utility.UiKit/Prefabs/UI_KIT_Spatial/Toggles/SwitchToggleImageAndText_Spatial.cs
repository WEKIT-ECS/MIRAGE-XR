using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class SwitchToggleImageAndText_Spatial : MonoBehaviour
    {
        [SerializeField] protected Toggle _toggle;
        [SerializeField] private GameObject _handleImage;
        [SerializeField] private GameObject _textOn;
        [SerializeField] private GameObject _textOff;

        protected virtual void Start()
        {
            _toggle.onValueChanged.AddListener(UpdateView);
            UpdateView(_toggle.isOn);
        }

        protected virtual void UpdateView(bool value)
        {
            _handleImage.SetActive(value);
            _textOff?.SetActive(!value);
            _textOn?.SetActive(value);
        }

        protected virtual void OnDestroy()
        {
            _toggle.onValueChanged.RemoveListener(UpdateView);
        }
    }
}
