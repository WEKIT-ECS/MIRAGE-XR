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
        [Header("Containers")]
        [FormerlySerializedAs("_localModelsContainer")] [SerializeField] private RectTransform localModelsContainer;
        [FormerlySerializedAs("_scetchfabContainer")] [SerializeField] private RectTransform sketchfabContainer;
        [FormerlySerializedAs("sketchfabListItem")]
        [Header("Prefabs")]
        [SerializeField] private SketchfabListItem sketchfabListItemPrefab;
        [Header("InputField")]
        [SerializeField] private TMP_InputField _headerInputField;
        [SerializeField] private TMP_InputField _searchField;

        private SketchfabModelList _lastSketchfabModelList;
        private Content<ModelContentData> _modelContent;
        private SketchfabModel _sketchfabModel;
        private LibraryModel _localModel;
        private bool _isLibraryModel;

        protected override void OnAccept()
        {
            if (_sketchfabModel == null)
            {
                Toast.Instance.Show("No sketchfab model selected");
                return;
            }

            var step = RootObject.Instance.LEE.StepManager.CurrentStep;
            var activityId = RootObject.Instance.LEE.ActivityManager.ActivityId;

            _modelContent ??= new Content<ModelContentData>
            {
                Id = Guid.NewGuid(),
                CreationDate = DateTime.UtcNow,
                IsVisible = true,
                Steps = new List<Guid> { step.Id },
                Type = ContentType.Model,
                Version = Application.version,
                ContentData = new ModelContentData
                {
                    Triggers = null,
                    AvailableTriggers = null,
                    IsLibraryModel = false,
                    ModelUid = null,
                    LibraryModel = null
                },
                Location = Location.GetIdentityLocation()
            };

            _modelContent.ContentData.IsLibraryModel = _isLibraryModel;
            _modelContent.ContentData.ModelUid = _sketchfabModel?.Uid;
            _modelContent.ContentData.LibraryModel = _localModel;

            RootObject.Instance.LEE.ActivityManager.AddSketchfabModel(_sketchfabModel);
            RootObject.Instance.LEE.ContentManager.AddContent(_modelContent);
            if (!_isLibraryModel && _sketchfabModel != null)
            {
                RootObject.Instance.LEE.AssetsManager.UploadSketchfabModel(activityId, _sketchfabModel.Uid).Forget();
            }

            Close();
        }

        protected override bool TryToGetArguments(params object[] args)
        {
            return true;
        }

        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);

            _modelContent = _content as Content<ModelContentData>;

            toggleLocal.onValueChanged.AddListener(OnToggleLocalValueChanged);
            toggleSketchfab.onValueChanged.AddListener(OnToggleSketchfabValueChanged);
            toggleLibraries.onValueChanged.AddListener(OnToggleLibrariesValueChanged);

            _buttonBack.onClick.AddListener(OnClickBackButton);
            _buttonSettings.onClick.AddListener(OnClickSettingsButton);
            _headerInputField.onValueChanged.AddListener(OnHeaderInputFieldChanged);
            _searchField.onEndEdit.AddListener(OnInputFieldSearchEndEdit);

            InitializeLocalModelsAsync().Forget();
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
                var listItem = Instantiate(sketchfabListItemPrefab, localModelsContainer);
                listItem.InitializeAsync(sketchfabModel, OnModelItemClick).Forget();
            }
        }

        private async UniTask SearchSketchfabModels(string text)
        {
            foreach (RectTransform children in sketchfabContainer)
            {
                Destroy(children.gameObject);
            }

            var response = await sketchfabManager.SearchModelListAsync(text);
            if (!response.Success)
            {
                return;
            }

            _lastSketchfabModelList = response.Object;
            foreach (var sketchfabModel in _lastSketchfabModelList.Models)
            {
                var listItem = Instantiate(sketchfabListItemPrefab, sketchfabContainer);
                listItem.InitializeAsync(sketchfabModel, OnModelItemClick).Forget();
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
                        return;
                    }

                    sketchfabListItem.SetStatus(SketchfabListItemStatus.Error);
                    if (response.Status == SketchfabResponse.SketchfabStatus.Unauthorized)
                    {
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
            // TODO
        }

        private void OnClickBackButton()
        {
            // TODO
        }

        private void OnInputFieldSearchEndEdit(string text)
        {
            SearchSketchfabModels(text).Forget();
        }

        private void OnHeaderInputFieldChanged(string text)
        {
            // TODO
        }
    }
}
