using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;

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

        [Header("Add MRTK to the list of compiler directives in Project Settings / Player to switch on MRTk support.")]

        [SerializeField]
        [Tooltip("Drag and drop the MRTk configuration profile onto this slot to ResetConfiguration once MRTK is instantiated.")]
        private MixedRealityToolkitConfigurationProfile MRTKProfile;

        private MixedRealityToolkit _mMRTk = null;

        [SerializeField]
        [Tooltip("Drag and drop the XRInteractionManager gameobject to here - so it can be disabled for when MRTK is used.")]
        private GameObject XRInteractionManagerReference;

        /// <summary>
        /// Initialise the Volume Camera on VisionOS, and MRTk otherwise
        /// </summary>
        internal void Initialization()
        {
#if UNITY_VISIONOS || VISION_OS
            Debug.LogInfo("On visionOS platform: no MRTk, adding VolumeCamera");
            initializeVolumeCamera();
#elif MRTK
            Debug.LogInfo("Not on visionOS, no volume camera, but MRTK");

            XRInteractionManagerReference.SetActive(false);
            MainCameraReference.GetComponent<XRUIInputModule>().enabled = false;

            MainCameraReference.AddComponent<MixedRealityInputModule>();
            MixedRealityPlayspace.AddComponent<MRTKHardwareRig>();
            MainCameraReference.AddComponent<ConeCastGazeProvider>();

            _mMRTk ??= new GameObject("MixedRealityToolKit").AddComponent<MixedRealityToolkit>();
            _mMRTk.ResetConfiguration(MRTKProfile);

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
