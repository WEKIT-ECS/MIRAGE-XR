using i5.Toolkit.Core.ServiceCore;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.UI;

namespace MirageXR
{
    public class ActivitySelectionMenu : MonoBehaviour
    {
        [SerializeField] private Text versionLabel;

        public GameObject TutorialButton;
        public GameObject Header;
        public GameObject LockButton;
        public GameObject ActivityCreationButton;

        private void Start()
        {
            if (!PlatformManager.Instance.WorldSpaceUi)
            {
                GetComponent<Canvas>().enabled = false;
                return;
            }
            
            EventManager.OnShowActivitySelectionMenu += ShowMenu;
            EventManager.OnHideActivitySelectionMenu += HideMenu;
            EventManager.OnEditorLoaded += HideMenu;
            EventManager.OnRecorderExit += ShowMenu;

            //Hide the user welcome text if no one is logged into the Moodle
            if (!DBManager.LoggedIn)
            {
                var usernameWelcomeText = transform.FindDeepChild("WelcomeUsername");
                usernameWelcomeText.gameObject.SetActive(false);
            }

            versionLabel.text = Application.productName + " v" + Application.version.Replace("-$branch", "");

            AssignGameObjects();
            SetupListeners();
        }

        private void AssignGameObjects()
        {
            try
            {
                this.LockButton = this.gameObject.transform.Find("Lock").gameObject;
                this.Header = this.gameObject.transform.Find("Panel").Find("Header").gameObject;
                this.ActivityCreationButton = this.gameObject.transform.Find("Panel").Find("ButtonAdd").gameObject;
            }
            catch
            {
                Debug.LogError("Could not find one or more children of ActivitySelectionMenu. Tutorial will not work.");
            }
        }

        private void SetupListeners()
        {
            LockButton.GetComponent<Button>().onClick.AddListener(EventManager.NotifyOnActivitySelectionMenuLockClicked);
            Header.GetComponent<ObjectManipulator>().OnManipulationEnded.AddListener(delegate { EventManager.NotifyOnActivitySelectionMenuDragEnd(); });
            ActivityCreationButton.GetComponent<Button>().onClick.AddListener(EventManager.NotifyOnNewActivityCreationButtonPressed);
            //ActivityCreationButton.GetComponent<PressableButton>().ButtonPressed.AddListener(EventManager.NotifyOnNewActivityCreationButtonPressed);
        }

        private void RemoveListeners()
        {
            LockButton.GetComponent<Button>().onClick.RemoveListener(EventManager.NotifyOnActivitySelectionMenuLockClicked);
            Header.GetComponent<ObjectManipulator>().OnManipulationEnded.RemoveListener(delegate { EventManager.NotifyOnActivitySelectionMenuDragEnd(); });
            // TODO: Why are there two button components here? Different platforms?
            ActivityCreationButton.GetComponent<Button>().onClick.RemoveListener(EventManager.NotifyOnNewActivityCreationButtonPressed);
            //ActivityCreationButton.GetComponent<PressableButton>().ButtonPressed.RemoveListener(EventManager.NotifyOnNewActivityCreationButtonPressed);
        }

        private void ShowMenu()
        {
            gameObject.SetActive(true);
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
            //Debug.Log("Open Recorder");
            //await ServiceManager.GetService<EditorSceneService>().LoadRecorderAsync();

            Loading.Instance.LoadingVisibility(true);

            await ServiceManager.GetService<EditorSceneService>().LoadEditorAsync();
            EventManager.ParseActivity(string.Empty);
        }
    }
}