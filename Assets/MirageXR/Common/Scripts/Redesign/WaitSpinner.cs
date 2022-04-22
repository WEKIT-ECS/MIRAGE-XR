using UnityEngine;

namespace MirageXR
{
    public class WaitSpinner : MonoBehaviour
    {
        public float rotateSpeed = 200f;

        private void Update()
        {
            transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
        }
    }
}