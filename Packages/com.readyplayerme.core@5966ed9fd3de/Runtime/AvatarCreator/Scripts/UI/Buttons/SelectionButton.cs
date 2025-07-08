using System;
using UnityEngine;
using UnityEngine.UI;

namespace ReadyPlayerMe.AvatarCreator
{
    /// <summary>
    /// A basic button element with an icon and functionality to bind an action to the
    /// buttons onClick event.
    /// </summary>
    public class SelectionButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private RawImage rawImage;
        [SerializeField] private AspectRatioFitter aspectRatioFitter;
        private RectTransform rawImageRectTransform;

        /// <summary>
        /// Adds a listener to the button's onClick event.
        /// </summary>
        /// <param name="action">A function to run when the button is clicked</param>
        public void AddListener(Action action)
        {
            button.onClick.AddListener(action.Invoke);
        }

        /// <summary>
        /// Sets the icon on the rawImage component
        /// </summary>
        /// <param name="texture">The texture to be assigned to the RawImage component</param>
        public void SetIcon(Texture texture)
        {
            if (rawImageRectTransform == null)
            {
                rawImageRectTransform = rawImage.GetComponent<RectTransform>();
            }
            rawImage.texture = texture;
            if (aspectRatioFitter != null)
            {
                aspectRatioFitter.aspectRatio = (float) texture.width / texture.height;
            }
        }

        public void SetColor(string hexColor)
        {
            rawImage.color = ColorUtility.TryParseHtmlString(hexColor, out var color) ? color : Color.white;
        }
    }
}
