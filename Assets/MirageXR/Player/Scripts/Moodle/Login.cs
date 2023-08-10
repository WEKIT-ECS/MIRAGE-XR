using i5.Toolkit.Core.VerboseLogging;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class Login : MonoBehaviour
    {
        [SerializeField] private GameObject loginCanvas;
        [SerializeField] private GameObject signinPanel;
        [SerializeField] private GameObject signoutPanel;
        [SerializeField] private GameObject siteConfigurationPanel;
        [SerializeField] private GameObject notificationPanel;
        [SerializeField] private Text logoutText;
        [SerializeField] private Text welcomUserText;
        [SerializeField] private InputField usernameField;
        [SerializeField] private InputField passwordField;
        [SerializeField] private InputField moodleURLText;
        [SerializeField] private Text siteConfigurationStatusLabel;
        [SerializeField] private Transform loginPanelPosition;
        [SerializeField] private GameObject AutenticationOpenButton;
        [SerializeField] private Toggle publicUploadToggle;
        [SerializeField] private Toggle rememberLogin;
        [SerializeField] private int moodleForumID;
        [SerializeField] private GameObject developerOptionsPanel;
        [SerializeField] private Toggle showConsole;
        [SerializeField] private GameObject developerConsole;


        [SerializeField] private GameObject lrsPanel;

        public Text status;

        private void Start()
        {
            status.text = string.Empty;
            siteConfigurationStatusLabel.text = string.Empty;
            publicUploadToggle.isOn = DBManager.publicUploadPrivacy;

            if (PlayerPrefs.HasKey("MoodleURL"))
            {
                ShowPanel(null); // hide login panel
            }
            else
            {
                OpenURLConfigurationPanel();
            }

            EventManager.OnHideActivitySelectionMenu += HideLoginPanel;
            EventManager.OnEditorLoaded += HideLoginPanel;

            AutoLogin();
        }

        private void OnDestroy()
        {
            EventManager.OnHideActivitySelectionMenu -= HideLoginPanel;
            EventManager.OnEditorLoaded -= HideLoginPanel;
        }


        public void SetXApi(Dropdown option)
        {
            switch (option.value)
            {
                case 0:
                    EventManager.NotifyxAPIChanged(DBManager.LearningRecordStores.WEKIT);
                    break;
            }
        }


        /// <summary>
        /// Cal UserLogin by the login button
        /// </summary>
        public async void CallLogin()
        {
            SaveRememberToggle();
            await UserLogin();
        }

        private void AutoLogin()
        {
            if (!DBManager.rememberUser) return;

            rememberLogin.isOn = true;

            if (!DBManager.LoggedIn && LocalFiles.TryToGetUsernameAndPassword(out var username, out var password))
            {
                usernameField.text = username;
                passwordField.text = password;

                CallLogin();
            }
        }

        private void SaveRememberToggle()
        {
            DBManager.rememberUser = rememberLogin.isOn;
        }

        /// <summary>
        /// Ask Moodle for token
        /// </summary>
        /// <returns></returns>
        private async Task UserLogin()
        {
            var (result, response) = await Network.LoginRequestAsync(usernameField.text, passwordField.text, DBManager.domain);
            if (result && response.StartsWith("succeed"))
            {
                // login succeed, send the token to be added into DBManager
                LoginSucceed(response.Split(',')[1]);
            }
            else
            {
                Debug.LogError($"User login failed. Error: {response}");
                status.color = Color.red;
                status.text = "Invalid login, please try again";
            }
        }

        /// <summary>
        /// Save token and the username into DBManager
        /// Send a welcome message to the user on activity selector
        /// </summary>
        /// <param name="token"></param>
        private async void LoginSucceed(string token)
        {
            var moodleManager = RootObject.Instance.moodleManager;
            DBManager.token = token;
            DBManager.username = usernameField.text;
            welcomUserText.text = $"Welcome {DBManager.username}";
            welcomUserText.gameObject.SetActive(true);
            Debug.LogInfo($"{DBManager.username} logged in successfully.");
            status.text = string.Empty;
            // close login menu
            ShowPanel(null);
            await moodleManager.GetUserId();
            await moodleManager.GetUserMail();
            Maggie.Speak(welcomUserText.text);

            // encrypt and save the login info
            EncryptAndSave();

            // reload the activity list
            var sessionListView = FindObjectOfType<SessionListView>();
            sessionListView.RefreshActivityList();
        }

        private void EncryptAndSave()
        {
            if (rememberLogin.isOn)
            {
                LocalFiles.SaveUsernameAndPassword(usernameField.text, passwordField.text);
            }
            else
            {
                LocalFiles.RemoveUsernameAndPassword();
            }
        }

        /// <summary>
        /// validate the username and password failed.
        /// </summary>
        public void VerifyInput()
        {
            const int usernameMinLength = 3;
            const int passwordMinLength = 4;

            if (usernameField.text.Length == 0)
                status.text = "Enter your username";
            else if (usernameField.text.Length < usernameMinLength)
                status.text = $"Username must be more than {usernameMinLength} characters";
            else if (passwordField.text.Length == 0)
                status.text = "Enter your password";
            else if (passwordField.text.Length < passwordMinLength)
                status.text = $"Password must be more than {passwordMinLength} characters";
            else
                status.text = string.Empty;

            transform.FindDeepChild("LoginButton").GetComponent<Button>().interactable = usernameField.text.Length >= usernameMinLength && passwordField.text.Length >= passwordMinLength; // TODO: possible NRE

            // remove space
            if (usernameField.text.Contains(" "))
            {
                usernameField.text = usernameField.text.Trim(' ');
                status.text = "Username cannot contain space.";
                status.color = Color.red;
            }
        }

        public async void Signout()
        {
            DBManager.LogOut();
            ShowPanel(signinPanel);
            PlayerPrefs.SetInt("guest", 0);
            welcomUserText.text = string.Empty;

            // reload the activity list
            var sessionListView = FindObjectOfType<SessionListView>();
            await sessionListView.CollectAvailableSessionsAsync();
        }

        private void HideLoginPanel()
        {
            ShowPanel(null);
        }

        public void ShowPanel(GameObject pnl)
        {
            signinPanel.SetActive(false);
            signoutPanel.SetActive(false);
            notificationPanel.SetActive(false);
            loginCanvas.SetActive(false);
            siteConfigurationPanel.SetActive(false);
            lrsPanel.SetActive(false);
            developerOptionsPanel.SetActive(false);

            var loginToggle = AutenticationOpenButton.GetComponent<SpriteToggle>();

            if (pnl != null)
            {
                loginCanvas.SetActive(true);
                pnl.SetActive(true);
                loginToggle.IsSelected = true;
            }
            else
            {
                loginToggle.IsSelected = false;
            }
        }

        public void ShowLogin()
        {
            var loginToggle = AutenticationOpenButton.GetComponent<SpriteToggle>();

            if (loginToggle.IsSelected)
            {
                HideLoginPanel();
            }
            else
            {
                if (DBManager.LoggedIn)
                {
                    logoutText.text = $"you are already logged in,\n\t<b>{DBManager.username}</b>";
                    ShowPanel(signoutPanel);
                }
                else if (!PlayerPrefs.HasKey("MoodleURL"))
                {
                    OpenURLConfigurationPanel();
                }
                else
                {
                    ShowPanel(signinPanel);
                }

                loginToggle.IsSelected = true;

                transform.position = loginPanelPosition.position;
                transform.rotation = loginPanelPosition.rotation;
            }
        }

        public void RegisterMoodle()
        {
            Application.OpenURL(DBManager.registerPage);
        }


        public void Support()
        {
            Application.OpenURL($"{DBManager.domain}/mod/forum/view.php?id={moodleForumID}");
        }


        public void CloseGuestNotification()
        {
            PlayerPrefs.SetInt("guest", 1);
            ShowPanel(null);
        }

        public void ShowGuestNotification()
        {
            ShowPanel(notificationPanel);
        }

        public void ShowLogout()
        {
            ShowPanel(signoutPanel);
        }

        public void Cancel()
        {
            ShowPanel(null);
        }

        public void SaveMoodleSettings()
        {
            if (moodleURLText.text == string.Empty)
            {
                siteConfigurationStatusLabel.text = "URL cannot be empty!";
                return;
            }

            string url;
            if (moodleURLText.text.ToLower().StartsWith("http://") || moodleURLText.text.ToLower().StartsWith("https://"))
            {
                url = moodleURLText.text.ToLower();
            }
            else if (moodleURLText.text.ToLower().StartsWith("www"))
            {
                url = $"https://{moodleURLText.text.ToLower()}";
            }
            else
            {
                siteConfigurationStatusLabel.text = "Please add a valid URL";
                return;
            }

            DBManager.domain = url;
            DBManager.publicUploadPrivacy = publicUploadToggle.isOn;

            Maggie.Speak("Settings saved successfully.");

            ShowLogin();
        }

        public void OpenURLConfigurationPanel()
        {
            ShowPanel(siteConfigurationPanel);
            moodleURLText.text = DBManager.domain;
        }

        public void ShowLRSSettings()
        {
            ShowPanel(lrsPanel);
        }

        public void ShowDeveloperOptions()
        {
            ShowPanel(developerOptionsPanel);
        }

        public void MoodleConfigurationCancel()
        {
            ShowLogin();
        }
    }
}