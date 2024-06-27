using UnityEngine;

namespace ReadyPlayerMe.Samples.QuickStart
{
    public class GroundCheck : MonoBehaviour
    {
        [SerializeField] [Tooltip("Useful for rough ground")]
        private float groundedOffset = -0.22f;
        [SerializeField] [Tooltip("The radius of the grounded check")]
        private float groundRadius = 0.28f;
        [SerializeField] [Tooltip("Defines which layers to check for collisions (Should be different from player layer)")]
        private LayerMask groundMask;

        public bool IsGrounded()
        {
            var position = transform.position;
            Vector3 spherePosition = new Vector3(position.x, position.y + groundedOffset,
                position.z);
            return Physics.CheckSphere(spherePosition, groundRadius, groundMask);
        }
    }
}
