using System.Collections.Generic;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.VisionOS
{
    /// <summary>
    /// Drop down component interactable. Used to represent a dropdown list.
    /// </summary>
    public class DropdownComponent : XRBaseInteractable
    {
        [Header("Dropdown Component Config")]
        [SerializeField]
        GameObject m_RootGameObject;

        [SerializeField]
        TMP_Text m_CurrentSelectionText;

        [SerializeField]
        List<DropDownElement> m_DropDownElements = new List<DropDownElement>();

        bool m_ShowingExpandedContent;

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();
            foreach (var element in m_DropDownElements)
            {
                element.selectEntered.AddListener(OnDrownDownSelectEntered);
            }
            SetShowExpandedContent(false);
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            base.OnDisable();
            foreach (var element in m_DropDownElements)
            {
                element.selectEntered.RemoveListener(OnDrownDownSelectEntered);
            }
        }

        /// <inheritdoc />
        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);
            SetShowExpandedContent(true);
        }

        void OnDrownDownSelectEntered(SelectEnterEventArgs args)
        {
            SetShowExpandedContent(false);
            SetSelectedElement((DropDownElement)args.interactableObject);
        }

        void SetSelectedElement(DropDownElement selectedElement)
        {
            m_CurrentSelectionText.text = selectedElement.dropDownText;
            foreach (var element in m_DropDownElements)
            {
                element.SetSelected(element == selectedElement);
            }
        }

        void SetShowExpandedContent(bool show)
        {
            m_ShowingExpandedContent = show;
            m_RootGameObject.SetActive(m_ShowingExpandedContent);
        }
    }
}
