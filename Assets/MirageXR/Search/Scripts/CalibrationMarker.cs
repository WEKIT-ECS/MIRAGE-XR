using UnityEngine;


namespace MirageXR
{
    public class CalibrationMarker : MonoBehaviour
    {
        private bool _calibratorSpawned = false;
        [SerializeField] private GameObject calibrator;
        [SerializeField] private GameObject selectionMenu;

        private GameObject _actionList;

        // Update is called once per frame
        void Update()
        {
            if (calibrator.activeInHierarchy)
            {
                UiManager.Instance.SetPosition(calibrator);
            }

            if (!_actionList && FindObjectOfType<ActionListMenu>())
                _actionList = FindObjectOfType<ActionListMenu>().gameObject;

        }

        public void SpawnCalibrator()
        {
            if (!_calibratorSpawned)
            {
                calibrator.SetActive(true);
                _calibratorSpawned = true;
            }
            else
            {
                calibrator.SetActive(false);
                _calibratorSpawned = false;
            }

        }

        public void Ontap()
        {

            SpawnCalibrator();
            UiManager.Instance.SetPosition(selectionMenu);
        }


        public void ShowActionList()
        {
            if (_actionList == null || !GameObject.Find(_actionList.name)) return;
            UiManager.Instance.SetPosition(_actionList);
            UiManager.Instance.LookAtCamera(_actionList);
        }

    }

}
