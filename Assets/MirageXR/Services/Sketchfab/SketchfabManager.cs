using i5.Toolkit.Core.OpenIDConnectClient;
using i5.Toolkit.Core.ServiceCore;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


namespace MirageXR
{
    public class SketchfabManager : MonoBehaviour
    {
        public const string URL = "http://127.0.0.1:52072";
        public const int RESULTS_PER_PAGE = 10;
        private const float RETRY_MAX_COUNT = 3;

        // paging button control
        [SerializeField] private Text ResultsShownText;
        [SerializeField] private Button NextPageButton;
        [SerializeField] private Button PrevPageButton;
        [SerializeField] private InputField InputFieldUserName;
        [SerializeField] private InputField InputFieldPassword;
        [SerializeField] private Button BtnLoginWithPassword;
        [SerializeField] private Button BtnCloseLoginWithPassword;
        [SerializeField] private Toggle ToggleRememberMe;
        [SerializeField] private GameObject PanelLoginWithPassword;
        [SerializeField] private ClientDataObject SketchfabDataObject;
        [SerializeField] private ClientDataObject SketchfabLoginWithPasswordDataObject;

        private float _retryCount;
        private bool _hasAccessToken;
        private bool _viewingLocalModels = false;

        private string _currentPathOption = "sketchfab";
        private string _currentSearchOption = "models";
        private string _currentSearchString = string.Empty;
        private int _currentSearchPage;

        private string _nextSearchResults = string.Empty;
        private string _prevSearchResults = string.Empty;

        private List<ModelPreviewItem> _currentPreviewItems = new List<ModelPreviewItem>();

        private string _token;
        private string _renewToken;

        public class AppCredentials
        {
            public string client_id = string.Empty;
            public string client_secret = string.Empty;
        }

        #region Start and Stop

        private void OnEnable()
        {
            ProcessDeepLinkMngr.Instance.OnDeepLinkReceived += DeepLinkReceiver;
        }

        private void OnDisable()
        {
            ProcessDeepLinkMngr.Instance.OnDeepLinkReceived -= DeepLinkReceiver;
        }

        private void Start()
        {
            BtnLoginWithPassword.onClick.AddListener(LoginWithPassword);
            BtnCloseLoginWithPassword.onClick.AddListener(CloseLoginWithPassword);
            NextPageButton.onClick.AddListener(NextResultsPage);
            PrevPageButton.onClick.AddListener(PreviousResultsPage);
            ToggleRememberMe.isOn = LearningExperienceEngine.DBManager.rememberSketchfabUser;
            ToggleRememberMe.onValueChanged.AddListener(value => LearningExperienceEngine.DBManager.rememberSketchfabUser = value);

            CheckForAppCredentials();
            _hasAccessToken = LearningExperienceEngine.LocalFiles.TryGetPassword("sketchfab", out _renewToken, out _token);

            // TODO: check validity of existing access tokens, remove obsolete entry where needed
            // step 1:
            // use the SketchfabAuthorizationFlowAnswer, which is accessed in the SketchfabOIDCProvider class of the i5 Toolkit
            // speak to BH about adding a function that returns the 'expires_in' parameter of the webresponse.

            if (_hasAccessToken)
            {
                StartSession();
                if (!string.IsNullOrEmpty(_renewToken) && LearningExperienceEngine.DBManager.isNeedToRenewSketchfabToken)
                {
                    RenewToken();
                }
            }
            else
            {
                ToggleDisplayPanels("login");
            }

            OAuthClient.Instance.onAuthenticationFailedOrCancelled += reason => Debug.Log("Login Failed!\n" + reason);
            OAuthClient.Instance.onValidatingUser += () => Debug.Log("On Validating User..");
            OAuthClient.Instance.onTokenReceivedSuccessfully += () => Debug.Log("Login Successful!");
        }

        private async void RenewToken()
        {
            var clientId = SketchfabLoginWithPasswordDataObject.clientData.ClientId;
            var clientSecret = SketchfabLoginWithPasswordDataObject.clientData.ClientSecret;
            var (result, json) = await Sketchfab.RenewTokenAsync(_renewToken, clientId, clientSecret);
            var response = JsonConvert.DeserializeObject<Sketchfab.SketchfabResponse>(json);
            if (result)
            {
                if (response.error != null || response.access_token == null)
                {
                    _token = response.access_token;
                    _renewToken = response.refresh_token;
                    LearningExperienceEngine.LocalFiles.SaveLoginDetails("sketchfab", _renewToken, _token);
                    LearningExperienceEngine.DBManager.sketchfabLastTokenRenewDate = DateTime.Today;
                }
            }
        }

        private void CloseLoginWithPassword()
        {
            ToggleDisplayPanels("login");
        }

        private async void LoginWithPassword()
        {
            var clientId = SketchfabLoginWithPasswordDataObject.clientData.ClientId;
            var clientSecret = SketchfabLoginWithPasswordDataObject.clientData.ClientSecret;
            var (result, json) = await Sketchfab.GetTokenAsync(InputFieldUserName.text, InputFieldPassword.text, clientId, clientSecret);
            var response = JsonConvert.DeserializeObject<Sketchfab.SketchfabResponse>(json);
            if (result)
            {
                if (response.error != null || response.access_token == null)
                {
                    DialogWindow.Instance.Show("Please, check your login/password", new DialogButtonContent("Ok"));
                    return;
                }
                if (ToggleRememberMe.isOn)
                {
                    LearningExperienceEngine.LocalFiles.SaveLoginDetails("direct_login_sketchfab", InputFieldUserName.text, InputFieldPassword.text);
                }
                _token = response.access_token;
                _renewToken = response.refresh_token;
                _hasAccessToken = true;
                LearningExperienceEngine.LocalFiles.SaveLoginDetails("sketchfab", _renewToken, _token);
                StartSession();
                LearningExperienceEngine.DBManager.sketchfabLastTokenRenewDate = DateTime.Now;
                return;
            }

            DialogWindow.Instance.Show("Please, check your internet connection", new DialogButtonContent("Ok"));
        }

        private void StartSession()
        {
            Debug.Log("Sketchfab session started");
            ToggleDisplayPanels(_currentPathOption);
        }

        #endregion Start and Stop

        #region Public Functions

        /// <summary>
        /// Opens the Sketchfab model selector panel.
        /// </summary>
        public async void SketchfabLogin_Click()
        {
            var service = ServiceManager.GetService<OpenIDConnectService>();
            service.OidcProvider.ClientData = SketchfabDataObject.clientData;
            service.LoginCompleted += LoginCompleted;
            service.ServerListener.ListeningUri = URL;
            await service.OpenLoginPageAsync();
        }

        /// <summary>
        /// Opens the Sketchfab model selector panel.
        /// </summary>
        public void SketchfabLoginWithPassword_Click()
        {
            ToggleDisplayPanels("loginWithPassword");
            if (ToggleRememberMe.isOn && LearningExperienceEngine.LocalFiles.TryGetPassword("direct_login_sketchfab", out var username, out var password))
            {
                InputFieldUserName.text = username;
                InputFieldPassword.text = password;
            }
        }

        /// <summary>
        /// Opens the local model selector panel, avoiding Sketchfab authentication.
        /// </summary>
        public void LocalModelsOnly_Click()
        {
            ToggleDisplayPanels("local");
        }

        /// <summary>
        /// Searches the Sketchfab repository for models, based on the search term(s) present in the input field.
        /// </summary>
        public void PerformSearch()
        {
            int cursorPosition = _currentSearchPage * RESULTS_PER_PAGE;
            string searchString = Sketchfab.GetSearchUrl(_currentSearchOption, _currentSearchString, cursorPosition, RESULTS_PER_PAGE);
            StartCoroutine(GetSketchfabResults(searchString, _token, SketchfabSearchCallback));
        }

        /// <summary>
        /// Selects the next page of models (default page size is 10).
        /// </summary>
        public void NextResultsPage()
        {
            if (_viewingLocalModels)
            {
                GetComponent<ModelManager>().IncrementPage();
            }
            else
            {
                _currentSearchPage++;
                StartCoroutine(GetSketchfabResults(_nextSearchResults, _token, SketchfabSearchCallback));
            }
        }

        /// <summary>
        /// Selects the previous page of models (default page size is 10).
        /// </summary>
        public void PreviousResultsPage()
        {
            if (_viewingLocalModels)
            {
                GetComponent<ModelManager>().DecrementPage();
            }
            else
            {
                _currentSearchPage--;
                StartCoroutine(GetSketchfabResults(_prevSearchResults, _token, SketchfabSearchCallback));
            }
        }

        /// <summary>
        /// Triggered by the UI to update the search option field
        /// </summary>
        /// <param name="dd"></param>
        public void UpdateSearchOption(Dropdown dd)
        {
            _currentSearchOption = dd.options[dd.value].text.ToLower();
        }

        /// <summary>
        /// Catches changes to the search input field.
        /// </summary>
        /// <param name="ff"></param>
        public void UpdateSearchString(InputField ff)
        {
            _currentSearchString = ff.text.ToLower();
        }

        /// <summary>
        /// Triggered by the UI, updates the model location (local or remote).
        /// </summary>
        /// <param name="pp"></param>
        public void UpdatePathString(Dropdown pp)
        {
            _currentPathOption = pp.options[pp.value].text.ToLower();
            pp.Hide();
            ToggleDisplayPanels(_currentPathOption.ToLower());
        }

        /// <summary>
        /// Connect the SketchfabManager class to the ModelManager to initiate downloads.
        /// </summary>
        /// <param name="buttonHandler"></param>
        /// <param name="modelPreview"></param>
        public void BeginModelDownload(ButtonHandler buttonHandler, ModelPreviewItem modelPreview)
        {
            const float megabytesInBytes = 1024f * 1024f;
            const float maxSizeWithoutConfirm = 30f;

            var modelManager = GetComponent<ModelManager>();
            StartCoroutine(GetModelDownloadInfo(_token, modelPreview, info =>
            {
                modelPreview.fileSize = info.gltf.size / megabytesInBytes;
                if (info.gltf.size > 0 && info.gltf.expires > 0)
                {
                    if (modelPreview.fileSize > maxSizeWithoutConfirm)
                    {
                        buttonHandler.ConfirmLargeFileDownload(modelPreview, info, modelManager.DownloadModel);
                    }
                    else
                    {
                        modelManager.DownloadModel(info.gltf.url, modelPreview);
                    }
                }
            }));
        }

        #endregion Public Functions

        #region Authentication and Login

        private void CheckForAppCredentials()
        {
            var hasClientAsset = CheckForClientAsset();
            var noAccessPanel = transform.Find("panelLogin/panelNoClientAccess").gameObject;

            noAccessPanel.SetActive(!hasClientAsset);
        }

        private bool CheckForClientAsset()
        {
            if (SketchfabDataObject == null)
            {
                var newCreds = Resources.Load<TextAsset>("Credentials/SketchfabClient");
                var appCreds = JsonUtility.FromJson<AppCredentials>(newCreds.ToString());
                if (appCreds != null)
                {
                    SketchfabDataObject = ScriptableObject.CreateInstance<ClientDataObject>();
                    SketchfabDataObject.clientData = new ClientData(appCreds.client_id, appCreds.client_secret);
                }
            }

            if (SketchfabDataObject == null ||
                string.IsNullOrEmpty(SketchfabDataObject.clientData.ClientId) ||
                string.IsNullOrEmpty(SketchfabDataObject.clientData.ClientSecret))
            {
                //Debug.Log("No Sketchfab credentials found. Please contact the MirageXR development team for access to the sketchfab account.");
                return false;
            }

            return true;
        }

        // Receives the auth code from the Deep Link Manager
        private void DeepLinkReceiver(string authCode)
        {
            if (!string.IsNullOrEmpty(authCode))
            {
                StartCoroutine(RequestToken(authCode, AccessTokenReceiver));
            }
        }

        // Access token receiver
        private void AccessTokenReceiver(string receivedToken)
        {
            _token = receivedToken;
            LearningExperienceEngine.LocalFiles.SaveLoginDetails("sketchfab", string.Empty, _token);
            StartSession();
        }

        // Receives the access token from the i5 service layer
        private void LoginCompleted(object sender, EventArgs e)
        {
            var service = ServiceManager.GetService<OpenIDConnectService>();
            service.LoginCompleted -= LoginCompleted;
            _token = service.AccessToken;
            LearningExperienceEngine.LocalFiles.SaveLoginDetails("sketchfab", string.Empty, _token);
            StartSession();
            LearningExperienceEngine.DBManager.sketchfabLastTokenRenewDate = DateTime.Now;
        }

        private IEnumerator RequestToken(string code, Action<string> callbackTokenReceived)
        {
            const string redirectUri = "https://wekit-community.org/sketchfab/callback.php";
            const string postUri = "https://sketchfab.com/oauth2/token/";

            var appID = SketchfabDataObject.clientData.ClientId;
            var appSecret = SketchfabDataObject.clientData.ClientSecret;

            var form = new WWWForm();
            form.AddField("client_id", appID);
            form.AddField("redirect_uri", redirectUri);
            form.AddField("code", code);
            form.AddField("grant_type", "authorization_code");
            form.AddField("client_secret", appSecret);

            var www = UnityWebRequest.Post(postUri, form);
            www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            yield return www.SendWebRequest();
            if (www.error != null)
            {
                callbackTokenReceived("error");
            }
            else
            {
                var response = www.downloadHandler.text;
                var tokenJson = JsonUtility.FromJson<AccessTokenResponseJson>(response);

                callbackTokenReceived(!string.IsNullOrEmpty(tokenJson.access_token) ? tokenJson.access_token : "empty");
            }
            yield return null;
        }

        public void OnLogoutClicked()
        {
            DialogWindow.Instance.Show("Are you sure you want to logout?", new DialogButtonContent("Yes", Logout),
                new DialogButtonContent("No"));
        }


        private void Logout()
        {
            LearningExperienceEngine.LocalFiles.RemoveKey("sketchfab");
            ToggleDisplayPanels("login");
        }

        #endregion Authentication and Login

        #region Search Functions

        private void ToggleDisplayPanels(string pathOption)
        {
            pathOption = pathOption.ToLower();

            switch (pathOption)
            {
                case "sketchfab" when ServiceManager.GetService<OpenIDConnectService>().IsLoggedIn || _hasAccessToken:
                    ShowRemoteModelsPage();
                    break;
                case "sketchfab":
                    ShowLoginPage();
                    break;
                case "local":
                    NextPageButton.interactable = true;
                    PrevPageButton.interactable = true;
                    ShowLocalModelsPage();
                    break;
                case "login":
                    ShowLoginPage();
                    break;
                case "loginwithpassword":
                    ShowLoginWithPasswordPage();
                    break;
                default:
                    Debug.Log($"Unrecognised path option: {pathOption}");
                    break;
            }
        }

        private void ShowRemoteModelsPage() //TODO: possible NRE
        {
            transform.Find("panelLogin").gameObject.SetActive(false);
            PanelLoginWithPassword.SetActive(false);
            transform.Find("panelSearch").gameObject.SetActive(true);
            transform.Find("panelSearch/btnSearch").gameObject.GetComponent<Button>().interactable = true;
            transform.Find("panelSearch/cmbSearchPath").gameObject.GetComponent<Dropdown>().SetValueWithoutNotify(0);
            transform.Find("panelLocalPreview").gameObject.SetActive(false);
            transform.Find("panelSketchfabPreview").gameObject.SetActive(true);
            transform.Find("panelPaging").gameObject.SetActive(true);
            GetComponent<ScrollableListPopulator>().SetWebButtonTextures();
            _viewingLocalModels = false;
            DisplayCurrentSearchRange();
        }

        private void ShowLocalModelsPage() //TODO: possible NRE
        {
            transform.Find("panelLogin").gameObject.SetActive(false);
            PanelLoginWithPassword.SetActive(false);
            transform.Find("panelSearch").gameObject.SetActive(true);
            transform.Find("panelSearch/btnSearch").gameObject.GetComponent<Button>().interactable = false;
            transform.Find("panelSearch/cmbSearchPath").gameObject.GetComponent<Dropdown>().SetValueWithoutNotify(1);
            transform.Find("panelLocalPreview").gameObject.SetActive(true);
            transform.Find("panelSketchfabPreview").gameObject.SetActive(false);
            transform.Find("panelPaging").gameObject.SetActive(true);
            _viewingLocalModels = true;
            GetComponent<ModelManager>().PopulateModelPreview();
        }

        private void ShowLoginPage() //TODO: possible NRE
        {
            transform.Find("panelLogin").gameObject.SetActive(true);
            PanelLoginWithPassword.gameObject.SetActive(false);
            transform.Find("panelSearch").gameObject.SetActive(false);
            transform.Find("panelLocalPreview").gameObject.SetActive(false);
            transform.Find("panelSketchfabPreview").gameObject.SetActive(false);
        }

        private void ShowLoginWithPasswordPage() //TODO: possible NRE
        {
            PanelLoginWithPassword.gameObject.SetActive(true);
            transform.Find("panelLogin").gameObject.SetActive(false);
            transform.Find("panelSearch").gameObject.SetActive(false);
            transform.Find("panelLocalPreview").gameObject.SetActive(false);
            transform.Find("panelSketchfabPreview").gameObject.SetActive(false);
            transform.Find("panelPaging").gameObject.SetActive(false);
        }

        private void SketchfabSearchCallback(string result)
        {
            ResultsShownText.text = string.Empty;

            var formattedResult = JsonUtility.FromJson<SketchfabModelSearchResult>(result);
            SetResultPaging(formattedResult.next, formattedResult.previous);

            _currentPreviewItems = Sketchfab.ReadWebResults(result, _currentSearchOption);

            var listPopulator = GetComponent<ScrollableListPopulator>();
            listPopulator.ClearPreviousSearches(false);
            listPopulator.MakeScrollingList(_currentPreviewItems, false);
        }

        private void SetResultPaging(string nextResults, string prevResults)
        {
            _nextSearchResults = nextResults;
            _prevSearchResults = prevResults;
            NextPageButton.interactable = !string.IsNullOrEmpty(_nextSearchResults);
            PrevPageButton.interactable = !string.IsNullOrEmpty(_prevSearchResults);

            DisplayCurrentSearchRange();
        }

        private void DisplayCurrentSearchRange()
        {
            int searchResultStartIndex = (_currentSearchPage * 10) + 1;
            int searchResultsEndIndex = searchResultStartIndex + (RESULTS_PER_PAGE - 1);
            ResultsShownText.text = $"{searchResultStartIndex} - {searchResultsEndIndex}";
        }

        #endregion Search Functions

        /// <summary>
        /// Requests gltf model information from sketchfab.
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="modelPreview">Model preview data.</param>
        /// <param name="callback">The function to run when dowload information is available.</param>
        /// <returns></returns>
        public static IEnumerator GetModelDownloadInfo(string token, ModelPreviewItem modelPreview, Action<ModelDownloadInfo> callback)
        {
            var webReqUrl = $"https://api.sketchfab.com/v3/models/{modelPreview.uid}/download";
            using var www = UnityWebRequest.Get(webReqUrl);
            www.SetRequestHeader("Authorization", $"Bearer {token}");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = www.downloadHandler.text;
                var downloadInfo = JsonUtility.FromJson<ModelDownloadInfo>(response);
                callback(downloadInfo);
            }
            else
            {
                DialogWindow.Instance.Show("Error downloading model, try re-logging in", new DialogButtonContent("Ok"));

                Debug.Log(www.error);
            }
        }

        public static IEnumerator GetSketchfabResults(string url, string token, Action<string> callbackReceivedCode)
        {
            const float cooldown = 0.5f;
            var retryCount = 0;
            while (retryCount < RETRY_MAX_COUNT)
            {
                retryCount++;

                using var request = new UnityWebRequest(url);
                request.SetRequestHeader("Authorisation", $"Bearer {token}");
                var handler = new DownloadHandlerBuffer();
                request.downloadHandler = handler;

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.ConnectionError && request.result != UnityWebRequest.Result.ProtocolError)
                {
                    if (request.responseCode == 200)
                    {
                        Debug.Log("Request finished successfully");
                        callbackReceivedCode(handler.text);
                        break;
                    }

                    Debug.Log(request.responseCode == 401
                        ? "Error 401: Unauthorized. Resubmitted request!"
                        : $"Request failed (status:{request.responseCode})");
                }

                yield return new WaitForSeconds(cooldown);
            }
        }

        public static IEnumerator DownloadImage(string url, Action<Sprite> callback)
        {
            const string httpPrefix = "http";
            const string filePrefix = "file://";

            var texture = new Texture2D(2, 2);
            if (url.StartsWith(httpPrefix))
            {
                using var www = UnityWebRequestTexture.GetTexture(url);

                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.ConnectionError && www.result != UnityWebRequest.Result.ProtocolError)
                {
                    texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                }

            }
            else if (url.StartsWith(filePrefix))
            {
                var imagePath = url.Substring(filePrefix.Length);
                if (File.Exists(imagePath))
                {
                    var fileData = File.ReadAllBytes(imagePath);
                    texture.LoadImage(fileData);
                }
            }
            else
            {
                Debug.Log("Unrecognised link for thumbnail image");
            }

            var rec = new Rect(0, 0, texture.width, texture.height);
            var ppu = Mathf.Max(texture.width / 9f, texture.height / 6f);
            var sprite = Sprite.Create(texture, rec, new Vector2(0.5f, 0.5f), ppu);

            callback?.Invoke(sprite);
        }
    }
}