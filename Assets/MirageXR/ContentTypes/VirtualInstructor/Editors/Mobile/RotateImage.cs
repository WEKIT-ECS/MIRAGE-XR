using UnityEngine;

namespace MirageXR
{
    public class RotateImage : MonoBehaviour
    {
     
        [SerializeField] private float rotationSpeed = 100f;

        void Update()
        {
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }
        
    }
}