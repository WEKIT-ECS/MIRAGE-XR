using UnityEngine;

namespace ReadyPlayerMe.AvatarCreator
{
    /// <summary>
    /// This class handles the rotation of the avatar based on input.
    /// </summary>
    public class AvatarRotator : MonoBehaviour
    {
        [SerializeField] private float speed = 50;

        private IAvatarRotatorInput avatarRotatorInput;

        private void Awake()
        {
            avatarRotatorInput = GetComponent<IAvatarRotatorInput>();
        }

        /// <summary>
        /// Resets the rotation of the avatar to its initial state.
        /// </summary>
        public void ResetRotation()
        {
            transform.rotation = Quaternion.identity;
        }

        private void Update()
        {
            if (avatarRotatorInput == null || !avatarRotatorInput.IsInputDetected())
            {
                return;
            }

            var rotationAmount = avatarRotatorInput.GetRotationAmount();
            transform.Rotate(Vector3.up, rotationAmount * Time.deltaTime * speed);
        }
    }
}
