using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class CalibrationLoadingEvent : MonoBehaviour
    {
        [SerializeField] private CalibrationTool calibrationTool;

        private AudioSource audioSource;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void CalibrateNow()
        {
            calibrationTool.Calibrate();
            audioSource.Stop();
        }


        public void PlayCalibrationSound()
        {
            audioSource.Play();
        }
    }

}
