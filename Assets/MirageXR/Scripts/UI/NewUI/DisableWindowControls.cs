using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class DisableWindowControls : MonoBehaviour
    {
        [SerializeField] private Toggle _pinButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private GameObject _moveButton;

        private void Start()
        {
            if (_pinButton != null)
            {
                _pinButton.onValueChanged.AddListener(ToggleControls);
            }
        }

        private void OnDestroy()
        {
            if (_pinButton != null)
            {
                _pinButton.onValueChanged.RemoveListener(ToggleControls);
            }
        }

        private void ToggleControls(bool isPinned)
        {
            _closeButton.enabled = !isPinned;
            _moveButton.SetActive(!isPinned);
        }
    }
}