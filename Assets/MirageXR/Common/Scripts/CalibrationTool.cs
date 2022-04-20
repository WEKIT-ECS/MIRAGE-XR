using UnityEngine;

namespace MirageXR
{
    public class CalibrationTool : MonoBehaviour
    {
        [SerializeField] private GameObject CalibrationModel;

        public static CalibrationTool Instance { get; private set; }

        public void SetCalibrationModel(GameObject calibrationModel)
        {
            CalibrationModel = calibrationModel;
        }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);
        }


        private void Start()
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
        public void Calibrate()
        {
            // Calibrate only if the marker is visible.
            if (CalibrationModel.activeInHierarchy)
            {
                EventManager.Click();
                //WorkplaceController.Instance.CalibrateAnchors(transform);
                EventManager.CalibrateWorkplace(transform);
            }
        }
    }
}
