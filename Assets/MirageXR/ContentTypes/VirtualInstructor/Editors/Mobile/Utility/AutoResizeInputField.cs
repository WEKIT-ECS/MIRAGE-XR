using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
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
