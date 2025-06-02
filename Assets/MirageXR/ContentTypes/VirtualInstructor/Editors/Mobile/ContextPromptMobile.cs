using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    /// <summary>
    /// Prompt editing component for mobile setup.
    /// Allows toggling between view and edit mode, and notifies parent on changes.
    /// </summary>
    public class ContextPromptMobile : MonoBehaviour
    {
        [SerializeField] private Button editButton;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_Text buttonText;
        [SerializeField] private SpeechSettingsMobile speechSettings;

        private bool _editMode;

        private void Start()
        {
            _editMode = false;
            inputField.interactable = false;
            editButton.onClick.AddListener(ToggleEdit);
        }

        private void ToggleEdit()
        {
            _editMode = !_editMode;
            inputField.interactable = _editMode;
            buttonText.text = _editMode ? "Save" : "Edit";

            if (!_editMode)
            {
                speechSettings.SetPrompt(inputField.text);
            }
        }

        public void SetPromptText(string prompt)
        {
            inputField.text = prompt;
        }
    }
}