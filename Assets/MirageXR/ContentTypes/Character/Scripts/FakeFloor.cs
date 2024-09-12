using UnityEngine;

namespace MirageXR
{
    [RequireComponent(typeof(BoxCollider))]
    public class FakeFloor : MonoBehaviour
    {
        private const float OFFSET = -0.05f;

        private static PlaneManagerWrapper planeManager => RootObject.Instance.PlaneManager;

        private static FloorManagerWrapper floorManager => RootObject.Instance.FloorManager;


        private void Start()
        {
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
            if (RootObject.Instance == null)
            {
                return;
            }
            
            var position = transform.position;
            if (floorManager.isFloorDetected)
            {
                position.y = floorManager.floorLevel + OFFSET;
            }
            else
            {
                position.y = UIOrigin.Instance.CurrentFloorYPosition();
            }

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
