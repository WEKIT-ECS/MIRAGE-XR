using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class MobileStepCreateActivity : MobileStep
    {
        protected override void Init()
        {
            this.instructionText = "Tap the plus button below to start a new activity.";
            this.highlightedObject = RootView.Instance.activityListView.BtnAddActivity.gameObject;
            EventManager.NewActivityCreationButtonPressed += DefaultExitEventListener;
        }

        protected override void Detach()
        {
            EventManager.NewActivityCreationButtonPressed -= DefaultExitEventListener;
        }
    }
}
