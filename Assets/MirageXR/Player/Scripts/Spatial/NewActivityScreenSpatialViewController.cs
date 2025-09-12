using System;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using UnityEngine;
using Activity = LearningExperienceEngine.DataModel.Activity;

namespace MirageXR  //TODO: add Spatial namespace
{
    public class NewActivityScreenSpatialViewController : ScreenViewController<NewActivityScreenSpatialViewController, NewActivityScreenSpatialView>
    {   //TODO: rename to ActivityScreenViewController
        private Activity _activity;
        private Texture2D _texture;

        public override ScreenName ScreenName => ScreenName.NewActivityScreen;

        private static RoomTwinManager roomTwinManager => RootObject.Instance.RoomTwinManager;

        protected override void OnBind()
        {
            base.OnBind();
            View.SetActionOnButtonBackClick(OnButtonBackClicked);
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

            View.SetActionOnInputFieldActivityNameEditEnd(OnInputFieldActivityNameEditEnd);
            View.SetActionOnInputFieldActivityDescriptionEditEnd(OnInputFieldActivityDescriptionEditEnd);

            RootObject.Instance.LEE.ActivityManager.OnActivityLoaded += OnActivityUpdated;
            RootObject.Instance.LEE.ActivityManager.OnActivityUpdated += OnActivityUpdated;
            RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged += OnEditorModeChanged;
            RootObject.Instance.LEE.StepManager.OnStepChanged += StepManagerOnStepChanged;
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
            await RootObject.Instance.LEE.MediaManager.SaveToMediaFileAsync(texture, _activity.Id, file.Id);
            await RootObject.Instance.LEE.ActivityManager.SetThumbnailAsync(file);
            RootObject.Instance.LEE.AssetsManager.UploadMediaFileAsync(_activity.Id, file.Id).Forget();
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
            roomTwinManager.DisplayRoomTwinAsync(value).Forget();
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
            var url = @"https://www.google.com";  // TODO: use correct url
            roomTwinManager.LoadRoomTwinModelAsync(url).Forget();
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
            UpdateInfoViewAsync().Forget();
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

        private async UniTask UpdateInfoViewAsync()
        {
            View.SetInputFieldActivityNameText(_activity.Name);
            View.SetInputFieldActivityDescriptionText(_activity.Description);

            if (_texture is not null)
            {
                Destroy(_texture);
            }

            if (_activity.Thumbnail != null)
            {
                _texture = await RootObject.Instance.LEE.MediaManager.LoadMediaFileToTexture2D(_activity.Id, _activity.Thumbnail.Id);
                if (_texture is not null)
                {
                    SetThumbnailView(_texture);
                    return;
                }
            }

            View.SetImageThumbnailActive(false);
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
