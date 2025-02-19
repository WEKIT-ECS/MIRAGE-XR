using UnityEngine;
using Microsoft.MixedReality.Toolkit;
#if MRTK
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine.XR.Interaction.Toolkit.UI;
#endif

namespace MirageXR
{
    /// <summary>
    /// Responsible for initialising VisionOS components if on VisionOS, or MRTk if otherwise
    /// </summary>
    public class VolumeCameraManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Drag & drop the MixedRealityPlayspace gameobject here, so the MRTk Hardware Rig component can be added.")]
        private GameObject MixedRealityPlayspace;

        [SerializeField]
        [Tooltip("Drag & drop the Volume Camera Prefab here.")]
        private GameObject VolumeCameraPrefab;

        [Header("Add MRTK to the list of compiler directives in Project Settings / Player to switch on MRTk support.")]

        [SerializeField]
        [Tooltip("Drag and drop the MRTk configuration profile onto this slot to ResetConfiguration once MRTK is instantiated.")]
        private MixedRealityToolkitConfigurationProfile MRTKProfile;

        [SerializeField]
        [Tooltip("Drag and drop the XRInteractionManager gameobject to here - so it can be disabled for when MRTK is used.")]
        private GameObject XRInteractionManagerReference;

        private MixedRealityToolkit _mMRTk = null;

        public void Initialization()
        {
#if UNITY_VISIONOS || VISION_OS
            InitializeVolumeCamera();
#elif MRTK
            InitializeMrtk();
#endif
        }

#if UNITY_VISIONOS || VISION_OS
        private void InitializeVolumeCamera()
        {
            Debug.LogInfo("On visionOS platform: no MRTk, adding VolumeCamera");
            var volumeCamera = Instantiate(VolumeCameraPrefab, Vector3.zero, Quaternion.identity);
            RootObject.Instance.AddVolumeCamera(volumeCamera);
            Debug.LogInfo("[VolumeCameraManager] volume camera initialized.");
        }
#endif

#if MRTK
        private void InitializeMrtk()
        {
            Debug.LogInfo("Not on visionOS, no volume camera, but MRTK");

            XRInteractionManagerReference.SetActive(false);
            var baseCamera = RootObject.Instance.BaseCamera;
            baseCamera.GetComponent<XRUIInputModule>().enabled = false;

            baseCamera.gameObject.AddComponent<MixedRealityInputModule>();
            //MixedRealityPlayspace.AddComponent<MRTKHardwareRig>();
            baseCamera.gameObject.AddComponent<ConeCastGazeProvider>();

            _mMRTk ??= new GameObject("MixedRealityToolKit").AddComponent<MixedRealityToolkit>();
            _mMRTk.ResetConfiguration(MRTKProfile);
        }
#endif
    }
}
