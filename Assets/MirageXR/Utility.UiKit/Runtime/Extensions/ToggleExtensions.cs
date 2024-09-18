using UnityEngine.Events;
using UnityEngine.UI;

namespace Utility.UiKit.Runtime.Extensions
{
    public static class ToggleExtensions
    {
        public static bool SafeGetInteractable(this Toggle button) => button != null && button.interactable;

        public static void SafeSetInteractable(this Toggle button, bool value)
        {
            if (button == null)
            {
                return;
            }

            button.interactable = value;
        }

        public static void SafeSetIsOn(this Toggle button, bool value)
        {
            if (button != null)
            {
                button.isOn = value;
            }
        }

        public static bool SafeGetIsOn(this Toggle button)
        {
            if (button != null)
            {
                return button.isOn;
            }

            return false;
        }

        public static void SafeSetIsOnWithoutNotify(this Toggle button, bool value)
        {
            if (button != null)
            {
                button.SetIsOnWithoutNotify(value);
            }
        }

        public static void SafeSetListener(this Toggle button, UnityAction<bool> action)
        {
            if (button != null)
            {
                button.onValueChanged.RemoveAllListeners();
                button.onValueChanged.AddListener(action);
            }
        }

        public static void SafeAddListener(this Toggle button, UnityAction<bool> action)
        {
            if (button != null)
            {
                button.onValueChanged.AddListener(action);
            }
        }

        public static void SafeRemoveListener(this Toggle button, UnityAction<bool> action)
        {
            if (button != null)
            {
                button.onValueChanged.RemoveListener(action);
            }
        }

        public static void SafeRemoveAllListeners(this Toggle button)
        {
            if (button != null)
            {
                button.onValueChanged.RemoveAllListeners();
            }
        }
    }
}