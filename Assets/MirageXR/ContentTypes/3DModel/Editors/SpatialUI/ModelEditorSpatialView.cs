using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using LearningExperienceEngine.DataModel;
using LearningExperienceEngine.NewDataModel;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MirageXR
{
    public class ModelEditorSpatialView : EditorSpatialView
    {
        private static ISketchfabManager sketchfabManager => RootObject.Instance.LEE.SketchfabManager;

        [Header("Tabs")]
        [SerializeField] private GameObject localModels;
        [SerializeField] private GameObject sketchfabModels;
        [SerializeField] private GameObject librariesModels;
        [Header("Toggles")]
        [SerializeField] private Toggle toggleLocal;
        [SerializeField] private Toggle toggleSketchfab;
        [SerializeField] private Toggle toggleLibraries;
        [Header("Buttons")]
        [SerializeField] private Button _buttonBack;
        [SerializeField] private Button _buttonSettings;
        [SerializeField] private Button buttonSearch;
        [Header("Containers")]
        [FormerlySerializedAs("_localModelsContainer")] [SerializeField] private RectTransform localModelsContainer;
        [FormerlySerializedAs("_scetchfabContainer")] [SerializeField] private RectTransform sketchfabContainer;
        [FormerlySerializedAs("sketchfabListItem")]
        [Header("Prefabs")]
        [SerializeField] private SketchfabListItem sketchfabListItemPrefab;
        [Header("InputField")]
        [SerializeField] private TMP_InputField _headerInputField;
        [SerializeField] private TMP_InputField _searchField;
        [Header("Settings")]
        [SerializeField] private Button closeSettings;
        [SerializeField] private Toggle toggleSettingsResetPosition;
        [SerializeField] private Toggle toggleSettingsFitToScreen;
        [SerializeField] private Slider sliderScale;
        [SerializeField] private GameObject settingsContainer;
        [SerializeField] private GameObject sliderScaleContainer;
        [SerializeField] private TMP_Text textSliderValue;

        private SketchfabModelList _lastSketchfabModelList;
        private Content<ModelContentData> _modelContent;
        private SketchfabModel _sketchfabModel;
        private LibraryModel _localModel;
        private bool _isLibraryModel;
        private string _searchText;
        private readonly List<float> _scaleList = new() { 1f, 0.5f, 0.1f, 0.05f, 0.01f, 0.005f, 0.001f};
        private readonly Dictionary<string, SketchfabListItem> _sketchfabListItems = new();

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

        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);

            _modelContent = Content as Content<ModelContentData>;

            toggleLocal.onValueChanged.AddListener(OnToggleLocalValueChanged);
            toggleSketchfab.onValueChanged.AddListener(OnToggleSketchfabValueChanged);
            toggleLibraries.onValueChanged.AddListener(OnToggleLibrariesValueChanged);

            _buttonSettings.onClick.AddListener(OnClickSettingsButton);
            closeSettings.onClick.AddListener(OnClickCloseSettingsButton);
            buttonSearch.onClick.AddListener(OnButtonSearchClicked);
            _headerInputField.onValueChanged.AddListener(OnHeaderInputFieldChanged);
            _searchField.onValueChanged.AddListener(OnInputFieldSearchValueChanged);
            _searchField.onEndEdit.AddListener(OnInputFieldSearchEndEdit);

            sketchfabManager.OnResetPositionChanged += SketchfabManagerOnResetPositionChanged;
            sketchfabManager.OnFitToScreenChanged += SketchfabManagerOnFitToScreenChanged;
            sketchfabManager.OnScaleChanged += SketchfabManagerOnScaleChanged;

            toggleSettingsResetPosition.onValueChanged.AddListener(OnResetPositionChanged);
            toggleSettingsFitToScreen.onValueChanged.AddListener(OnToggleFitToScreenChanged);
            sliderScale.wholeNumbers = true;
            sliderScale.minValue = 0;
            sliderScale.maxValue = _scaleList.Count - 1;
            sliderScale.onValueChanged.AddListener(OnScaleSliderValueChanged);
            settingsContainer.SetActive(false);

            InitializeLocalModelsAsync().Forget();
        }

        private void OnScaleSliderValueChanged(float value)
        {
            sketchfabManager.Scale = _scaleList[(int)value];
        }

        private void OnResetPositionChanged(bool value)
        {
            sketchfabManager.ResetPosition = value;
        }

        private void OnToggleFitToScreenChanged(bool value)
        {
            sketchfabManager.FitToScreen = value;
        }

        private void SketchfabManagerOnResetPositionChanged(bool value)
        {
            toggleSettingsResetPosition.isOn = value;
        }

        private void SketchfabManagerOnFitToScreenChanged(bool value)
        {
            toggleSettingsFitToScreen.isOn = value;
            sliderScaleContainer.gameObject.SetActive(!value);
        }

        private void SketchfabManagerOnScaleChanged(float value)
        {
            textSliderValue.text = value.ToString("F3");
            for (var i = 0; i < _scaleList.Count; i++)
            {
                if (Mathf.Approximately(_scaleList[i], value))
                {
                    sliderScale.value = i;
                }
            }
        }

        private void OnToggleLocalValueChanged(bool value)
        {
            localModels.SetActive(value);

            if (value)
            {
                InitializeLocalModelsAsync().Forget();
            }
        }

        private void OnToggleSketchfabValueChanged(bool value)
        {
            sketchfabModels.SetActive(value);
        }

        private void OnToggleLibrariesValueChanged(bool value)
        {
            librariesModels.SetActive(value);
        }

        private async UniTask InitializeLocalModelsAsync()
        {
            foreach (RectTransform children in localModelsContainer)
            {
                Destroy(children.gameObject);
            }

            var models = await sketchfabManager.GetCachedModelsAsync();
            foreach (var sketchfabModel in models)
            {
                if (sketchfabModel != null)
                {
                    var listItem = Instantiate(sketchfabListItemPrefab, localModelsContainer);
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
                    var listItem = Instantiate(sketchfabListItemPrefab, sketchfabContainer);
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

        private void OnClickSettingsButton()
        {
            settingsContainer.SetActive(!settingsContainer.activeSelf);
        }

        private void OnClickCloseSettingsButton()
        {
            settingsContainer.SetActive(false);
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

        private void OnHeaderInputFieldChanged(string text)
        {
            // TODO
        }
    }
}
