using LearningExperienceEngine.DataModel;
using UnityEngine;
using Activity = LearningExperienceEngine.DataModel.Activity;

namespace MirageXR
{
    public class NewActivityScreenSpatialViewController : ScreenViewController<NewActivityScreenSpatialViewController, NewActivityScreenSpatialView>
    {
        private Activity _activity;
        public override ScreenName ScreenName => ScreenName.NewActivityScreen;

        protected override void OnBind()
        {
            base.OnBind();
            View.SetActionOnButtonBackClick(OnButtonBackClicked);
            View.SetActionOnButtonSettingsClick(OnButtonSettingsClicked);
            View.SetActionOnButtonCollaborativeSessionClick(OnButtonCollaborativeSessionClicked);
            View.SetActionOnButtonAddNewStepClick(OnButtonAddNewStepClicked);

            RootObject.Instance.LEE.ActivityManager.OnActivityLoaded += OnActivityUpdated;
            RootObject.Instance.LEE.ActivityManager.OnActivityUpdated += OnActivityUpdated;
        }

        private void OnActivityUpdated(Activity activity)
        {
            _activity = activity;
            UpdateView();
        }

        private void UpdateView()
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
            }
        }

        private void OnStepItemClicked(ActivityStep step)
        {
            RootObject.Instance.LEE.StepManager.GoToStep(step.Id);
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
            // TODO
            MenuManager.Instance.ShowCollaborativeSessionPanelView();
        }

        private void OnButtonSettingsClicked()
        {
            var prefab = View.GetEditorPrefab(ContentType.Audio);
            PopupsViewer.Instance.Show(prefab);
        }
    }
}
