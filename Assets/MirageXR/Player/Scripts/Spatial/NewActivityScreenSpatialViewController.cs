using System;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using LearningExperienceEngine.NewDataModel;
using UnityEngine;
using Activity = LearningExperienceEngine.DataModel.Activity;

namespace MirageXR  //TODO: add Spatial namespace
{
    public class NewActivityScreenSpatialViewController : ScreenViewController<NewActivityScreenSpatialViewController, NewActivityScreenSpatialView>
    {   //TODO: rename to ActivityScreenViewController
        private Activity _activity;
        private Texture2D _texture;
        private System.Threading.CancellationTokenSource _updateInfoCts;

        public override ScreenName ScreenName => ScreenName.NewActivityScreen;

        private static RoomTwinManager roomTwinManager => RootObject.Instance.RoomTwinManager;

        protected override void OnBind()
        {
            base.OnBind();
            View.SetActionOnButtonBackClick(OnButtonBackClicked);
            View.SetActionOnButtonCloseClick(OnButtonCloseClicked);
            View.SetActionOnButtonSettingsClick(OnButtonSettingsClicked);
            View.SetActionOnButtonCollaborativeSessionClick(OnButtonCollaborativeSessionClicked);
            View.SetActionOnButtonAddNewStepClick(OnButtonAddNewStepClicked);
            View.SetActionOnButtonNextStepClick(OnButtonNextStepClicked);
            View.SetActionOnButtonPreviousStepClick(OnButtonPreviousStepClicked);
            View.SetActionOnToggleEditorValueChanged(OnToggleEditorValueChanged);
            View.SetActionOnButtonWireframeVignetteClick(OnButtonWireframeVignetteClicked);
            View.SetActionOnButtonAssignRoomModelClick(OnButtonAssignRoomModelClicked);
            View.SetActionOnButtonRepositionClick(OnButtonRepositionClicked);
            View.SetActionOnToggleShowRoomScanValueChanged(OnToggleShowRoomScanValueChanged);
            View.SetActionOnButtonThumbnailClick(OnButtonThumbnailClicked);
            View.SetActionOnButtonDeleteThumbnailClick(OnButtonDeleteThumbnailClicked);

            View.SetActionOnInputFieldActivityNameEditEnd(OnInputFieldActivityNameEditEnd);
            View.SetActionOnInputFieldActivityDescriptionEditEnd(OnInputFieldActivityDescriptionEditEnd);

            RootObject.Instance.LEE.ActivityManager.OnActivityLoaded += OnActivityUpdated;
            RootObject.Instance.LEE.ActivityManager.OnActivityUpdated += OnActivityUpdated;
            RootObject.Instance.LEE.ActivityManager.OnActivityClosed += OnActivityClosed;
            RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged += OnEditorModeChanged;
            RootObject.Instance.LEE.StepManager.OnStepChanged += StepManagerOnStepChanged;
        }

        protected override void OnUnbind()
        {
            base.OnUnbind();
            if (RootObject.Instance?.LEE != null)
            {
                if (RootObject.Instance.LEE.ActivityManager != null)
                {
                    RootObject.Instance.LEE.ActivityManager.OnActivityLoaded -= OnActivityUpdated;
                    RootObject.Instance.LEE.ActivityManager.OnActivityUpdated -= OnActivityUpdated;
                    RootObject.Instance.LEE.ActivityManager.OnActivityClosed -= OnActivityClosed;
                    RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged -= OnEditorModeChanged;
                }
                if (RootObject.Instance.LEE.StepManager != null)
                {
                    RootObject.Instance.LEE.StepManager.OnStepChanged -= StepManagerOnStepChanged;
                }
            }

            _updateInfoCts?.Cancel();
            _updateInfoCts?.Dispose();
            _updateInfoCts = null;
        }

        private void OnInputFieldActivityNameEditEnd(string text)
        {
            RootObject.Instance.LEE.ActivityManager.SetActivityName(text);
        }

        private void OnInputFieldActivityDescriptionEditEnd(string text)
        {
            RootObject.Instance.LEE.ActivityManager.SetActivityDescription(text);
        }

        private void OnButtonThumbnailClicked()
        {
            var prefab = MenuManager.Instance.GetImageSelectPopupViewPrefab();
            PopupsViewer.Instance.Show(prefab, (Action<Texture2D>)OnThumbnailSelected, _texture);
        }

        private void OnThumbnailSelected(Texture2D texture)
        {
            SaveThumbnailAsync(texture).Forget();
        }

        private async UniTask SaveThumbnailAsync(Texture2D texture)
        {
            if (texture == null || _activity == null)
            {
                return;
            }

            SetThumbnailView(texture);
            View.SetButtonDeleteThumbnailActive(true);

            try
            {
                var activityId = _activity.Id;

                // Delete existing server and local thumbnail if one exists
                if (_activity.Thumbnail != null && _activity.Thumbnail.Id != Guid.Empty)
                {
                    await RootObject.Instance.LEE.MediaManager.DeleteMediaFileAsync(activityId, _activity.Thumbnail.Id);
                }

                var fileId = Guid.NewGuid();
                await RootObject.Instance.LEE.MediaManager.SaveToMediaFileAsync(texture, activityId, fileId);

                var file = new FileModel
                {
                    Id = fileId,
                    CreationDate = DateTime.UtcNow,
                    FileHash = string.Empty
                };
                await RootObject.Instance.LEE.ActivityManager.SetThumbnailAsync(file);
                await RootObject.Instance.LEE.AssetsManager.UploadMediaFileAsync(activityId, fileId);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save thumbnail: {ex}");
            }
        }

        private void OnButtonDeleteThumbnailClicked()
        {
            DeleteThumbnailAsync().Forget();
        }

        private async UniTask DeleteThumbnailAsync()
        {
            if (_activity == null)
            {
                return;
            }

            try
            {
                var activityId = _activity.Id;

                // Delete server asset and local file
                if (_activity.Thumbnail != null && _activity.Thumbnail.Id != Guid.Empty)
                {
                    await RootObject.Instance.LEE.MediaManager.DeleteMediaFileAsync(activityId, _activity.Thumbnail.Id);
                }

                // Clear thumbnail on server activity model
                await RootObject.Instance.LEE.ActivityManager.SetThumbnailAsync(null);

                // Reset UI
                View.SetImageThumbnailActive(false);
                View.SetButtonDeleteThumbnailActive(false);
                if (_texture != null)
                {
                    Destroy(_texture);
                    _texture = null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete thumbnail: {ex}");
            }
        }

        private void OnToggleEditorValueChanged(bool value)
        {
            RootObject.Instance.LEE.ActivityManager.IsEditorMode = !value;
        }

        private void OnEditorModeChanged(bool value)
        {
            View.SetInputFieldActivityNameTextInteractable(value);
            View.SetInputFieldActivityDescriptionInteractable(value);
            View.SetPanelAddNewStepActive(value);
            View.RemoveActionOnToggleEditorValueChanged(OnToggleEditorValueChanged);
            View.SetIsToggleEditorOn(!value);
            View.SetActionOnToggleEditorValueChanged(OnToggleEditorValueChanged);
        }

        private void OnButtonPreviousStepClicked()
        {
            RootObject.Instance.LEE.StepManager.GoToNextStep();
        }

        private void OnButtonNextStepClicked()
        {
            RootObject.Instance.LEE.StepManager.GoToPreviousStep();
        }

        private void OnToggleShowRoomScanValueChanged(bool value)
        {
            roomTwinManager.SetRoomTwinVisibility(value);
        }

        private void OnButtonRepositionClicked()
        {
            // TODO
        }

        private void OnButtonWireframeVignetteClicked()
        {
            MenuManager.Instance.ShowRoomScanSettingsPanelView();
        }

        private void OnButtonAssignRoomModelClicked()
        {
            //var url = @"https://www.google.com";  // TODO: use correct url
            //roomTwinManager.LoadRoomTwinModelAsync(url).Forget();

            MenuManager.Instance.ShowRoomScanGalleryView();
        }

        private void OnActivityUpdated(Activity activity)
        {
            _activity = activity;
            UpdateView();
        }

        private void UpdateView()
        {
            View.SetTitleText(_activity?.Name);
            UpdateStepsView();

            _updateInfoCts?.Cancel();
            _updateInfoCts?.Dispose();
            _updateInfoCts = new System.Threading.CancellationTokenSource();
            UpdateInfoViewAsync(_updateInfoCts.Token).Forget();
        }

        private void UpdateStepsView()
        {
            var container = View.GetStepsContainer();
            var prefab = View.GetStepsItemPrefab();

            foreach (Transform child in container.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (var activityStep in _activity.Steps)
            {
                var obj = Instantiate(prefab, container);
                obj.Initialize(activityStep, OnStepItemClicked, OnStepItemMenuClicked);
                obj.OnStepSelected(activityStep == RootObject.Instance.LEE.StepManager.CurrentStep);
            }
        }
        
        private void StepManagerOnStepChanged(ActivityStep step)
        {
            if (_activity == null)
            {
                return;
            }
            UpdateView();
        }

        private async UniTask UpdateInfoViewAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            if (_activity == null)
            {
                return;
            }

            View.SetInputFieldActivityNameText(_activity.Name);
            View.SetInputFieldActivityDescriptionText(_activity.Description);

            if (_texture is not null)
            {
                Destroy(_texture);
                _texture = null;
            }

            if (_activity.Thumbnail != null && _activity.Thumbnail.Id != Guid.Empty)
            {
                var fileId = _activity.Thumbnail.Id;
                _texture = await RootObject.Instance.LEE.MediaManager.LoadMediaFileToTexture2D(_activity.Id, fileId);
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (_texture == null)
                {
                    await RootObject.Instance.LEE.MediaManager.DownloadMediaFileAsync(_activity.Id, fileId);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                    _texture = await RootObject.Instance.LEE.MediaManager.LoadMediaFileToTexture2D(_activity.Id, fileId);
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (_texture is not null)
                {
                    SetThumbnailView(_texture);
                    View.SetButtonDeleteThumbnailActive(true);
                    return;
                }
            }

            View.SetImageThumbnailActive(false);
            View.SetButtonDeleteThumbnailActive(false);
        }

        private void SetThumbnailView(Texture2D texture2D)
        {
            var sprite = Utilities.TextureToSprite(texture2D);  //TODO: temp
            View.SetImageThumbnailActive(true);
            View.SetImageThumbnailTexture(sprite);
            var containerSize = View.GetImageThumbnailContainerSize();
            var size = LearningExperienceEngine.Utilities.FitRectToRect(containerSize, new Vector2(texture2D.width, texture2D.height));
            View.SetImageThumbnailSize(size);
        }

        private void OnStepItemClicked(ActivityStep step)
        {
            RootObject.Instance.LEE.StepManager.GoToStep(step.Id);
            MenuManager.Instance.ShowScreen(ScreenName.StepsScreen);
        }

        private void OnStepItemMenuClicked(ActivityStep step)
        {
            RootObject.Instance.LEE.StepManager.RemoveStep(step.Id);
        }

        private void OnButtonBackClicked()
        {
            MenuManager.Instance.ShowScreen(ScreenName.MainScreen);
        }

        private void OnButtonCloseClicked()
        {
            LearningExperienceEngine.EventManager.ClearAll();
            roomTwinManager.SetRoomTwinVisibility(false);
            RootObject.Instance.LEE.ActivityManager.CloseActivity();
            MenuManager.Instance.ShowScreen(ScreenName.MainScreen);
        }

        private void OnActivityClosed()
        {
            _activity = null;
            if (_texture != null)
            {
                Destroy(_texture);
                _texture = null;
            }

            var container = View.GetStepsContainer();
            if (container != null)
            {
                foreach (Transform child in container.transform)
                {
                    Destroy(child.gameObject);
                }
            }

            View.SetTitleText(string.Empty);
            View.SetInputFieldActivityNameText(string.Empty);
            View.SetInputFieldActivityDescriptionText(string.Empty);
            View.SetImageThumbnailActive(false);
            View.SetButtonDeleteThumbnailActive(false);

            _updateInfoCts?.Cancel();
            _updateInfoCts?.Dispose();
            _updateInfoCts = null;
        }

        private void OnButtonAddNewStepClicked()
        {
            var baseCamera = RootObject.Instance.BaseCamera;
            var position = (baseCamera.transform.forward * 0.5f) + baseCamera.transform.position;
            var stepManager = RootObject.Instance.LEE.StepManager;
            var step = stepManager.AddStep(new Location { Position = position, Rotation = Vector3.zero, Scale = Vector3.one });
            stepManager.GoToStep(step.Id);
            
        }

        private void OnButtonCollaborativeSessionClicked()
        {
#if FUSION2
            if (RootObject.Instance.CollaborationManager.IsConnectedToServer)
            {
                MenuManager.Instance.ShowCollaborativeSessionSettingsPanelView();
            }
            else
            {
				MenuManager.Instance.ShowCollaborativeSessionPanelView();
			}
#else
			MenuManager.Instance.ShowCollaborativeSessionPanelView();
#endif
		}

        private void OnButtonSettingsClicked()
        {
        }
    }
}
