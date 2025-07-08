using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ReadyPlayerMe.AvatarCreator
{
    /// <summary>
    /// A Unity MonoBehaviour class for displaying and confirming images.
    /// Allows users to set an image to be displayed and confirms it upon user interaction.
    /// </summary>
    public class PhotoConfirmationElement : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private RawImage photoTextureTarget;

        [Space(5)]
        [Header("Events")]
        public UnityEvent<Texture2D> OnPhotoConfirmed;

        /// <summary>
        /// Sets the texture to be displayed in the element.
        /// </summary>
        /// <param name="texture">The Texture2D to be displayed.</param>
        public void SetTexture(Texture2D texture)
        {
            photoTextureTarget.texture = texture;
        }

        /// <summary>
        /// Confirms the currently displayed image and invokes the "onPhotoConfirmed" event with the confirmed texture.
        /// </summary>
        public void ConfirmPhoto()
        {
            OnPhotoConfirmed?.Invoke((Texture2D) photoTextureTarget.texture);
        }
    }
}
