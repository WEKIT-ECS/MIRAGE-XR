using UnityEngine;

namespace ReadyPlayerMe.Samples.QuickStart
{
    public class LookAtCamera : MonoBehaviour
    {
        [SerializeField] private GameObject cam;

        private void Update()
        {
            transform.LookAt(cam.transform);
        }
    }
}
