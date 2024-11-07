using System.Collections.Generic;
using System.Numerics;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using LearningExperienceEngine.DTOs;
using UnityEngine;

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
            
            RootObject.Instance.LEE.ActivityManager.OnActivitiesFetched += OnActivitiesFetched;
            RootObject.Instance.LEE.ActivityManager.OnActivityLoaded += OnActivityLoaded;
        }

        private void OnActivityLoaded(Activity activity)
        {
            MenuManager.Instance.ShowScreen(ScreenName.NewActivityScreen);
        }

        private void OnActivitiesFetched(List<Activity> activities)
        {
            var container = View.GetActivityContainer();
            var prefab = View.GetActivityListItemPrefab();
            
            foreach (Transform child in container.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (var activity in activities)
            {
                var item = Instantiate(prefab, container);
                item.Initialize(activity, OnActivityListItemClicked, OnActivityListItemDeleteClicked);
            }
        }

        private void OnAddNewActivityClick()
        {
#if UNITY_VISIONOS || VISION_OS
            var baseCamera = RootObject.Instance.VolumeCamera;
            RootObject.Instance.LEE.ActivityManager.CreateNewActivity((baseCamera.transform.forward * 0.5f) + baseCamera.transform.position + new UnityEngine.Vector3(0, 1.2f, 0));
#else
            var baseCamera = RootObject.Instance.BaseCamera;
            RootObject.Instance.LEE.ActivityManager.CreateNewActivity((baseCamera.transform.forward * 0.5f) + baseCamera.transform.position);
#endif
        }

        private void OnActivityListItemClicked(Activity activity)
        {
            RootObject.Instance.LEE.ActivityManager.LoadActivityAsync(activity.Id).Forget();
        }

        private void OnActivityListItemDeleteClicked(Activity activity)
        {
            DeleteActivityAsync(activity).Forget();
        }

        private async UniTask DeleteActivityAsync(Activity activity)
        {
            await RootObject.Instance.LEE.ActivityManager.DeleteActivityAsync(activity.Id);
            await RootObject.Instance.LEE.ActivityManager.FetchActivitiesAsync();
        }
    }
}
