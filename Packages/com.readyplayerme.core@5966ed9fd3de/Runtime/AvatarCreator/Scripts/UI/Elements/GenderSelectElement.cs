using ReadyPlayerMe.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ReadyPlayerMe.AvatarCreator
{
    /// <summary>
    /// A Unity MonoBehaviour class for gender selection UI.
    /// Provides buttons to select a gender (male or female) and triggers events upon selection.
    /// </summary>
    public class GenderSelectElement : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField, Tooltip("Button for selecting a male avatar")]
        private Button maleButton;

        [SerializeField, Tooltip("Button for selecting a female avatar")]
        private Button femaleButton;

        [Space(5)]
        [Header("Events")]
        public UnityEvent<OutfitGender> OnGenderSelected;

        /// <summary>
        /// Adds event listeners when the element is enabled.
        /// </summary>
        private void OnEnable()
        {
            maleButton.onClick.AddListener(MaleButtonClicked);
            femaleButton.onClick.AddListener(FemaleButtonClicked);
        }

        /// <summary>
        /// Removes event listeners when the element is disabled.
        /// </summary>
        private void OnDisable()
        {
            maleButton.onClick.RemoveListener(MaleButtonClicked);
            femaleButton.onClick.RemoveListener(FemaleButtonClicked);
        }

        /// <summary>
        /// Invoked when the male selection button is clicked.
        /// Triggers the OnGenderSelected event with the Masculine gender parameter.
        /// </summary>
        private void MaleButtonClicked()
        {
            OnGenderSelected?.Invoke(OutfitGender.Masculine);
        }

        /// <summary>
        /// Invoked when the female selection button is clicked.
        /// Triggers the OnGenderSelected event with the Feminine gender parameter.
        /// </summary>
        private void FemaleButtonClicked()
        {
            OnGenderSelected?.Invoke(OutfitGender.Feminine);
        }
    }
}
