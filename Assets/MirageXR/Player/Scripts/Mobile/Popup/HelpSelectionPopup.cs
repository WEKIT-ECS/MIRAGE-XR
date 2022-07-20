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
    public class HelpSelectionPopup : PopupBase
    {
        [SerializeField] private Button btnGotIt;
        [SerializeField] private Button selectionButton;
        private GameObject highlightedObject;

        /// <summary>
        /// Sets the text that is going to be shown on the popup.
        /// </summary>
        /// <param name="text">The instruction text to be shown.</param>

        public override void Init(Action<PopupBase> onClose, params object[] args)
        {
            base.Init(onClose, args);
            btnGotIt.onClick.AddListener(GotItOnClick);
        }

        private void GotItOnClick()
        {
            EventManager.NotifyOnTutorialPopupCloseClicked();
            Close();
        }

        private void textOnclick()
        {
            EventManager.NotifyOnTutorialPopupCloseClicked();
            Close();
            TutorialManager.Instance.showHelp(1);
        }

        public Button createNewSelectionButton(string title) {

            var button = Instantiate(selectionButton, Vector3.zero, Quaternion.identity) as Button;
            var rectTransform = button.GetComponent<RectTransform>();
            rectTransform.SetParent(gameObject.transform);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(100, 50);
            rectTransform.GetComponent<TMP_Text>().text = title;

            return button;
        }

        protected override bool TryToGetArguments(params object[] args)
        {
            return true;
        }
    }
}
