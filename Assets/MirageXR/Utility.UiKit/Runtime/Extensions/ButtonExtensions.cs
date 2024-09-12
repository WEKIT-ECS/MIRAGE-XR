using UnityEngine.Events;
using UnityEngine.UI;

namespace Utility.UiKit.Runtime.Extensions
{
    public static class ButtonExtensions
    {
        public static bool SafeGetInteractable(this Button button) => button != null && button.interactable;
        
        public static void SafeSetInteractable(this Button button, bool value)
        {
            if (button == null)
            {
                return;
            }
            button.interactable = value;
        }
        
        public static void SafeSetListener(this Button button, UnityAction action)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(action);
            }
        }
        
        public static void SafeAddListener(this Button button, UnityAction action)
        {
            if (button != null)
            {
                button.onClick.AddListener(action);
            }
        }
        
        public static void SafeRemoveListener(this Button button, UnityAction action)
        {
            if (button != null)
            {
                button.onClick.RemoveListener(action);
            }
        }
        
        public static void SafeRemoveAllListeners(this Button button)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }
    }
}
