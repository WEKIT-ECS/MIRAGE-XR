using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class TaskStationDetailMenu : MonoBehaviour
    {
        public static TaskStationDetailMenu Instance;
        
        [SerializeField] private GameObject TSMenuPanel;
        [SerializeField] private LineRenderer descriptionLineRenderer;
        [SerializeField] private LineRenderer poiLineRenderer;
        [SerializeField] private Transform desciptionBinderPoint;
        [SerializeField] private GameObject navigatorArrowPrefab;

        public Button SelectedButton { get; set; }
        public Transform NavigatorTarget { get; set; }
        public string TargetPredicate { get; set; }
        public GameObject ActiveTaskStation => currentTSTC.gameObject;

        private TaskStationStateController currentTSTC;
        private GameObject navigatorArrowModel;

        public GameObject AddAugmentationButton { get; private set; }
        public InputField ActionTitleInputField { get; private set; }
        public InputField ActionDescriptionInputField { get; private set; }

        private void Start()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                if (Instance != this)
                {
                    Destroy(gameObject);
                }
            }

            SetupListeners();

            //deactive on start before repositioning 
            gameObject.SetActive(false);

            //instantiate the navigator arrow if does not exists in the scene
            if(!navigatorArrowModel && navigatorArrowPrefab)
            {
                navigatorArrowModel = Instantiate(navigatorArrowPrefab);
                navigatorArrowModel.GetComponentInChildren<MeshRenderer>().enabled = false;
            }
        }

        private void SetupListeners()
        {
            // Setup AugmentationCreationButton listener. Created for tutorial.
            try
            {
                var obj = gameObject.transform.FindDeepChild("AddButton");
                if (obj)
                {
                    AddAugmentationButton = obj.gameObject;
                    var button = AddAugmentationButton.GetComponent<Button>();
                    if (button)
                    {
                        button.onClick.AddListener(EventManager.NotifyOnAddAugmentationButtonClicked);
                    }
                }
            }
            catch
            {
                Debug.LogError("Augmentation Creation Button not found on Task Station Menu. Tutorial will not work.");
            }

            //Setup Title and Description Field listeners. Created for tutorial.
            try
            {
                var obj = gameObject.transform.FindDeepChild("ActionTitleInputField");
                if (obj)
                {
                    ActionTitleInputField = obj.GetComponent<InputField>();
                    if (ActionTitleInputField)
                    {
                        ActionTitleInputField.onValueChanged.AddListener(value => EventManager.NotifyOnActionStepTitleChanged());
                    }
                }
            }
            catch
            {
                Debug.LogError("Action Title Input Field not found on Task Station Menu. Tutorial will not work.");
            }

            try
            {
                var obj = gameObject.transform.FindDeepChild("DescriptionInputField");
                if (obj)
                {
                    ActionDescriptionInputField = obj.GetComponent<InputField>();
                    if (ActionDescriptionInputField)
                    {
                        ActionDescriptionInputField.onValueChanged.AddListener(value => EventManager.NotifyOnActionStepDescriptionInputChanged());
                    }
                }
            }
            catch
            {
                Debug.LogError("Action Description Input Field not found on Task Station Menu. Tutorial will not work.");
            }
        }

        public void MoveNavigatorArrow()
        {
            var navigator = navigatorArrowModel;
            
            if (navigator == null) return;
            if (NavigatorTarget == null)
            {
                navigator.GetComponentInChildren<MeshRenderer>().enabled = false;
                return;
            }

            var mainCamera = Camera.main;
            navigator.GetComponentInChildren<MeshRenderer>().enabled = true;
            var cameraTransform = mainCamera.transform;
            var fromPos =  cameraTransform.position + cameraTransform.forward;
            var toPos = mainCamera.WorldToViewportPoint(NavigatorTarget.position);

            // target is behind the camera
            if (toPos.z < mainCamera.nearClipPlane)
            {
                navigator.GetComponentInChildren<MeshRenderer>().enabled = false;
                return;
            }

            // target is found
            if (toPos.x >= 0.0f && toPos.x <= 1.0f && toPos.y >= 0.0f && toPos.y <= 1.0f)
            {
                navigator.GetComponentInChildren<MeshRenderer>().enabled = false;
                return;
            }

            toPos.x -= 0.5f;  // Translate to use center of viewport
            toPos.y -= 0.5f;
            toPos.z = 0;

            float fAngle = Mathf.Atan2(toPos.x, toPos.y);
            var cameraRotate = mainCamera.transform.eulerAngles;
            navigator.transform.eulerAngles = new Vector3(cameraRotate.x, cameraRotate.y, -fAngle * Mathf.Rad2Deg);

            navigator.transform.position = fromPos;
        }

        public void ResetTaskStationMenu(TaskStationStateController taskStationStateController)
        {
            gameObject.SetActive(true);
            transform.SetParent(null);
            //make the detail menu as a child of the task tastion diamond
            transform.position = taskStationStateController.transform.Find("DetailViewPosition").position;
            transform.SetParent(taskStationStateController.transform);
            currentTSTC = taskStationStateController;
        }

        public void SetTaskStationMenuPanel(GameObject tsMenuPanel)
        {
            TSMenuPanel = tsMenuPanel;
        }

        private void Update()
        {
            bool panelIsActive = TSMenuPanel.activeInHierarchy;

            if (currentTSTC && panelIsActive)
            {
                BindTaskStationToDescription(currentTSTC.transform);
                if (SelectedButton != null)
                {
                    BindPoiToTaskStation(currentTSTC.transform, SelectedButton.transform.Find("ButtonBinderConnector"));
                }
                else 
                {
                    if (poiLineRenderer != null)
                    {
                        poiLineRenderer.enabled = false;
                    }
                }
            }
            else
            {
                HideBinders();
            }

            MoveNavigatorArrow();
        }

        private void HideBinders()
        {
            if (descriptionLineRenderer != null)
            {
                descriptionLineRenderer.enabled = false;
            }

            poiLineRenderer.enabled = false;
        }

        public void HideDeletedBinder()
        {
            poiLineRenderer.enabled = false;
        }

        public void BindTaskStationToDescription(Transform taskStation)
        {
            if (taskStation == null) return;
            
            Vector3 descriptionBinderConnector = taskStation.Find("FaceUser/DescriptionBinderConnector").position;
            Vector3 taskStationToDescription = descriptionBinderConnector - taskStation.position;
            if (descriptionLineRenderer != null)
            {
                descriptionLineRenderer.enabled = true;
                descriptionLineRenderer.positionCount = 4;
                descriptionLineRenderer.SetPosition(0, descriptionBinderConnector);
                descriptionLineRenderer.SetPosition(1, descriptionBinderConnector + 0.035f * taskStationToDescription.normalized);
                descriptionLineRenderer.SetPosition(2, desciptionBinderPoint.position + 0.035f * desciptionBinderPoint.right);
                descriptionLineRenderer.SetPosition(3, desciptionBinderPoint.position);
            }
        }

        public void BindPoiToTaskStation(Transform taskStation, Transform target)
        {
            if (target == null || taskStation == null) return;

            Vector3 poiBinderConnector = taskStation.Find("PoiBinderConnector").position;

            poiLineRenderer.enabled = true;
            poiLineRenderer.positionCount = 4;
            poiLineRenderer.SetPosition(0, poiBinderConnector);
            poiLineRenderer.SetPosition(1, poiBinderConnector + new Vector3(0.018f, 0f, 0f));
            poiLineRenderer.SetPosition(2, target.position - new Vector3(0.02f, 0f, 0f));
            poiLineRenderer.SetPosition(3, target.position);
        }
    }
}
