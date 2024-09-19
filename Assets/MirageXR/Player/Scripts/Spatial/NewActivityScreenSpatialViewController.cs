using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class NewActivityScreenSpatialViewController : ScreenViewController<NewActivityScreenSpatialViewController, NewActivityScreenSpatialView>
    {
        public override ScreenName ScreenName => ScreenName.NewActivityScreen;

        protected override void OnBind()
        {
            base.OnBind();
            View.SetActionOnButtonBackClick(OnButtonBackClicked);
            View.SetActionOnButtonSettingsClick(OnButtonSettingsClicked);
            View.SetActionOnButtonCollaborativeSessionClick(OnButtonCollaborativeSessionClicked);
            View.SetActionOnButtonAddNewStepClick(OnButtonAddNewStepClicked);
        }

        private void OnButtonBackClicked()
        {
            MenuManager.Instance.ShowScreen(ScreenName.MainScreen);
        }
        
        private void OnButtonAddNewStepClicked()
        {
            // TODO
        }

        private void OnButtonCollaborativeSessionClicked()
        {
            // TODO
        }

        private void OnButtonSettingsClicked()
        {
            // TODO
        }
    }
}
