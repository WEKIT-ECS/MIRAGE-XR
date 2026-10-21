using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Responsible for initialising VisionOS components if on VisionOS.
    /// </summary>
    public class VolumeCameraManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Drag & drop the Volume Camera Prefab here.")]
        private GameObject VolumeCameraPrefab;

        public void Initialization()
        {
#if UNITY_VISIONOS || VISION_OS
            InitializeVolumeCamera();
#endif
        }

#if UNITY_VISIONOS || VISION_OS
        private void InitializeVolumeCamera()
        {
            Debug.LogInfo("On visionOS platform: adding VolumeCamera");
            var volumeCamera = Instantiate(VolumeCameraPrefab, Vector3.zero, Quaternion.identity);
            Debug.LogInfo("[VolumeCameraManager] volume camera initialized.");
        }
#endif
    }
}
