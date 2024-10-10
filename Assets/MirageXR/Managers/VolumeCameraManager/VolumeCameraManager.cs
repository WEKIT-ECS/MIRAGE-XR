using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

namespace MirageXR
{
    public class VolumeCameraManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject mixedRealityPlayspace;
        [SerializeField]
        private GameObject volumeCamera;
        [SerializeField]
        private MixedRealityToolkit mixedRealityToolkit;
        [SerializeField]
        private MRTKHardwareRig mRTKHardwareRig;
        [SerializeField]
        private MixedRealityInputModule mixedRealityInputModule;
        [SerializeField]
        private GazeProvider gazeProvider;

        internal void Initialization()
        {
#if UNITY_VISIONOS || VISION_OS
            initializeVolumeCamera();

            mixedRealityToolkit.enabled = false;
            mRTKHardwareRig.enabled = false;
            mixedRealityInputModule.enabled = false;
            gazeProvider.enabled = false;
            Debug.Log("On vision os platform, MRTK scripts turned off");
#else
            Debug.Log("Not on visionos platform, no volume camera created");
            mixedRealityToolkit.enabled = true;
            mRTKHardwareRig.enabled = true;
            mixedRealityInputModule.enabled = true;
            gazeProvider.enabled = true;
#endif
        }

        public void initializeVolumeCamera()
        {
            Instantiate(volumeCamera, new Vector3(0,0,0), Quaternion.identity);
            volumeCamera.transform.parent = mixedRealityPlayspace.transform;
            Debug.Log("Volume camera initialized");
        }
    }
}
