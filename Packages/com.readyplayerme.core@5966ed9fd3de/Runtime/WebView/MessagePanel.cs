using UnityEngine;
using UnityEngine.UI;

namespace ReadyPlayerMe.Core.WebView
{
    /// <summary>
    /// This class is responsible for displaying and updating a UI panel and message text.
    /// </summary>
    public class MessagePanel : MonoBehaviour
    {
        [SerializeField] private Text messageLabel;

        /// <summary>
        /// Set message from a string value.
        /// </summary>
        /// <param name="message">Message to display.</param>
        public void SetMessage(string message)
        {
            messageLabel.text = message;
        }

        /// <summary>
        /// Set message from a message type.
        /// </summary>
        /// <param name="type">Describes the option for the message that is to be displayed.</param>
        public void SetMessage(MessageType type)
        {
            messageLabel.text = type.GetValue();
        }

        /// <summary>
        /// Set message panel visibility.
        /// </summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        /// <summary>
        /// Set message panel padding in pixels.
        /// </summary>
        public void SetMargins(int left, int top, int right, int bottom)
        {
            var rect = transform as RectTransform;
            if (rect != null)
            {
                rect.offsetMax = new Vector2(-right, -top);
                rect.offsetMin = new Vector2(left, bottom);
            }
        }
    }
}
