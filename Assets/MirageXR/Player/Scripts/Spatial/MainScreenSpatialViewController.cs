using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DTOs;
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
            RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged += OnEditorModeChanged;
            RootObject.Instance.LEE.AuthorizationManager.OnLoginCompleted += OnLoginCompleted;
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
            // TODO
        }

        private void OnLoginButtonClick()
        {
            RootObject.Instance.LEE.AuthorizationManager.Login();
        }

        private void OnButtonSortingClick()
        {
            MenuManager.Instance.ShowSortingView();
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

        private void OnActivitiesFetched(ActivityResponse response)
        {
            var container = View.GetActivityContainer();
            var prefab = View.GetActivityListItemPrefab();
            var selectedActivityId = RootObject.Instance.LEE.ActivityManager.ActivityId;
            
            foreach (Transform child in container.transform)
            {
                Destroy(child.gameObject);
            }

            ActivitySpatialListItem selectedItem = null;
            foreach (var activity in response.Activities)
            {
                var isSelected = activity.Id == selectedActivityId;
                var item = Instantiate(prefab, container);
                item.Initialize(activity, OnActivityListItemClicked, OnActivityListItemDeleteClicked, isSelected);
                if (isSelected)
                {
                    selectedItem = item;
                }
            }

            selectedItem?.transform.SetAsFirstSibling();
        }

        private void OnAddNewActivityClick()
        {
            var baseCamera = RootObject.Instance.BaseCamera;
            var position = (baseCamera.transform.forward * 0.5f) + baseCamera.transform.position;   //TODO: move to Manager
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
                RootObject.Instance.LEE.ActivityManager.LoadActivityAsync(activity.Id).Forget();
            }
        }

        private void OnActivityListItemDeleteClicked(LearningExperienceEngine.DTOs.Activity activity)
        {
            DeleteActivityAsync(activity).Forget();
        }

        private async UniTask DeleteActivityAsync(LearningExperienceEngine.DTOs.Activity activity)
        {
            await RootObject.Instance.LEE.ActivityManager.DeleteActivityAsync(activity.Id);
            await RootObject.Instance.LEE.ActivityManager.FetchActivitiesAsync();
        }
    }
}
