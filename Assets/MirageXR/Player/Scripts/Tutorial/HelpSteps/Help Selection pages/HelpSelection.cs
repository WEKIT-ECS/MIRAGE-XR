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
    public abstract class HelpSelection : TutorialStep
    {
        protected string _instructionText;

        protected HelpSelectionPopup _popup;
        private TutorialObjectHighlighter _highlighter;

        private bool _isHighlighting = false;

        protected override void SecuredEnterStep()
        {
            SetupInstruction();
            SetupHighlighter();
            Init();
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
        }

        protected abstract void Init();
 
        protected abstract void Detach();

        protected void SetupInstruction()
        {
            _popup = (HelpSelectionPopup)PopupsViewer.Instance.Show(manager.HelpSelectionPopup);
        }

        protected void SetupHighlighter()
        {
            if (highlightedObject != null)
            {
                _highlighter = manager.MobileHighlighter;
                _highlighter.Show(highlightedObject);
                _isHighlighting = true;
                EventManager.TutorialPopupCloseClicked += RemoveHighlight;
                EventManager.HighlightingButtonClicked += RemoveHighlight;
                EventManager.HighlightingButtonClicked += RemoveInstruction;
            }
        }

        protected void RemoveInstruction()
        {
            if (_popup != null)
            {
                _popup.Close();
            }

            if (_isHighlighting)
            {
                EventManager.HighlightingButtonClicked -= RemoveInstruction;
            }
        }

        protected void RemoveHighlight()
        {
            if (_isHighlighting)
            {
                _highlighter.Remove();
                EventManager.TutorialPopupCloseClicked -= RemoveHighlight;
                EventManager.HighlightingButtonClicked -= RemoveHighlight;
                _isHighlighting = false;
            }
        }
    }
}
