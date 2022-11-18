using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class HelpStepSearch : HelpStep
    {
        protected override void Init()
        {
            this.instructionText = "To search for an activity by name, tap the search menu item below.";
            GameObject search = GameObject.Find("Search");//RootView_v2.Instance._searchPrefab.gameObject;
            EventManager.NewActivityCreationButtonPressed += DefaultExitEventListener;

            Button button = search.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(SecuredExitStep);
            }
            this.highlightedObject = search;
        }

        protected override void Detach()
        {
            EventManager.NewActivityCreationButtonPressed -= DefaultExitEventListener;
        }

        protected override void SecuredExitStep()
        {
            Detach();
            RemoveHighlight();
            RemoveInstruction();
        }
    }
}
