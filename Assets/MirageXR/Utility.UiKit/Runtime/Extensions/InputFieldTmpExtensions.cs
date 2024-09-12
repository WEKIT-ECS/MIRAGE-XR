using TMPro;
using UnityEngine.Events;

namespace Utility.UiKit.Runtime.Extensions
{
    public static class InputFieldTmpExtensions
    {
        public static bool SafeGetInteractable(this TMP_InputField inputField) => inputField != null && inputField.interactable;
    
        public static void SafeSetInteractable(this TMP_InputField inputField, bool value)
        {
            if (inputField == null)
            {
                return;
            }
            inputField.interactable = value;
        }
        
        public static void SafeSetListener(this TMP_InputField inputField, UnityAction<string> action)
        {
            if (inputField != null)
            {
                inputField.onValueChanged.RemoveAllListeners();
                inputField.onValueChanged.AddListener(action);
            }
        }
        
        public static void SafeAddListener(this TMP_InputField inputField, UnityAction<string> action)
        {
            if (inputField != null)
            {
                inputField.onValueChanged.AddListener(action);
            }
        }
    
        public static void SafeRemoveListener(this TMP_InputField inputField, UnityAction<string> action)
        {
            if (inputField != null)
            {
                inputField.onValueChanged.RemoveListener(action);
            }
        }
        
        public static void SafeRemoveAllListeners(this TMP_InputField inputField)
        {
            if (inputField != null)
            {
                inputField.onValueChanged.RemoveAllListeners();
            }
        }
        
        public static void SafeSetEndEditListener(this TMP_InputField inputField, UnityAction<string> action)
        {
            if (inputField != null)
            {
                inputField.onEndEdit.RemoveAllListeners();
                inputField.onEndEdit.AddListener(action);
            }
        }

        public static void SafeAddEndEditListener(this TMP_InputField inputField, UnityAction<string> action)
        {
            if (inputField != null)
            {
                inputField.onEndEdit.AddListener(action);
            }
        }

        public static void SafeRemoveEndEditListener(this TMP_InputField inputField, UnityAction<string> action)
        {
            if (inputField != null)
            {
                inputField.onEndEdit.RemoveListener(action);
            }
        }

        public static void SafeRemoveAllEndEditListeners(this TMP_InputField inputField)
        {
            if (inputField != null)
            {
                inputField.onEndEdit.RemoveAllListeners();
            }
        }
    }
}
