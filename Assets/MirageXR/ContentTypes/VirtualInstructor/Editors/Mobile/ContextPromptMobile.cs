using TMPro;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Prompt editing component for mobile setup.
    /// Allows toggling between view and edit mode, and notifies parent on changes.
    /// </summary>
    public class ContextPromptMobile : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private SpeechSettingsMobile speechSettings;
        
        private void Start()
        {
            inputField.interactable = true;
            inputField.onValueChanged.AddListener(OnTextChanged);
        }

        /// <summary>
        /// Updates the prompt whenever the text value in the input field changes.
        /// </summary>
        /// <param name="prompt">The new text value from the input field.</param>
        private void OnTextChanged(string prompt)
        {
            speechSettings.SetPrompt(prompt);
        }

        /// <summary>
        /// Updates the text displayed in the input field with the given prompt.
        /// </summary>
        /// <param name="prompt">The text to be displayed in the input field.</param>
        public void SetPromptText(string prompt)
        {
            inputField.text = prompt;
        }
    }
}