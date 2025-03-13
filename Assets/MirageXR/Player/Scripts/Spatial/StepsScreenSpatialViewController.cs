using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using UnityEngine;

namespace MirageXR
{
    public class StepsScreenSpatialViewController : ScreenViewController<StepsScreenSpatialViewController, StepsScreenSpatialView>
    {
        public override ScreenName ScreenName => ScreenName.StepsScreen;

        private ActivityStep _step;
        private List<Content> _contents;
        private readonly List<ContetItemView> _contentsItemViews = new();
        private readonly List<StepsMediaListItemView> _mediaListItemViews = new();
        private readonly List<StepsToolsListItemView> _toolsListItemViews = new();

        protected override void OnBind()
        {
            base.OnBind();
            View.SetActionOnButtonBackClick(OnButtonBackClicked);
            View.SetActionOnButtonAddAugmentationClick(OnButtonAddAugmentationClicked);
            View.SetActionOnToggleEditorValueChanged(OnToggleEditorValueChanged);
            View.SetActionOnButtonMediaAddNewFileClick(OnButtonMediaAddNewFileClick);
            View.SetActionOnButtonToolsAddNewToolClick(OnButtonToolsAddNewToolClick);
            View.SetActionOnButtonNextStepClick(OnButtonNextStepClicked);
            View.SetActionOnButtonPreviousStepClick(OnButtonPreviousStepClicked);
            View.SetActionOnTitleInputEndEdit(OnTitleInputEndEdit);
            View.SetActionOnDescriptionInputEndEdit(OnDescriptionInputEndEdit);

            RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged += OnEditorModeChanged;
            RootObject.Instance.LEE.ActivityManager.OnActivityUpdated += ActivityManagerOnActivityUpdated;
            RootObject.Instance.LEE.StepManager.OnStepChanged += StepManagerOnStepChanged;
            RootObject.Instance.LEE.ContentManager.OnContentActivated += ContentManagerOnContentActivated;
        }

        private void OnTitleInputEndEdit(string text)
        {
            RootObject.Instance.LEE.StepManager.SetStepName(_step.Id, text);
        }

        private void OnDescriptionInputEndEdit(string text)
        {
            RootObject.Instance.LEE.StepManager.SetStepDescription(_step.Id, text);
        }

        private void ActivityManagerOnActivityUpdated(Activity activity)
        {
            if (_step != null)
            {
                UpdateInfoViewAsync().Forget();
            }
        }

        private void OnButtonMediaAddNewFileClick()
        {
            var prefab = MenuManager.Instance.GetImageSelectPopupViewPrefab();
            PopupsViewer.Instance.Show(prefab, (Action<Texture2D>)OnMediaFileSelected);
        }

        private void OnButtonToolsAddNewToolClick()
        {
            
        }
        
        private void OnButtonPreviousStepClicked()
        {
            RootObject.Instance.LEE.StepManager.GoToNextStep();
        }

        private void OnButtonNextStepClicked()
        {
            RootObject.Instance.LEE.StepManager.GoToPreviousStep();
        }

        private void OnMediaFileSelected(Texture2D texture)
        {
            SaveMediaFileAsync(texture).Forget();
        }

        private async UniTask SaveMediaFileAsync(Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }

            var file = new FileModel
            {
                Id = Guid.NewGuid(),
                CreationDate = DateTime.UtcNow,
                FileHash = string.Empty
            };

            var activityId = RootObject.Instance.LEE.ActivityManager.ActivityId;
            await RootObject.Instance.LEE.MediaManager.SaveToMediaFileAsync(texture, activityId, file.Id);
            RootObject.Instance.LEE.StepManager.AddAttachment(_step.Id, file);
            RootObject.Instance.LEE.AssetsManager.UploadMediaFileAsync(activityId, file.Id).Forget();
        }

        private void OnToggleEditorValueChanged(bool value)
        {
            RootObject.Instance.LEE.ActivityManager.IsEditorMode = !value;
        }

        private void OnEditorModeChanged(bool value)
        {
            View.SetPanelAddNewStepActive(value);
            View.RemoveActionOnToggleEditorValueChanged(OnToggleEditorValueChanged);
            View.SetIsToggleEditorOn(!value);
            View.SetActionOnToggleEditorValueChanged(OnToggleEditorValueChanged);
            View.SetTitleInputInteractable(value);
            View.SetDescriptionInputInteractable(value);

            foreach (var itemView in _contentsItemViews)
            {
                itemView.Interactable = value;
            }

            foreach (var itemView in _mediaListItemViews)
            {
                itemView.Interactable = value;
            }
            View.SetActiveContainerMediaAddNewFile(value);

            foreach (var itemView in _toolsListItemViews)
            {
                itemView.Interactable = value;
            }
            View.SetActiveContainerToolsAddNewToolClick(value);
        }

        private void ContentManagerOnContentActivated(List<Content> contents)
        {
            _contents = contents;
            UpdateAugmentationsView();
        }

        private void StepManagerOnStepChanged(ActivityStep step)
        {
            _step = step;
            UpdateInfoViewAsync().Forget();
        }

        private async UniTask UpdateInfoViewAsync()
        {
            View.SetTitleInputText(_step.Name);
            View.SetDescriptionInputText(_step.Description);
            await UpdateInfoMediaViewAsync();
            UpdateInfoToolsView();
        }

        private async UniTask UpdateInfoMediaViewAsync()
        {
            var isEditorMode = RootObject.Instance.LEE.ActivityManager.IsEditorMode;
            var container = View.GetContainerMedia();
            var prefab = View.GetStepsMediaListItemViewPrefab();

            foreach (var item in _mediaListItemViews)
            {
                Destroy(item.gameObject);
            }
            _mediaListItemViews.Clear();

            if (_step?.Attachment != null)
            {
                foreach (var file in _step.Attachment)
                {
                    var activityId = RootObject.Instance.LEE.ActivityManager.ActivityId;
                    var texture = await RootObject.Instance.LEE.MediaManager.LoadMediaFileToTexture2D(activityId, file.Id);

                    if (texture != null)
                    {
                        var item = Instantiate(prefab, container);
                        item.Initialize(file, texture, _step.Id, (stepId, fileModel) =>
                        {
                            RootObject.Instance.LEE.StepManager.RemoveAttachment(stepId, fileModel.Id);
                        });
                        item.Interactable = isEditorMode;
                        _mediaListItemViews.Add(item);
                    }
                }
            }
        }

        private void UpdateInfoToolsView()
        {
            var isEditorMode = RootObject.Instance.LEE.ActivityManager.IsEditorMode;
            var container = View.GetContainerTools();
            var prefab = View.GetStepsToolsListItemViewPrefab();

            foreach (var item in _toolsListItemViews)
            {
                Destroy(item.gameObject);
            }
            _toolsListItemViews.Clear();

            if (_step?.RequiredToolsPartsMaterials != null)
            {
                foreach (var tool in _step.RequiredToolsPartsMaterials)
                {
                    var item = Instantiate(prefab, container);
                    item.Initialize(tool, _step.Id, (stepId, requiredToolsPartsMaterials) =>
                    {
                        RootObject.Instance.LEE.StepManager.RemoveToolPartMaterial(stepId, requiredToolsPartsMaterials.Id);
                    });
                    item.Interactable = isEditorMode;
                    _toolsListItemViews.Add(item);
                }
            }
        }

        private void UpdateAugmentationsView()
        {
            var container = View.GetStepContainer();
            var prefab = View.GetContetItemViewPrefab();

            foreach (var item in _contentsItemViews)
            {
                Destroy(item.gameObject);
            }
            _contentsItemViews.Clear();

            foreach (var content in _contents)
            {
                var item = Instantiate(prefab, container);
                item.Initialize(content, OnStepItemClick, OnStepItemDeleteClick);
                _contentsItemViews.Add(item);
            }
        }

        private void OnStepItemDeleteClick(Content content)
        {
            RootObject.Instance.LEE.ContentManager.RemoveContent(content.Id);
        }

        private void OnStepItemClick(Content content)
        {
            var prefab = MenuManager.Instance.GetEditorPrefab(content.Type);
            PopupsViewer.Instance.Show(prefab, content);
        }

        private void OnButtonAddAugmentationClicked()
        {
            MenuManager.Instance.ShowSelectAugmentationScreenSpatialView();
        }

        private void OnButtonBackClicked()
        {
            MenuManager.Instance.ShowScreen(ScreenName.NewActivityScreen);
        }
    }
}
