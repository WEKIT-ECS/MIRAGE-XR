using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Steps intended for use in the mobile version of the tutorial (Mobile UI).
    /// It uses the TutorialPopup to present instructions to the user and the
    /// TutorialObjectHighlighter to highlight UI elements that should be interacted with.
    /// </summary>
    public abstract class HelpStep : TutorialStep
    {
        protected string instructionText;

        private TutorialPopup popup;
        private TutorialObjectHighlighter highlighter;

        private bool isHighlighting = false;

        protected override void SecuredEnterStep()
        {
            Init();
            SetupInstruction();
            SetupHighlighter();
        }

        protected override void SecuredCloseStep()
        {
            Detach();
            RemoveHighlight();
            RemoveInstruction();
        }

        protected override void SecuredExitStep()
        {
            Detach();
            RemoveHighlight();
            RemoveInstruction();
           // manager.NextStep();
        }

        protected abstract void Init();
 
        protected abstract void Detach();

        protected void SetupInstruction()
        {
            popup = (TutorialPopup)PopupsViewer.Instance.Show(manager.MobilePopup);
            popup.SetInstructionText(instructionText);
        }

        protected void SetupHighlighter()
        {
            if (highlightedObject != null)
            {
                highlighter = manager.MobileHighlighter;
                highlighter.Show(highlightedObject);
                isHighlighting = true;
                EventManager.TutorialPopupCloseClicked += RemoveHighlight;
                EventManager.HighlightingButtonClicked += RemoveHighlight;
                EventManager.HighlightingButtonClicked += RemoveInstruction;
            }
        }

        protected void RemoveInstruction()
        {
            if (popup != null)
            {
                popup.Close();
            }

            if (isHighlighting)
            {
                EventManager.HighlightingButtonClicked -= RemoveInstruction;
            }
        }

        protected void RemoveHighlight()
        {
            if (isHighlighting)
            {
                highlighter.Remove();
                EventManager.TutorialPopupCloseClicked -= RemoveHighlight;
                EventManager.HighlightingButtonClicked -= RemoveHighlight;
                isHighlighting = false;
            }
        }
    }
}
