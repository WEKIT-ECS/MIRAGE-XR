using UnityEngine;

namespace ReadyPlayerMe.AvatarCreator
{
    /// <summary>
    /// A Unity MonoBehaviour class for previewing avatars.
    /// Allows attaching and previewing a 3D avatar model, with options to reset its rotation.
    /// </summary>
    public class AvatarPreviewElement : MonoBehaviour
    {
        [SerializeField] private AvatarRotator avatarRotator;
        /// <summary>
        /// If true, the avatar will face directly at the camera when previewed.
        /// </summary>
        [Tooltip("If true the avatar will face directly at the camera"), SerializeField] private bool resetRotationOnPreview = true;
        private GameObject avatar;

        /// <summary>
        /// Preview the specified avatar by attaching it to the element.
        /// </summary>
        /// <param name="newAvatar">The GameObject representing the avatar model to be previewed.</param>
        public void PreviewAvatar(GameObject newAvatar)
        {
            if (avatar != null)
            {
                Destroy(avatar);
            }

            avatar = newAvatar;
            avatar.transform.SetParent(avatarRotator.transform);
            avatar.transform.localPosition = Vector3.zero;
            avatar.transform.localRotation = Quaternion.identity;
            if (resetRotationOnPreview)
            {
                avatarRotator.ResetRotation();
            }
        }
    }
}
