using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    /// <summary>
    /// Automatically adjusts the height of a TMP_InputField based on its content.
    /// Updates the layout when the input text changes to ensure proper resizing 
    /// of the prompt field. Used for dynamic resizing of prompt input fields in
    /// the instructor setup UI.
    /// </summary>
    public class AutoResizeInputField : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _tmpInputField;
        [SerializeField] private ContentSizeFitter _contentSizeFitter;
        private void Awake()
        {
            _contentSizeFitter.SetLayoutVertical();
            _tmpInputField.onValueChanged.AddListener(OnTextChanged);
        }

        private void OnTextChanged(string text)
        {
            _contentSizeFitter.SetLayoutVertical();
        }
        
        private void OnDestroy()
        {
            _tmpInputField.onValueChanged.RemoveListener(OnTextChanged);
        }
    }
}
