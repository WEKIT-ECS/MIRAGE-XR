using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Represents a class that rotates an image.
    /// </summary>
    public class RotateImage : MonoBehaviour
    {
        
        [SerializeField] private float rotationSpeed = 100f;

        
        void Update()
        {
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }
        
    }
}