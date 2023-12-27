using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DG.Tweening;
using i5.Toolkit.Core.OpenIDConnectClient;
using i5.Toolkit.Core.ServiceCore;
using MirageXR;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModelEditorView : PopupEditorBase
{
    private const int RESULTS_PER_PAGE = 20;
    private const float HIDED_SIZE = 100f;
    private const float HIDE_ANIMATION_TIME = 0.5f;

    public enum ModelEditorTabs
    {
        Local,
        Sketchfab,
        Library
    }

    public override ContentType editorForType => ContentType.MODEL;

    [SerializeField] private Transform _contentContainer;
    [SerializeField] private ScrollRect _scroll;
    [SerializeField] private Transform _contentLocalContainer;
    [SerializeField] private ScrollRect _scrollLocal;
    [SerializeField] private ModelListItem _modelListItemPrefab;
    [SerializeField] private DirectLoginPopup _directLoginPopupPrefab;
    [SerializeField] private GameObject _loadMorePrefab;
    //[SerializeField] private Button _btnSearch;
    [SerializeField] private Button _btnLogout;
    [SerializeField] private Button _btnAddFile;
    [SerializeField] private Button _clearSearchBtn;
    [SerializeField] private TMP_InputField _inputSearch;
    [Space]
    [SerializeField] private Toggle _toggleLocal;
    [SerializeField] private Toggle _toggleSketchfab;
    [SerializeField] private Toggle _toggleLibraries;
    [SerializeField] private ClientDataObject _clientDataObject;
    [SerializeField] private ClientDataObject _clientDirectLoginDataObject;
    [Space]
    [SerializeField] private GameObject _localTab;
    [SerializeField] private GameObject _sketchfabTab;
    [SerializeField] private GameObject _librariesTab;
    [SerializeField] private GameObject _bottomButtonsPanel;
    [SerializeField] private GameObject _localEmptyPanel;
    [SerializeField] private GameObject _localPanel;
    [Space]
    [SerializeField] private Button _btnArrow;
    [SerializeField] private RectTransform _panel;
    [SerializeField] private GameObject _arrowDown;
    [SerializeField] private GameObject _arrowUp;
    [Space]
    [SerializeField] private ModelLibraryManager _libraryManager;

    private string _token;
    private string _renewToken;
    private string _searchString;
    private string _searchOption = "models";
    private GameObject _loadMoreObject;
    private ModelPreviewItem _previewItem;
    private int _pageIndex;

    private readonly List<ModelListItem> _items = new List<ModelListItem>();
    private string _modelFileType;
    private ModelListItem _currentItem;
    private ModelEditorTabs _currentTab;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        _toggleLocal.onValueChanged.RemoveAllListeners();
        _toggleSketchfab.onValueChanged.RemoveAllListeners();
        _toggleLibraries.onValueChanged.RemoveAllListeners();

        try
        {
            LocalFiles.TryGetPassword("sketchfab", out _renewToken, out _token);

            _showBackground = false;
            base.Initialization(onClose, args);

            _btnLogout.onClick.AddListener(OnLogoutClicked);
            _btnAddFile.onClick.AddListener(OnAddLocalFile);
            _clearSearchBtn.onClick.AddListener(ClearSearchField);
            _btnArrow.onClick.AddListener(OnArrowButtonPressed);
            _toggleLocal.onValueChanged.AddListener(OnToggleLocalValueChanged);
            _toggleSketchfab.onValueChanged.AddListener(OnToggleSketchfabValueChanged);
            _toggleLibraries.onValueChanged.AddListener(OnToggleLibrariesValueChanged);
            _inputSearch.onValueChanged.AddListener(OnInputFieldSearchChanged);
            _pageIndex = 0;
            RootView_v2.Instance.HideBaseView();
            _modelFileType = NativeFilePicker.ConvertExtensionToFileType("fbx");

            _toggleLocal.isOn = true;
            _toggleLibraries.isOn = false;
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    private void ResetView()
    {
        try
        {
            if (LocalFiles.TryGetPassword("sketchfab", out _renewToken, out _token))
            {
                if (!string.IsNullOrEmpty(_renewToken) && DBManager.isNeedToRenewSketchfabToken)
                {
                    RenewToken();
                }
            }
            else
            {
                DialogWindow.Instance.Show("Login to Sketchfab",
                    new DialogButtonContent("Via the browser", LoginToSketchfab),
                    new DialogButtonContent("With password", ShowDirectLoginPopup),
                    new DialogButtonContent("Cancel", Close));
            }

            Clear();
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    private async void RenewToken()
    {
        await RenewTokenAsync();
    }

    private async Task RenewTokenAsync()
    {
        try
        {
            var clientId = _clientDirectLoginDataObject.clientData.ClientId;
            var clientSecret = _clientDirectLoginDataObject.clientData.ClientSecret;

            var (result, json) = await MirageXR.Sketchfab.RenewTokenAsync(_renewToken, clientId, clientSecret);
            var response = JsonConvert.DeserializeObject<MirageXR.Sketchfab.SketchfabResponse>(json);
            if (result)
            {
                if (response.error == null && response.access_token != null)
                {
                    _token = response.access_token;
                    _renewToken = response.refresh_token;
                    LocalFiles.SaveLoginDetails("sketchfab", _renewToken, _token);
                    DBManager.sketchfabLastTokenRenewDate = DateTime.Today;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    private bool CheckAndLoadCredentials()
    {
        try
        {
            if (_clientDataObject == null)
            {
                var json = Resources.Load<TextAsset>("Credentials/SketchfabClient");
                var appCredentials = JsonUtility.FromJson<SketchfabManager.AppCredentials>(json.ToString());
                if (appCredentials != null)
                {
                    _clientDataObject = ScriptableObject.CreateInstance<ClientDataObject>();
                    _clientDataObject.clientData =
                        new ClientData(appCredentials.client_id, appCredentials.client_secret);
                }
            }

            if (_clientDataObject == null)
            {
                DialogWindow.Instance.Show(
                    "Sketchfab client asset not found or reference is missing.\n\nContact the MirageXR development team for more information or access to the file.",
                    new DialogButtonContent("Close", Close));
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            return false;
        }
    }

    private void OnToggleLocalValueChanged(bool value)
    {
        if (value)
        {
            if (!CheckAndLoadCredentials())
            {
                ResetView();
                return;
            }

            _currentTab = ModelEditorTabs.Local;
            _localTab.SetActive(true);
            _sketchfabTab.SetActive(false);
            _librariesTab.SetActive(false);
            _bottomButtonsPanel.SetActive(true);
            ShowLocalModels();
        }
    }

    private void OnToggleSketchfabValueChanged(bool value)
    {
        _inputSearch.text = string.Empty;
        if (value)
        {
            if (!CheckAndLoadCredentials())
            {
                ResetView();
                return;
            }

            _currentTab = ModelEditorTabs.Sketchfab;
            _localTab.SetActive(false);
            _sketchfabTab.SetActive(true);
            _librariesTab.SetActive(false);
            _bottomButtonsPanel.SetActive(false);
            ShowRemoteModels();
        }
    }

    private void OnToggleLibrariesValueChanged(bool value)
    {
        if (value)
        {
            _currentTab = ModelEditorTabs.Library;
            _localTab.SetActive(false);
            _sketchfabTab.SetActive(false);
            _librariesTab.SetActive(true);
            _bottomButtonsPanel.SetActive(false);
            _libraryManager.EnableCategoryButtons(this);
        }
    }

    private void ShowLocalModels()
    {
        Clear();
        var previewItems = MirageXR.Sketchfab.GetLocalModels();
        if (previewItems.Count == 0)
        {
            _localEmptyPanel.SetActive(true);
            _localPanel.SetActive(false);
            return;
        }
        _localEmptyPanel.SetActive(false);
        _localPanel.SetActive(true);
        AddItems(previewItems, true);
    }

    private void ShowRemoteModels()
    {
        Clear();
    }

    private void AddLoadMoreButton()
    {
        _loadMoreObject = Instantiate(_loadMorePrefab, _contentContainer);
        _loadMoreObject.GetComponentInChildren<Button>().onClick.AddListener(OnLoadMoreButtonClicked);
    }

    private void OnSearchClicked()
    {
        if (_currentTab == ModelEditorTabs.Local)
        {
            SearchLocal();
        }
        else
        {
            SearchRemote();
        }
    }

    private void OnInputFieldSearchChanged(string text)
    {
        // TODO
    }

    public void OnStartSearch()
    {
        if (_inputSearch.text != string.Empty)
        {
            SearchRemote();
        }
    }

    private void SearchLocal()
    {
        foreach (var item in _items)
        {
            var active = string.IsNullOrEmpty(_inputSearch.text) || item.previewItem.name.Contains(_inputSearch.text);
            item.gameObject.SetActive(active);
        }
    }

    private async void SearchRemote()
    {
        try
        {
            _searchString = _inputSearch.text;

            if (string.IsNullOrEmpty(_searchString))
            {
                Toast.Instance.Show("Search field cannot be empty");
                return;
            }

            _pageIndex = 0;
            var (result, content) = await MirageXR.Sketchfab.SearchModelsAsync(_token, _searchOption, _searchString,
                _pageIndex, RESULTS_PER_PAGE);
            if (!result)
            {
                Toast.Instance.Show("Error when trying to get a list of models.");
                return;
            }

            Clear();
            var previewItems = MirageXR.Sketchfab.ReadWebResults(content, _searchOption);
            if (previewItems.Count == 0)
            {
                Toast.Instance.Show("Nothing found");
                return;
            }

            AddItems(previewItems);
            if (previewItems.Count >= RESULTS_PER_PAGE)
            {
                AddLoadMoreButton();
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    private void ClearSearchField()
    {
        _inputSearch.text = string.Empty;
    }

    private async void OnLoadMoreButtonClicked()
    {
        try
        {
            _pageIndex++;
            var cursorPosition = _pageIndex * RESULTS_PER_PAGE + 1;

            var (result, content) = await MirageXR.Sketchfab.SearchModelsAsync(_token, _searchOption, _searchString, cursorPosition, RESULTS_PER_PAGE);
            if (!result)
            {
                Toast.Instance.Show("Error when trying to get a list of models.");
                return;
            }

            var previewItems = MirageXR.Sketchfab.ReadWebResults(content, _searchOption);
            if (previewItems.Count == 0)
            {
                _loadMoreObject.SetActive(false);
                Toast.Instance.Show("Nothing to download");
                return;
            }

            AddItems(previewItems);

            if (previewItems.Count < RESULTS_PER_PAGE)
            {
                _loadMoreObject.SetActive(false);
            }

            _loadMoreObject.transform.SetAsLastSibling();
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    private void Clear()
    {
        try
        {
            //disable the library items
            _libraryManager.DisableCategoryButtons();

            _pageIndex = 0;
            _scroll.normalizedPosition = Vector2.up;
            for (int i = _contentContainer.childCount - 1; i >= 0; i--)
            {
                var child = _contentContainer.GetChild(i);
                Destroy(child.gameObject);
            }
            _scrollLocal.normalizedPosition = Vector2.up;
            for (int i = _contentLocalContainer.childCount - 1; i >= 0; i--)
            {
                var child = _contentLocalContainer.GetChild(i);
                Destroy(child.gameObject);
            }

            _items.Clear();
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    private void AddItems(IEnumerable<ModelPreviewItem> previewItems, bool isDownloaded = false)
    {
        foreach (var item in previewItems)
        {
            if (_currentTab == ModelEditorTabs.Sketchfab)
            {
                var model = Instantiate(_modelListItemPrefab, _contentContainer);
                model.Init(item, isDownloaded, DownloadItem, Accept, null, null);
                _items.Add(model);
            }
            else if (_currentTab == ModelEditorTabs.Local)
            {
                var model = Instantiate(_modelListItemPrefab, _contentLocalContainer);
                model.Init(item, isDownloaded, DownloadItem, Accept, RemoveLocalItemAsync, RenameLocalItemAsync);
                _items.Add(model);
            }
        }

        UpdateModelsItemView();
    }

    private void RemoveLocalItemAsync(ModelListItem item)
    {
        RemoveModelFromLocalStorageAsync(item).AsAsyncVoid();
    }

    private void RenameLocalItemAsync(ModelListItem item)
    {
        _currentItem = item;
        RootView_v2.Instance.dialog.ShowBottomInputField(
           "New title:",
           "Enter new title",
           "Cancel", null,
           "Save", EnterNewTitle);
    }

    private void EnterNewTitle(string text)
    {
        if (!string.IsNullOrWhiteSpace(text) && IsValidNewTitle(text))
        {
            RenameLocalModelAsync(text, _currentItem).AsAsyncVoid();
        }
        else
        {
            Debug.Log("Invalid text format");
        }
    }

    private bool IsValidNewTitle(string value)
    {
        const string regexExpression = @"^[^/\\?!]+$";
        var regex = new Regex(regexExpression);
        return regex.IsMatch(value);
    }

    private async Task RenameLocalModelAsync(string newName, ModelListItem item)
    {
        _previewItem = item.previewItem;
        await MirageXR.Sketchfab.RenameLocalModelAsync(newName, _previewItem);
        ShowLocalModels();
    }

    private async Task RemoveModelFromLocalStorageAsync(ModelListItem item)
    {
        _previewItem = item.previewItem;
        await MirageXR.Sketchfab.RemoveLocalModelAsync(_previewItem);
        ShowLocalModels();
    }

    private async void UpdateModelsItemView()
    {
        const int cooldown = 25;
        foreach (var item in _items)
        {
            await item.LoadImage();
            await Task.Delay(cooldown);
        }
    }

    private async void LoginToSketchfab()
    {
        try
        {
            var service = ServiceManager.GetService<OpenIDConnectService>();
            service.OidcProvider.ClientData = _clientDataObject.clientData;
            service.LoginCompleted += OnLoginCompleted;
            service.ServerListener.ListeningUri = SketchfabManager.URL;
            await service.OpenLoginPageAsync();
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    private void ShowDirectLoginPopup()
    {
        var clientId = _clientDirectLoginDataObject.clientData.ClientId;
        var clientSecret = _clientDirectLoginDataObject.clientData.ClientSecret;
        Action<bool, string> onPopupClose = OnDirectLoginCompleted;
        PopupsViewer.Instance.Show(_directLoginPopupPrefab, onPopupClose, clientId, clientSecret);
    }

    private void OnDirectLoginCompleted(bool result, string json)
    {
        if (result)
        {
            var response = JsonConvert.DeserializeObject<MirageXR.Sketchfab.SketchfabResponse>(json);
            _token = response.access_token;
            _renewToken = response.refresh_token;
            LocalFiles.SaveLoginDetails("sketchfab", _renewToken, _token);
            Toast.Instance.Show("Authorization was successful");
            DBManager.sketchfabLastTokenRenewDate = DateTime.Now;
            return;
        }
        Close();
    }

    private void OnLoginCompleted(object sender, EventArgs e)
    {
        var service = ServiceManager.GetService<OpenIDConnectService>();
        service.LoginCompleted -= OnLoginCompleted;
        _token = service.AccessToken;
        LocalFiles.SaveLoginDetails("sketchfab", string.Empty, _token);
        Toast.Instance.Show("Authorization was successful");
        DBManager.sketchfabLastTokenRenewDate = DateTime.Now;
    }

    private void Accept(ModelListItem item)
    {
        AcceptAsync(item).AsAsyncVoid();
    }

    private async Task AcceptAsync(ModelListItem item)
    {
        _previewItem = item.previewItem;
        item.interactable = false;
        await MirageXR.Sketchfab.LoadModelAsync(_previewItem);
        item.interactable = true;
        OnAccept();
    }

    private void DownloadItem(ModelListItem item)
    {
        DownloadItemAsync(item).AsAsyncVoid();
    }

    private async Task DownloadItemAsync(ModelListItem item)
    {
        try
        {
            var (result, downloadInfo) = await MirageXR.Sketchfab.GetDownloadInfoAsync(_token, item.previewItem);
            if (!result)
            {
                await RenewTokenAsync();
                (result, downloadInfo) = await MirageXR.Sketchfab.GetDownloadInfoAsync(_token, item.previewItem);
                if (!result)
                {
                    Logout();
                    Toast.Instance.Show("The session is out of date. Re-login is required.");
                    return;
                }
            }

            item.OnBeginDownload();
            if (await MirageXR.Sketchfab.DownloadModelAndExtractAsync(downloadInfo.gltf.url, item.previewItem, item.OnDownload))
            {
                Toast.Instance.Show("Download successfully.");
                item.isDownloaded = true;
                item.OnEndDownload();
                return;
            }

            item.OnEndDownload();
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }

        Toast.Instance.Show("Download error. Please, try again.");
    }

    private void OnLogoutClicked()
    {
        DialogWindow.Instance.Show("Are you sure you want to logout?", new DialogButtonContent("Yes", Logout),
            new DialogButtonContent("No"));
    }


    private void Logout()
    {
        LocalFiles.RemoveKey("sketchfab");
        ResetView();
    }

    protected override void OnAccept()
    {
        _previewItem.name = ZipUtilities.CheckFileForIllegalCharacters(_previewItem.name);
        AddAugmentation(_previewItem.name);
    }



    public void AddAugmentation(string prefabName, bool libraryModel = false)
    {
        var predicate = $"3d:{prefabName}";
        if (_content != null)
        {
            EventManager.DeactivateObject(_content);
        }
        else
        {
            _content = augmentationManager.AddAugmentation(_step, GetOffset());
            _content.option = prefabName;
            _content.predicate = predicate;
            _content.url = predicate;
            _content.text = libraryModel ? ModelLibraryManager.LibraryKeyword : string.Empty;
        }

        _content.predicate = predicate;
        EventManager.ActivateObject(_content);

        base.OnAccept();

        Close();
    }



    private void OnArrowButtonPressed()
    {
        if (_arrowDown.activeSelf)
        {
            var hidedSize = HIDED_SIZE;
            _panel.DOAnchorPosY(-_panel.rect.height + hidedSize, HIDE_ANIMATION_TIME);
            _arrowDown.SetActive(false);
            _arrowUp.SetActive(true);
        }
        else
        {
            _panel.DOAnchorPosY(0.0f, HIDE_ANIMATION_TIME);
            _arrowDown.SetActive(true);
            _arrowUp.SetActive(false);
        }
    }

    private void OnAddLocalFile()
    {
        // Don't attempt to import/export files if the file picker is already open
        if (NativeFilePicker.IsFilePickerBusy())
        {
            return;
        }

        // Pick a 3D Model file
        NativeFilePicker.Permission permission = NativeFilePicker.PickFile(
            (path) =>
            {
                if (path == null)
                {
                    Debug.Log("Operation cancelled");
                }
                else
                {
                    Debug.Log("Picked file: " + path);

                    // TODO: add local model to the list
                }
            }, new string[] { _modelFileType });

        Debug.Log("Permission result: " + permission);
    }

    private void OnDestroy()
    {
        RootView_v2.Instance.ShowBaseView();
    }
}