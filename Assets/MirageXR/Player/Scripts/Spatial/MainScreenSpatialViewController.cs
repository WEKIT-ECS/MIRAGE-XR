using System.Collections.Generic;
using LearningExperienceEngine.DataModel;

namespace MirageXR
{
    public class MainScreenSpatialViewController : ScreenViewController<MainScreenSpatialViewController, MainScreenSpatialView>
    {
        public override ScreenName ScreenName => ScreenName.MainScreen;

        protected override void OnBind()
        {
            base.OnBind();
            View.SetActionOnButtonSortingClick(() => {MenuManager.Instance.ShowSortingView(); });
            View.SetActionOnButtonAddNewActivityClick(OnAddNewActivityClick);
            
            RootObject.Instance.ActivityManager.OnActivitiesFetched += OnActivitiesFetched;
            RootObject.Instance.ActivityManager.OnActivityLoaded += OnActivityLoaded;
        }

        private void OnActivityLoaded(Activity activity)
        {
            MenuManager.Instance.ShowScreen(ScreenName.NewActivityScreen);
        }

        private void OnActivitiesFetched(List<Activity> activities)
        {
            var container = View.GetActivityContainer();
            var prefab = View.GetActivityListItemPrefab();
            foreach (var activity in activities)
            {
                var item = Instantiate(prefab, container);
                item.Initialize(activity, OnActivityListItemClicked);
            }
        }

        private void OnAddNewActivityClick()
        {
            var baseCamera = RootObject.Instance.BaseCamera;
            RootObject.Instance.ActivityManager.CreateNewActivity((baseCamera.transform.forward * 0.5f) + baseCamera.transform.position);
        }

        private void OnActivityListItemClicked(Activity activity)
        {
            RootObject.Instance.ActivityManager.LoadActivityAsync(activity.Id);
        }
    }
}
