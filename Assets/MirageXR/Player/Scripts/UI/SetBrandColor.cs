using i5.Toolkit.Core.VerboseLogging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class SetBrandColor : MonoBehaviour
    {
        private static LearningExperienceEngine.BrandManager brandManager => LearningExperienceEngine.LearningExperienceEngine.Instance.BrandManager;

        public enum ColorType
        {
            PrimaryColor,
            SecondaryColor
        }

        //For classes that implements Graphic like Image, Text etc
        [SerializeField] private bool setGraphicColor = true;

        //For classes that implements Selectable like buttons
        [SerializeField] private bool setSelectableColors;
        [SerializeField] private ColorType colorType;

        private void Awake()
        {
            if (brandManager == null)
            {
                Debug.LogWarning("BrandManager has been not initialized");
                return;
            }

            var colorToUse = colorType == ColorType.PrimaryColor ? brandManager.PrimaryColor : brandManager.SecondaryColor;
            if (setGraphicColor)
            {
                GetComponent<Graphic>().color = colorToUse;
            }

            if (!setSelectableColors)
            {
                return;
            }

            var selectable = GetComponent<Selectable>();
            selectable.colors = new ColorBlock()
            {
                normalColor = selectable.colors.normalColor,
                highlightedColor = colorToUse,
                selectedColor = ShadeColor(colorToUse, 20f),
                pressedColor = ShadeColor(colorToUse, -20f),
                disabledColor = new Color(0, 0, 0, 0.3f),
                colorMultiplier = 1,
                fadeDuration = 0.1f,
            };
        }

        //Returns a lighter or darker version of the color. Negative value for darker and positive value for lighter.
        private static Color ShadeColor(Color color, float percent)
        {
            var R = color.r * 255;
            var G = color.g * 255;
            var B = color.b * 255;

            R = Mathf.RoundToInt(R * (100f + percent) / 100f);
            G = Mathf.RoundToInt(G * (100f + percent) / 100f);
            B = Mathf.RoundToInt(B * (100f + percent) / 100f);

            R = (R < 255) ? R : 255;
            G = (G < 255) ? G : 255;
            B = (B < 255) ? B : 255;

            return new Color(R / 255f, G / 255f, B / 255f);
        }
    }
}
