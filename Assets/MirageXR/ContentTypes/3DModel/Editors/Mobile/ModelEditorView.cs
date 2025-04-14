using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using LearningExperienceEngine.DataModel;
using LearningExperienceEngine.NewDataModel;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModelEditorView : PopupEditorBase
{
    private static ISketchfabManager sketchfabManager => RootObject.Instance.LEE.SketchfabManager;
    
    private const int RESULTS_PER_PAGE = 20;
    private const float HIDED_SIZE = 100f;
    private const float HIDE_ANIMATION_TIME = 0.5f;

    public enum ModelEditorTabs
    {
        Local,
        Sketchfab,
        Library
    }

    public class SketchfabConfig
    {
        public string ClientID;
        public string ClientSecret;
    }

    public override LearningExperienceEngine.ContentType editorForType => LearningExperienceEngine.ContentType.MODEL;

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
    [Space]
    [SerializeField] private SketchfabListItem sketchfabListItemPrefab;

    private string _token;
    private string _renewToken;
    private string _searchString;
    private string _searchOption = "models";
    private GameObject _loadMoreObject;
    private ModelPreviewItem _previewItem;
    private int _pageIndex;

    private SketchfabModelList _lastSketchfabModelList;
    private readonly List<ModelListItem> _items = new List<ModelListItem>();
    private string _modelFileType;
    private ModelListItem _currentItem;
    private ModelEditorTabs _currentTab;
    private SketchfabConfig _sketchfabConfig;
    private string _searchText;
    private SketchfabModel _sketchfabModel;
    private bool _isLibraryModel;
    private LibraryModel _localModel;
    private readonly Dictionary<string, SketchfabListItem> _sketchfabListItems = new();
    private Content<ModelContentData> _modelContent;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        _toggleLocal.onValueChanged.RemoveAllListeners();
        _toggleSketchfab.onValueChanged.RemoveAllListeners();
        _toggleLibraries.onValueChanged.RemoveAllListeners();

        try
        {
            LearningExperienceEngine.UserSettings.TryGetPassword("sketchfab", out _renewToken, out _token);

            _showBackground = false;
            base.Initialization(onClose, args);

            //_btnAddFile.onClick.AddListener(OnAddLocalFile);
            //_clearSearchBtn.onClick.AddListener(ClearSearchField);
            //_btnArrow.onClick.AddListener(OnArrowButtonPressed);
            _toggleLocal.onValueChanged.AddListener(OnToggleLocalValueChanged);
            _toggleSketchfab.onValueChanged.AddListener(OnToggleSketchfabValueChanged);
            //_toggleLibraries.onValueChanged.AddListener(OnToggleLibrariesValueChanged);
            _inputSearch.onValueChanged.AddListener(OnInputFieldSearchValueChanged);
            _inputSearch.onEndEdit.AddListener(OnInputFieldSearchEndEdit);
            _pageIndex = 0;
            RootView_v2.Instance.HideBaseView();
            _modelFileType = NativeFilePicker.ConvertExtensionToFileType("fbx");
            _toggleLocal.isOn = true;

            InitializeLocalModelsAsync().Forget();
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    protected override void OnAccept()
    {
        if (_sketchfabModel == null)
        {
            Toast.Instance.Show("No sketchfab model selected");
            return;
        }

        var activityId = RootObject.Instance.LEE.ActivityManager.ActivityId;

        _modelContent = CreateContent<ModelContentData>(ContentType.Model);

        _modelContent.ContentData.IsLibraryModel = _isLibraryModel;
        _modelContent.ContentData.ModelUid = _sketchfabModel?.Uid;
        _modelContent.ContentData.LibraryModel = _localModel;

        _modelContent.ContentData.Scale = sketchfabManager.Scale;
        _modelContent.ContentData.ResetPosition = sketchfabManager.ResetPosition;
        _modelContent.ContentData.FitToScreen = sketchfabManager.FitToScreen;

        RootObject.Instance.LEE.ActivityManager.AddSketchfabModel(_sketchfabModel);
            
        if (IsContentUpdate)
        {
            RootObject.Instance.LEE.ContentManager.UpdateContent(_modelContent);
        }
        else
        {
            RootObject.Instance.LEE.ContentManager.AddContent(_modelContent);
        }
        if (!_isLibraryModel && _sketchfabModel != null)
        {
            RootObject.Instance.LEE.AssetsManager.UploadSketchfabModel(activityId, _sketchfabModel.Uid).Forget();
        }

        Close();
    }
    
    private void OnToggleSketchfabValueChanged(bool value)
    {
        _sketchfabTab.SetActive(value);
    }

    private void OnToggleLocalValueChanged(bool value)
    {
        _localTab.SetActive(value);
    }

    private async UniTask InitializeLocalModelsAsync()
    {
        foreach (RectTransform children in _contentLocalContainer)
        {
            Destroy(children.gameObject);
        }

        var models = await sketchfabManager.GetCachedModelsAsync();
        foreach (var sketchfabModel in models)
        {
            if (sketchfabModel != null)
            {
                var listItem = Instantiate(sketchfabListItemPrefab, _contentLocalContainer);
                listItem.InitializeAsync(sketchfabModel, OnModelItemClick).Forget();   
            }
        }
    }

    private async UniTask SearchSketchfabModels(string text)
    {
        foreach (var (id, item) in _sketchfabListItems)
        {
            Destroy(item.gameObject);
        }

        _sketchfabListItems.Clear();
        var response = await sketchfabManager.SearchModelListAsync(text);
        if (!response.Success)
        {
            Toast.Instance.Show("Unable to search sketchfab models");
            return;
        }

        _lastSketchfabModelList = response.Object;
        foreach (var sketchfabModel in _lastSketchfabModelList.Models)
        {
            if (!_sketchfabListItems.ContainsKey(sketchfabModel.Uid))
            {
                var listItem = Instantiate(sketchfabListItemPrefab, _contentContainer);
                listItem.InitializeAsync(sketchfabModel, OnModelItemClick).Forget();
                _sketchfabListItems.Add(sketchfabModel.Uid, listItem);
            }
        }
    }

    private void OnModelItemClick(SketchfabModel model, SketchfabListItem sketchfabListItem)
    {
        OnModelItemClickAsync(model, sketchfabListItem).Forget();
    }

    private async UniTask OnModelItemClickAsync(SketchfabModel model, SketchfabListItem sketchfabListItem)
    {
        if (!sketchfabManager.IsModelCached(model.Uid))
        {
            sketchfabListItem.SetStatus(SketchfabListItemStatus.Downloading);
            sketchfabListItem.Interactable = false;
            var cancellationToken = sketchfabListItem.gameObject.GetCancellationTokenOnDestroy();
            var response = await sketchfabManager.CacheModelAsync(model, cancellationToken);
            if (!response.IsSuccess)
            {
                if (response.Status == SketchfabResponse.SketchfabStatus.Canceled)
                {
                    Toast.Instance.Show(response.ErrorMessage);
                    return;
                }

                sketchfabListItem.SetStatus(SketchfabListItemStatus.Error);
                if (response.Status == SketchfabResponse.SketchfabStatus.Unauthorized)
                {
                    Toast.Instance.Show(response.ErrorMessage);
                    //TODO: show sketchfab auth screen
                }

                AppLog.LogError(response.ErrorMessage);
                return;
            }

            sketchfabListItem.Interactable = true;
            sketchfabListItem.SetStatus(SketchfabListItemStatus.Local);
        }
        else
        {
            _isLibraryModel = false;
            _sketchfabModel = model;
            OnAccept();
        }
    }

    private void OnInputFieldSearchValueChanged(string text)
    {
        _searchText = text;
    }

    private void OnButtonSearchClicked()
    {
        SearchSketchfabModels(_searchText).Forget();
    }

    private void OnInputFieldSearchEndEdit(string text)
    {
        _searchText = text;
        SearchSketchfabModels(_searchText).Forget();
    }
}