using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

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
        [SerializeField]
        [Tooltip("Drag and drop the main camera gameobject here, to attach the MRTk MixedRealityInputModule and ConeCastGazeProvider components.")]
        private GameObject MainCameraReference;

        [SerializeField]
        [Tooltip("Drag and drop the MRTk configuration profile onto this slot to ResetConfiguration once MRTk is instantiated.")]
        private MixedRealityToolkitConfigurationProfile MRTKProfile;

        private MixedRealityToolkit _mMRTk = null;

        /// <summary>
        /// Initialise the Volume Camera on VisionOS, and MRTk otherwise
        /// </summary>
        internal void Initialization()
        {
#if UNITY_VISIONOS || VISION_OS
            Debug.LogInfo("On visionOS platform: no MRTk, adding VolumeCamera");
            initializeVolumeCamera();
#else
            Debug.LogInfo("Not on visionOS, no volume camera, adding MRTk");

            _mMRTk ??= new GameObject("MixedRealityToolKit").AddComponent<MixedRealityToolkit>();
            _mMRTk.ResetConfiguration(_mMRTkprofile);

            MixedRealityPlayspace.AddComponent<MRTKHardwareRig>();
            MainCameraReference.AddComponent<MixedRealityInputModule>();
            MainCameraReference.AddComponent<ConeCastGazeProvider>();
#endif
        }

        /// <summary>
        /// Instantiate the volume camera prefab
        /// </summary>
        public void initializeVolumeCamera()
        {
            Instantiate(VolumeCameraPrefab, new Vector3(0,0,0), Quaternion.identity);
            Debug.LogInfo("[VolumeCameraManager] volume camera initialized.");
        }
    }
}
