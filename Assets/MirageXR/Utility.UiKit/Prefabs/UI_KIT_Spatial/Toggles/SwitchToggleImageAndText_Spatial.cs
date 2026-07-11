using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class SwitchToggleImageAndText_Spatial : MonoBehaviour
    {
        private const float ActiveAlpha = 1f;
        private const float InactiveAlpha = 0f;

        [SerializeField] protected Toggle _toggle;
        [SerializeField] private GameObject _handleImage;
        [SerializeField] private GameObject _textOn;
        [SerializeField] private GameObject _textOff;

        private Coroutine _refreshRoutine;

        protected virtual void Start()
        {
            _toggle.onValueChanged.AddListener(UpdateView);
            UpdateView(_toggle.isOn);
        }

        protected virtual void UpdateView(bool value)
        {
            if (_handleImage != null)
            {
                UpdateToggleColors(value ? 1f : 0f);
                RefreshAfterDeselect(value);
            }
            _textOff?.SetActive(!value);
            _textOn?.SetActive(value);
        }

        private void UpdateToggleColors(float alpha)
        {
            var colors = _toggle.colors;
            var normalColor = colors.normalColor;
            normalColor.a = alpha;
            colors.normalColor = normalColor;
            _toggle.colors = colors;
        }

        private void RefreshAfterDeselect(bool value)
        {
            if (value)
            {
                return;
            }

            if (_refreshRoutine != null)
            {
                StopCoroutine(_refreshRoutine);
            }
            _refreshRoutine = StartCoroutine(RefreshAfterDeselectRoutine());
        }

        private IEnumerator RefreshAfterDeselectRoutine() //The first selected toggle has no hover effect unless it is refreshed
        {
            yield return null;
            RefreshParentGameObject();
            _refreshRoutine = null;
        }

        private void RefreshParentGameObject()
        {
            var parent = _toggle.transform.parent;
            parent.gameObject.SetActive(false);
            parent.gameObject.SetActive(true);
        }

        protected virtual void OnDestroy()
        {
            if (_refreshRoutine != null)
            {
                StopCoroutine(_refreshRoutine);
            }

            _toggle.onValueChanged.RemoveListener(UpdateView);
        }
    }
}
