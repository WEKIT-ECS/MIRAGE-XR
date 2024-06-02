using System;
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
            var target = RootView_v2.Instance.bottomPanelView.btnCreate;
            this.highlightedObject = target.gameObject;
            target.onValueChanged.AddListener(onToggleValueChanged);
        }

        private void onToggleValueChanged(bool isOn)
        {
            if (isOn)
            {
                Console.WriteLine("Sam Says");
            }
            else
            {
                Console.WriteLine("Simon Says");
            }
        }

        protected override void Detach()
        {
            EventManager.NewActivityCreationButtonPressed -= DefaultExitEventListener;
        }
    }
}
