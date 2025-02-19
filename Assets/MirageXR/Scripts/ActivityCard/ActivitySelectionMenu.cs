using LearningExperienceEngine;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.UI;

namespace MirageXR
{
    public class ActivitySelectionMenu : MonoBehaviour
    {
        [SerializeField] private Text versionLabel;

        [SerializeField] private GameObject tutorialButton;
        [SerializeField] private GameObject header;
        [SerializeField] private GameObject lockButton;
        [SerializeField] private GameObject activityCreationButton;
        [SerializeField] private Toggle _toggleGrid;
        [SerializeField] private GridSettings _gridMenu;
        [SerializeField] private Transform _loginPanelPos;

        public GameObject TutorialButton => tutorialButton;
        public GameObject Header => header;
        public GameObject LockButton => lockButton;
        public GameObject ActivityCreationButton => activityCreationButton;

        private void Start()
        {
            if (!RootObject.Instance.PlatformManager.WorldSpaceUi)
            {
                GetComponent<Canvas>().enabled = false;
                return;
            }

            EventManager.OnShowActivitySelectionMenu += ShowMenu;
            EventManager.OnHideActivitySelectionMenu += HideMenu;
            EventManager.OnEditorLoaded += HideMenu;
            EventManager.OnRecorderExit += ShowMenu;

            _toggleGrid.onValueChanged.AddListener(OnToggleGridValueChanged);

            // Hide the user welcome text if no one is logged into the Moodle
            if (!LearningExperienceEngine.UserSettings.LoggedIn)
            {
                var usernameWelcomeText = transform.FindDeepChild("WelcomeUsername");
                usernameWelcomeText.gameObject.SetActive(false);
            }

            versionLabel.text = Application.productName + " v" + Application.version.Replace("-$branch", "");

            SetupListeners();
        }

        private void OnToggleGridValueChanged(bool value)
        {
            if (value)
            {
                _gridMenu.Show(_loginPanelPos.GetPose());
            }
            else
            {
                _gridMenu.Hide();
            }
        }

        private void SetupListeners()
        {
            lockButton.GetComponent<Button>().onClick.AddListener(EventManager.NotifyOnActivitySelectionMenuLockClicked);
            header.GetComponent<ObjectManipulator>().OnManipulationEnded.AddListener(delegate { EventManager.NotifyOnActivitySelectionMenuDragEnd(); });
            activityCreationButton.GetComponent<Button>().onClick.AddListener(EventManager.NotifyOnNewActivityCreationButtonPressed);
            // activityCreationButton.GetComponent<PressableButton>().ButtonPressed.AddListener(EventManager.NotifyOnNewactivityCreationButtonPressed);
        }

        private void RemoveListeners()
        {
            lockButton.GetComponent<Button>().onClick.RemoveListener(EventManager.NotifyOnActivitySelectionMenuLockClicked);
            header.GetComponent<ObjectManipulator>().OnManipulationEnded.RemoveListener(delegate { EventManager.NotifyOnActivitySelectionMenuDragEnd(); });
            // TODO: Why are there two button components here? Different platforms?
            activityCreationButton.GetComponent<Button>().onClick.RemoveListener(EventManager.NotifyOnNewActivityCreationButtonPressed);
            // activityCreationButton.GetComponent<PressableButton>().ButtonPressed.RemoveListener(EventManager.NotifyOnNewactivityCreationButtonPressed);
        }

        private void ShowMenu()
        {
            gameObject.SetActive(true);

            // Reload the activity selection list with the new saved activity
            var sessionListView = FindObjectOfType<SessionListView>();
            if (sessionListView)
                sessionListView.RefreshActivityList();
        }

        private void HideMenu()
        {
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            EventManager.OnShowActivitySelectionMenu -= ShowMenu;
            EventManager.OnHideActivitySelectionMenu -= HideMenu;
            EventManager.OnEditorLoaded -= HideMenu;
            EventManager.OnRecorderExit -= ShowMenu;

            RemoveListeners();
        }

        public async void OpenRecorder()
        {
            // Debug.Log("Open Recorder");
            // await ServiceManager.GetService<EditorSceneService>().LoadRecorderAsync();

            Loading.Instance.LoadingVisibility(true);

            await RootObject.Instance.EditorSceneService.LoadEditorAsync();
            await LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.CreateNewActivity();
        }
    }
}