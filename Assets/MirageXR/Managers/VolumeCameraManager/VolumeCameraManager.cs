using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MirageXR
{
    public class VolumeCameraManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject mixedRealityPlayspace;
        [SerializeField]
        private GameObject volumeCamera;

        internal void Initialization()
        {
#if UNITY_VISIONOS || VISION_OS
            initializeVolumeCamera();
            Debug.Log("Volume camera created");
#else
            Debug.Log("Not on visionos platform, no volume camera created");
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
