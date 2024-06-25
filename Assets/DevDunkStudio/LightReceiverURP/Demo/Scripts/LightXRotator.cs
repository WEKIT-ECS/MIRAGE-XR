using UnityEngine;

namespace DevDunk.LightReceiver.Sample
{
    public class LightXRotator : MonoBehaviour
    {
        public float maxRot;
        private float ogRot;

        private void Start()
        {
            ogRot = transform.rotation.eulerAngles.x;
        }

        private void Update()
        {
            Vector3 rot = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(ogRot - Mathf.PingPong(Time.time * 10, maxRot), rot.y, rot.z);
        }
    }
}