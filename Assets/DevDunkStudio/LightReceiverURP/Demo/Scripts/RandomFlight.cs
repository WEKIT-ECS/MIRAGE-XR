using UnityEngine;

namespace DevDunk.LightReceiver.Sample
{
    public class RandomFlight : MonoBehaviour
    {
        public Vector3 maxOffset;
        Vector3 startPos;
        Vector3 endPos;
        Vector3 currentPos;
        float timer;
        void Start()
        {
            currentPos = startPos = transform.position;
            endPos = startPos + new Vector3(Random.Range(-maxOffset.x, maxOffset.x), Random.Range(-maxOffset.y, maxOffset.y), Random.Range(-maxOffset.z, maxOffset.z));
            timer = 0;
        }

        // Update is called once per frame
        void Update()
        {
            transform.position = Vector3.Lerp(currentPos, endPos, timer);
            timer += Time.deltaTime * 0.3f;
            if (timer > 1)
            {
                timer = 0;
                currentPos = endPos;
                endPos = startPos + new Vector3(Random.Range(-maxOffset.x, maxOffset.x), Random.Range(-maxOffset.y, maxOffset.y), Random.Range(-maxOffset.z, maxOffset.z));
            }
        }
    }
}