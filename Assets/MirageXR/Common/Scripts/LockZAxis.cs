using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Component that keeps an object from rotating around the Z axis.
    /// </summary>
    public class LockZAxis : MonoBehaviour
    {
        // checks the object's rotation and resets the z-rotation to 0.
        private void Update()
        {
            var rotation = transform.eulerAngles;
            rotation.z = 0;
            transform.eulerAngles = rotation;
        }
    }
}