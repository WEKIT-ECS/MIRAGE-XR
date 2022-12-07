using UnityEngine;

namespace MirageXR
{
    public class AmbientLighting : MonoBehaviour
    {
        private readonly int _numberOfLights = 4;

        private async void Start()
        {
            DontDestroyOnLoad(gameObject);

            transform.position = Camera.main.transform.position;

            // create sun (with enabled shadow)
            var sunPrefab = await ReferenceLoader.GetAssetReferenceAsync<GameObject>("Sun");

            if (sunPrefab == null) return;

            var sun = Instantiate(sunPrefab, transform.position * Random.Range(2, 5), Quaternion.Euler(50, -30, 0));

            // disable shadow on Hololens
            if (PlatformManager.Instance.WorldSpaceUi)
                sun.GetComponent<Light>().shadows = LightShadows.None;

            // create 4 point lights
            for (int i = 0; i < _numberOfLights; i++)
            {
                Vector3 startPosition = transform.position + transform.forward * Random.Range(-5, 5) + transform.right * Random.Range(-5, 5);
                var ambientLightPrefab = await ReferenceLoader.GetAssetReferenceAsync<GameObject>("AmbientLight");
                if (ambientLightPrefab != null)
                {
                    Instantiate(ambientLightPrefab, startPosition, Quaternion.identity, transform);
                }

            }
        }

    }
}

