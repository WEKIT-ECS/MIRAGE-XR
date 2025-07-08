using UnityEngine;

namespace ReadyPlayerMe.AvatarCreator
{
    /// <summary>
    /// This class handles avatar rotation based on mouse input.
    /// </summary>
    public class MouseRotationHandler : MonoBehaviour, IAvatarRotatorInput
    {
        private const int MOUSE_BUTTON_INDEX = 0;
        private float lastPosX;
        private bool rotate;

        /// <summary>
        /// Checks if mouse input is detected for avatar rotation.
        /// </summary>
        /// <returns>True if mouse input is detected; otherwise, false.</returns>
        public bool IsInputDetected()
        {
            if (Input.GetMouseButtonDown(MOUSE_BUTTON_INDEX))
            {
                lastPosX = Input.mousePosition.x;
                rotate = true;
            }
            else if (Input.GetMouseButtonUp(MOUSE_BUTTON_INDEX))
            {
                rotate = false;
            }

            return rotate;
        }

        /// <summary>
        /// Gets the rotation amount based on mouse input.
        /// </summary>
        /// <returns>The rotation amount as a float value.</returns>
        public float GetRotationAmount()
        {
            var rotationAmount = lastPosX - Input.mousePosition.x;
            lastPosX = Input.mousePosition.x;
            return rotationAmount;
        }
    }
}
