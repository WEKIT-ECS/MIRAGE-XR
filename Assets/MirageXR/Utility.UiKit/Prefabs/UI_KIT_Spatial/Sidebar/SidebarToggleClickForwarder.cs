using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MirageXR
{
    [RequireComponent(typeof(BoxCollider))]
    public class SidebarToggleClickForwarder : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private XRSimpleInteractable interactable;

        private void Awake()
        {
            if (interactable == null)
            {
                interactable = gameObject.AddComponent<XRSimpleInteractable>();
            }
        }

        private void OnEnable()
        {
            if (interactable != null)
            {
                interactable.selectEntered.AddListener(OnSelectEntered);
            }
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
            ApplyNormalTintAlpha(toggle.isOn);
        }

        private void OnDisable()
        {
            if (interactable != null)
            {
                interactable.selectEntered.RemoveListener(OnSelectEntered);
            }
            toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }

        private void LateUpdate()
        {
            ApplyNormalTintAlpha(toggle.isOn);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            SelectToggle();
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            SelectToggle();
        }

        private void SelectToggle()
        {
            if (toggle == null || !toggle.interactable)
            {
                return;
            }

            toggle.isOn = true;
        }

        private void OnToggleValueChanged(bool isOn)
        {
            ApplyNormalTintAlpha(isOn);
        }

        private void ApplyNormalTintAlpha(bool isOn)
        {
            var colors = toggle.colors;
            var normalColor = colors.normalColor;
            var targetAlpha = isOn ? 1f : 0f;
            if (Mathf.Approximately(normalColor.a, targetAlpha))
            {
                return;
            }

            normalColor.a = targetAlpha;
            colors.normalColor = normalColor;
            toggle.colors = colors;
        }
    }
}
