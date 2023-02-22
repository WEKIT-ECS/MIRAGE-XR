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
        [SerializeField] private Button _btnGotIt;
        [SerializeField] private Button _selectionButton;
        private GameObject _highlightedObject;

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

        public Button CreateNewSelectionButton(string title)
        {
            var button = Instantiate(_selectionButton, Vector3.zero, Quaternion.identity);
            var rectTransform = button.GetComponent<RectTransform>();
            rectTransform.SetParent(gameObject.transform);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(400, 40);
            rectTransform.localScale = new Vector3(1, 1, 1);
            rectTransform.localPosition = new Vector3(rectTransform.position.x, rectTransform.position.y, 0);

            rectTransform.GetComponent<TMP_Text>().text = title;

            return button;
        }

        protected override bool TryToGetArguments(params object[] args)
        {
            return true;
        }
    }
}
