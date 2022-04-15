using UnityEngine;

namespace MirageXR
{
    public class CalibrationTool : MonoBehaviour
    {
        [SerializeField] private GameObject CalibrationModel;

        public static CalibrationTool Instance;

        public void SetCalibrationModel(GameObject calibrationModel)
        {
            CalibrationModel = calibrationModel;
        }

        private void Awake()
        {
            Instance = this;
        }


        private void Start ()
        {
            Reset();
        }

        public void SetPlayer()
        {
            CalibrationModel.gameObject.SetActive(true);
        }

        public void Reset()
        {
            CalibrationModel.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Calibrate workplace model anchors.
        /// </summary>
        public void Calibrate ()
        {
            // Calibrate only if the marker is visible.
            if (CalibrationModel.activeInHierarchy)
            {
                EventManager.Click();
                WorkplaceManager.Instance.CalibrateAnchors(transform);
            }
                
        }
    }
}