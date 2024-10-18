using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class StepsScreenSpatialViewController : ScreenViewController<StepsScreenSpatialViewController, StepsScreenSpatialView>
    {
        public override ScreenName ScreenName => ScreenName.StepsScreen;
        
        protected override void OnBind()
        {
            base.OnBind();
            View.SetActionOnButtonBackClick(OnButtonBackClicked);
            View.SetActionOnButtonAddAugmentationClick(OnButtonAddAugmentationClicked);
        }

        private void OnButtonAddAugmentationClicked()
        {
            MenuManager.Instance.ShowSelectAugmentationScreenSpatialView();
        }

        private void OnButtonBackClicked()
        {
            // TODO
        }
    }
}
