using System.Collections;
using TMPro;
using UnityEngine;

namespace Utility.UiKit.Runtime.Extensions
{
    public static class TmpTextExtensions
    {
        public static string SafeGetText(this TMP_Text text) => text == null ? string.Empty : text.text;
        public static void SafeSetText(this TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }
    
        public static Color SafeGetColor(this TMP_Text text) => text == null ? Color.magenta : text.color;
        public static void SafeSetColor(this TMP_Text text, Color color)
        {
            if (text != null)
            {
                text.color = color;
            }
        }
    }
}