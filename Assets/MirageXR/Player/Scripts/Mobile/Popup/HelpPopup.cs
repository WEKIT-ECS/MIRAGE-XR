﻿using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MirageXR
{
    /// <summary>
    /// Popup that presents instructions to the user in the mobile version
    /// of the tutorial. Uses the PopupBase to do so.
    /// </summary>
    public class HelpPopup : PopupBase
    {
        [SerializeField] private Button _btnGotIt;
        [SerializeField] private TMP_Text _txtInstruction;
        private GameObject _highlightedObject;

        /// <summary>
        /// Sets the text that is going to be shown on the popup.
        /// </summary>
        /// <param name="text">The instruction text to be shown.</param>

        public void SetInstructionText(String text)
        {
            _txtInstruction.text = text;
        }

        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);
            _btnGotIt.onClick.AddListener(GotItOnClick);
        }

        private void GotItOnClick()
        {
            EventManager.NotifyOnTutorialPopupCloseClicked();
            Close();
        }

        protected override bool TryToGetArguments(params object[] args)
        {
            return true;
        }
    }
}
