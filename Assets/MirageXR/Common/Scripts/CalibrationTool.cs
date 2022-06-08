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
            if (CalibrationModel)
            {
                CalibrationModel.SetActive(true);
            }
        }

        public void Reset()
        {
            if (CalibrationModel)
            {
                CalibrationModel.SetActive(false);
            }
        }

        /// <summary>
        /// Calibrate workplace model anchors.
        /// </summary>
        public async void Calibrate()
        {
            // Calibrate only if the marker is visible.
            if (CalibrationModel.activeInHierarchy)
            {
                EventManager.Click();
                await RootObject.Instance.workplaceManager.CalibrateWorkplace(transform);
            }
        }
    }
}
