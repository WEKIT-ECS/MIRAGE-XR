using UnityEngine;

namespace MirageXR
{
    public class AmbientLighting : MonoBehaviour
    {
        private readonly int numberOfLights = 4;

        void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            transform.position = Camera.main.transform.position;

            //create sun (with enabled shadow)
            var sun = Instantiate(Resources.Load<GameObject>("Prefabs/Sun"), transform.position * Random.Range(2, 5), Quaternion.Euler(50, -30, 0));

            //disable shadow on Hololens
            if (PlatformManager.Instance.WorldSpaceUi)
                sun.GetComponent<Light>().shadows = LightShadows.None;

                //create 4 point lights 
                for (int i = 0; i < numberOfLights; i++)
                {
                    Vector3 startPosition = transform.position + transform.forward * Random.Range(-5, 5) + transform.right * Random.Range(-5, 5);
                    Instantiate(Resources.Load("Prefabs/AmbientLight"), startPosition, Quaternion.identity, transform);
                }
        }

    }
}

