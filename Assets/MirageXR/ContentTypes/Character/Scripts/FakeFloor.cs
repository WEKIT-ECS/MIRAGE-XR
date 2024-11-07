using UnityEngine;

namespace MirageXR
{
    [RequireComponent(typeof(BoxCollider))]
    public class FakeFloor : MonoBehaviour
    {
        private const float OFFSET = -0.05f;

        private static PlaneManagerWrapper planeManager => RootObject.Instance.PlaneManager;

        private static FloorManagerWithFallback floorManager => RootObject.Instance.FloorManagerWithRaycastFallback;

        private Transform _camera;

        private void Start()
        {
            _camera = Camera.main.transform;
            gameObject.GetComponent<BoxCollider>().enabled = true;
            planeManager.onDetectionEnabled.AddListener(OnDetectionEnabled);
            planeManager.onDetectionDisabled.AddListener(OnDetectionDisabled);
        }

        private void OnDestroy()
        {
            if (RootObject.Instance != null)
            {
                planeManager.onDetectionEnabled.RemoveListener(OnDetectionEnabled);
                planeManager.onDetectionDisabled.RemoveListener(OnDetectionDisabled);   
            }
        }

        private void Update()
        {
            if (RootObject.Instance is null)
            {
                return;
            }
            
            var position = transform.position;
            position.y = floorManager.GetFloorHeight(_camera.position) + OFFSET;

            transform.position = position;
        }

        private void OnDetectionEnabled()
        {
            gameObject.GetComponent<BoxCollider>().enabled = false;
        }

        private void OnDetectionDisabled()
        {
            gameObject.GetComponent<BoxCollider>().enabled = true;
        }
    }
}
