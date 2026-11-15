using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using LearningExperienceEngine.DTOs;
using TMPro;
using UnityEngine;
using Activity = LearningExperienceEngine.DataModel.Activity;

namespace MirageXR
{
    public class MainScreenSpatialViewController : ScreenViewController<MainScreenSpatialViewController, MainScreenSpatialView>
    {
        public override ScreenName ScreenName => ScreenName.MainScreen;

        protected override void OnBind()
        {
            base.OnBind();
            View.SetActionOnButtonSortingClick(OnButtonSortingClick);
            View.SetActionOnButtonAddNewActivityClick(OnAddNewActivityClick);
            View.SetActionOnButtonCollaborativeSessionClick(OnCollaborativeSessionClick);
            View.SetActionOnButtonQRCodeClick(OnButtonQRCodeClick);
            View.SetActionOnToggleEditorValueChanged(OnToggleEditorValueChanged);
            View.SetActionOnButtonLoginClick(OnLoginButtonClick);
            View.SetActionOnButtonRegisterClick(OnRegisterButtonClick);
            View.SetActionOnButtonBackClick(OnBackButtonClick);
            View.SetBlurredBackgroundActive(false);
            View.SetMainScreenActive(false);
            View.SetSignInScreenActive(true);
            View.SetBackButtonActive(false);

            RootObject.Instance.LEE.ActivityManager.OnActivitiesFetched += OnActivitiesFetched;
            RootObject.Instance.LEE.ActivityManager.OnActivityLoaded += OnActivityLoaded;
            RootObject.Instance.LEE.ActivityManager.OnActivityClosed += OnActivityClosed;
            RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged += OnEditorModeChanged;
            RootObject.Instance.LEE.AuthorizationManager.OnLoginCompleted += OnLoginCompleted;
            RootObject.Instance.LEE.AuthorizationManager.OnPrivilegesChanged += UpdateAddNewActivityButtonState;
            UpdateAddNewActivityButtonState();
        }

        protected override void OnUnbind()
        {
            base.OnUnbind();
            if (RootObject.Instance?.LEE != null)
            {
                if (RootObject.Instance.LEE.ActivityManager != null)
                {
                    RootObject.Instance.LEE.ActivityManager.OnActivitiesFetched -= OnActivitiesFetched;
                    RootObject.Instance.LEE.ActivityManager.OnActivityLoaded -= OnActivityLoaded;
                    RootObject.Instance.LEE.ActivityManager.OnActivityClosed -= OnActivityClosed;
                    RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged -= OnEditorModeChanged;
                }
                if (RootObject.Instance.LEE.AuthorizationManager != null)
                {
                    RootObject.Instance.LEE.AuthorizationManager.OnLoginCompleted -= OnLoginCompleted;
                    RootObject.Instance.LEE.AuthorizationManager.OnPrivilegesChanged -= UpdateAddNewActivityButtonState;
                }
            }
        }

        private void OnBackButtonClick()
        {
            MenuManager.Instance.ShowScreen(ScreenName.NewActivityScreen );
        }

        private void OnLoginCompleted(string token)
        {
            RootObject.Instance.LEE.ActivityManager.FetchActivitiesAsync();
            View.SetBlurredBackgroundActive(true);
            View.SetMainScreenActive(true);
            View.SetSignInScreenActive(false);
        }

        protected override void OnViewActivated()
        {
            base.OnViewActivated();
            FetchActivitiesAsync().Forget();
        }

        private async UniTask FetchActivitiesAsync()
        {
            await RootObject.Instance.LEE.ActivityManager.FetchActivitiesAsync();
            View.ActivityListScrollToTopSmooth();
        } 

        private void OnRegisterButtonClick()
        {
            RootObject.Instance.LEE.AuthorizationManager.Login();
        }

        private void OnLoginButtonClick()
        {
            RootObject.Instance.LEE.AuthorizationManager.Login();
        }

        private void OnButtonSortingClick()
        {
            MenuManager.Instance.ShowSortingView();
        }

        private void OnButtonQRCodeClick()
        {
            MenuManager.Instance.ToggleQRCodeScannerView();
        }

        private void OnCollaborativeSessionClick()
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

        private void OnToggleEditorValueChanged(bool value)
        {
            RootObject.Instance.LEE.ActivityManager.IsEditorMode = !value;
        }

        private void OnEditorModeChanged(bool value)
        {
            View.RemoveActionOnToggleEditorValueChanged(OnToggleEditorValueChanged);
            View.SetIsToggleEditorOn(!value);
            View.SetActionOnToggleEditorValueChanged(OnToggleEditorValueChanged);
        }

        private void OnActivityLoaded(Activity activity)
        {
            MenuManager.Instance.ShowScreen(ScreenName.NewActivityScreen);
            View.SetBackButtonActive(true);
        }

        private void OnActivityClosed()
        {
            View.SetBackButtonActive(false);
            FetchActivitiesAsync().Forget();
        }

        private string _activeTagFilter;
        private ActivityResponse _lastResponse;

        private void UpdateAddNewActivityButtonState()
        {
            View.SetAddNewActivityButtonActive(RootObject.Instance.LEE.AuthorizationManager.CanCreateActivity);
        }

        private void OnActivitiesFetched(ActivityResponse response)
        {
            _lastResponse = response;
            UpdateAddNewActivityButtonState();

            var container = View.GetActivityContainer();
            var prefab = View.GetActivityListItemPrefab();
            var selectedActivityId = RootObject.Instance.LEE.ActivityManager.ActivityId;
            
            foreach (Transform child in container.transform)
            {
                Destroy(child.gameObject);
            }

            IEnumerable<LearningExperienceEngine.DTOs.Activity> activities = response.Activities;
            if (!string.IsNullOrEmpty(_activeTagFilter))
            {
                activities = activities.Where(a => a.Tags != null && a.Tags.Contains(_activeTagFilter));
            }

            if (LearningExperienceEngine.UserSettings.groupByTags)
            {
                var groups = activities.GroupBy(a => a.Tags != null && a.Tags.Count > 0 ? a.Tags[0] : "Untagged");
                ActivitySpatialListItem selectedItem = null;

                foreach (var group in groups)
                {
                    CreateGroupHeader(container, group.Key);

                    foreach (var activity in group)
                    {
                        var isSelected = activity.Id == selectedActivityId;
                        var item = Instantiate(prefab, container);
                        item.Initialize(activity, OnActivityListItemClicked, OnActivityListItemDeleteClicked, isSelected, OnTagClicked);
                        if (isSelected)
                        {
                            selectedItem = item;
                        }
                    }
                }

                selectedItem?.transform.SetAsFirstSibling();
            }
            else
            {
                ActivitySpatialListItem selectedItem = null;
                foreach (var activity in activities)
                {
                    var isSelected = activity.Id == selectedActivityId;
                    var item = Instantiate(prefab, container);
                    item.Initialize(activity, OnActivityListItemClicked, OnActivityListItemDeleteClicked, isSelected, OnTagClicked);
                    if (isSelected)
                    {
                        selectedItem = item;
                    }
                }

                selectedItem?.transform.SetAsFirstSibling();
            }
        }

        private void CreateGroupHeader(Transform container, string title)
        {
            var headerGo = new GameObject($"GroupHeader_{title}", typeof(RectTransform));
            headerGo.transform.SetParent(container, false);
            var rt = headerGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 36);

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(headerGo.transform, false);
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = new Vector2(0, 0);
            textRt.anchorMax = new Vector2(1, 1);
            textRt.offsetMin = new Vector2(15, 0);
            textRt.offsetMax = new Vector2(-15, 0);

            var tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = title;
            tmp.fontSize = 17;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(0.85f, 0.85f, 0.85f, 1f);
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.raycastTarget = false;
        }

        private void OnTagClicked(string tag)
        {
            if (_activeTagFilter == tag)
            {
                _activeTagFilter = null; // Toggle off
            }
            else
            {
                _activeTagFilter = tag;
            }

            if (_lastResponse != null)
            {
                OnActivitiesFetched(_lastResponse);
            }
        }

        private void OnAddNewActivityClick()
        {
            var baseCamera = RootObject.Instance.BaseCamera;
            var worldPos = (baseCamera.transform.forward * 0.5f) + baseCamera.transform.position;   //TODO: move to Manager
            var anchor = RootObject.Instance.CalibrationManager?.Anchor;
            var position = anchor != null ? anchor.InverseTransformPoint(worldPos) : worldPos;
            RootObject.Instance.LEE.ActivityManager.CreateNewActivity(position);
        }

        private void OnActivityListItemClicked(LearningExperienceEngine.DTOs.Activity activity)
        {
            var selectedActivityId = RootObject.Instance.LEE.ActivityManager.ActivityId;
            if (selectedActivityId == activity.Id)
            {
                MenuManager.Instance.ShowScreen(ScreenName.NewActivityScreen);
            }
            else
            {
                LoadActivityAsync(activity).Forget();
            }
        }

        private async UniTask LoadActivityAsync(LearningExperienceEngine.DTOs.Activity activity)
        {
            try
            {
                await RootObject.Instance.LEE.ActivityManager.LoadActivityAsync(activity.Id);
            }
            catch (Exception ex)
            {
                AppLog.LogError($"Failed to load activity '{activity.Name}': {ex.Message}");
                if (DialogWindow.Instance != null)
                {
                    DialogWindow.Instance.Show("Error", $"Activity '{activity.Name}' could not be found or loaded.", new DialogButtonContent("OK"));
                }
                else if (MenuManager.Instance?.Dialog != null)
                {
                    MenuManager.Instance.Dialog.ShowMiddle("Error", $"Activity '{activity.Name}' could not be found or loaded.", "OK", () => { });
                }
            }
        }

        private void OnActivityListItemDeleteClicked(LearningExperienceEngine.DTOs.Activity activity)
        {
            DeleteActivityAsync(activity).Forget();
        }

        private async UniTask DeleteActivityAsync(LearningExperienceEngine.DTOs.Activity activity)
        {
            try
            {
                await RootObject.Instance.LEE.ActivityManager.DeleteActivityAsync(activity.Id);
                await RootObject.Instance.LEE.ActivityManager.FetchActivitiesAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete activity '{activity.Name}': {ex.Message}");
                if (DialogWindow.Instance != null)
                {
                    DialogWindow.Instance.Show("Delete Failed", $"Failed to delete activity: {ex.Message}", new DialogButtonContent("OK"));
                }
            }
        }
    }
}
