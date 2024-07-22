using UnityEngine;

namespace DevDunk.LightReceiver.Sample
{
    public class LightYPosition : MonoBehaviour
    {
        public float maxPos;
        private float ogPos;

        private void Start()
        {
            ogPos = transform.position.y;
        }

        private void Update()
        {
            Vector3 pos = transform.position;
            transform.position = new Vector3(pos.x, ogPos + (Mathf.Sin(Time.time)+1) * 0.5f * maxPos, pos.z);
        }
    }
}