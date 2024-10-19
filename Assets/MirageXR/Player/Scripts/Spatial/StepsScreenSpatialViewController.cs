using System.Collections.Generic;
using LearningExperienceEngine.DataModel;
using UnityEngine;

namespace MirageXR
{
    public class StepsScreenSpatialViewController : ScreenViewController<StepsScreenSpatialViewController, StepsScreenSpatialView>
    {
        public override ScreenName ScreenName => ScreenName.StepsScreen;

        private ActivityStep _step;
        private List<Content> _contents;
        
        protected override void OnBind()
        {
            base.OnBind();
            View.SetActionOnButtonBackClick(OnButtonBackClicked);
            View.SetActionOnButtonAddAugmentationClick(OnButtonAddAugmentationClicked);
            
            RootObject.Instance.LEE.StepManager.OnStepChanged += StepManagerOnStepChanged;
            RootObject.Instance.LEE.ContentManager.OnContentActivated += ContentManagerOnContentActivated;
        }

        private void ContentManagerOnContentActivated(List<Content> contents)
        {
            _contents = contents;
            UpdateView();
        }

        private void StepManagerOnStepChanged(ActivityStep step)
        {
            _step = step;
        }

        private void UpdateView()
        {
            var container = View.GetStepContainer();
            var prefab = View.GetContetItemViewPrefab();

            foreach (Transform item in container)
            {
                Destroy(item.gameObject);
            }

            foreach (var content in _contents)
            {
                var item = Instantiate(prefab, container);
                item.Initialize(content, OnStepItemClick);
            }
        }

        private void OnStepItemClick(Content content)
        {
            var prefab = View.GetEditorPrefab(content.Type);
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
