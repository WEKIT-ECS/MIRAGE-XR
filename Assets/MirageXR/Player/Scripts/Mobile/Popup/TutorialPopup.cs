using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MirageXR
{
    /// <summary>
    /// Popup that presents instructions to the user in the mobile version
    /// of the tutorial. Uses the PopupBase to do so.
    /// </summary>
    public class TutorialPopup : PopupBase
    {
        [SerializeField] private RectTransform _background;
        [SerializeField] private Button _btnGotIt;
        [SerializeField] private TMP_Text _txtInstruction;
        private GameObject highlightedObject;

        /// <summary>
        /// Sets the text that is going to be shown on the popup.
        /// </summary>
        /// <param name="text">The instruction text to be shown.</param>
        public void SetInstructionText(String text)
        {
            _txtInstruction.text = text;
        }
        
        public override void Init(Action<PopupBase> onClose, params object[] args)
        {
            base.Init(onClose, args);

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

        public void MovePopup()
        {
            var newY = gameObject.transform.localPosition.y - _background.rect.height;

            gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, newY, gameObject.transform.localPosition.z);
        }
    }
}
