using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MirageXR
{
    /// <summary>
    /// This class is used in the mobile version of the tutorial
    /// to highlight UI elements that should be interacted with.
    /// Show() should be used to set up the highlight and Remove() 
    /// to remove it.
    /// </summary>
    public class TutorialObjectHighlighter
    {
        private GameObject _copy;

        /// <summary>
        /// Highlights the GameObject in the input. This works differently for
        /// different GameObjects, but basically it copies the GameObject and places
        /// it on top of the current screen. If the object is actually a Button, for example,
        /// it will also copy its OnClick() actions.
        /// </summary>
        /// <param name="gameObject"></param>
        public void Show(GameObject gameObject)
        {
            // If gameObject is TMP_InputField create a Button of the Field's size and
            // make sure control is passed on to the Field when the Button is clicked.
            TMP_InputField inputField = gameObject.GetComponent<TMP_InputField>();
            if (inputField != null)
            {
                GameObject hbPrefab = Resources.Load("prefabs/UI/Mobile/Tutorial/HighlightButton", typeof(GameObject)) as GameObject;
                _copy = Object.Instantiate(hbPrefab, gameObject.transform.position, gameObject.transform.rotation);
                _copy.transform.SetParent(RootView.Instance.transform);
                _copy.transform.SetPositionAndRotation(gameObject.transform.position, gameObject.transform.rotation);
                _copy.transform.localScale = gameObject.transform.localScale;

                var rectTransform = (RectTransform)gameObject.transform;
                var height = rectTransform.rect.height;
                var width = rectTransform.rect.width;

                var copyRectTransform = (RectTransform)_copy.transform;
                copyRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                copyRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
                copyRectTransform.SetPositionAndRotation(gameObject.transform.position, gameObject.transform.rotation);

                HighlightingButton higBtn = _copy.GetComponent<HighlightingButton>();
                higBtn.SetTarget(inputField);

                _copy.SetActive(true);
                return;
            }

            // If it's not an input field, make a copy of the object
            _copy = Object.Instantiate(gameObject);
            _copy.transform.SetParent(RootView.Instance.transform);
            _copy.transform.SetPositionAndRotation(gameObject.transform.position, gameObject.transform.rotation);
            _copy.transform.localScale = gameObject.transform.localScale;

            // If it's a button, also copy its OnClicks
            Button button = _copy.GetComponent<Button>();
            if (button != null)
            {
                button.onClick = gameObject.GetComponent<Button>().onClick;
            }

            //If it's a Toggle (in the mobile bottom mobile navigation bar), copy
            // its onValueChanged
            Toggle toggle = _copy.GetComponent<Toggle>();
            if (toggle != null)
            {
                toggle.interactable = true;
                toggle.onValueChanged = gameObject.GetComponent<Toggle>().onValueChanged;
            }

            _copy.SetActive(true);
        }

        /// <summary>
        /// If an object was highlighted, removes the highlight and
        /// destroys it.
        /// </summary>
        public void Remove()
        {
            if (_copy != null)
            {
                _copy.SetActive(false);
                Object.Destroy(_copy);
            }
        }
    }
}
