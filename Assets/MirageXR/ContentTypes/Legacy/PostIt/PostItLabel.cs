using i5.Toolkit.Core.VerboseLogging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class PostItLabel : MirageXRPrefab
    {
        [SerializeField] private TMP_Text _tmpLabelText;
        [SerializeField] private Text _legacyLabelText;
        [SerializeField] private Image _background;
        
        private RectTransform _rectTransform;
        [SerializeField] private GameObject TextLabelPrefab;
        private GameObject _instantiatedTextLabel;
        private LearningExperienceEngine.ToggleObject _obj;
        private SimpleBoundingBox _boundingBox;

        public override bool Init(LearningExperienceEngine.ToggleObject obj)
        {
            _obj = obj;
            _rectTransform = GetComponent<RectTransform>();

            // Find text components if not assigned
            if (_tmpLabelText == null)
            {
                _tmpLabelText = GetComponentInChildren<TMP_Text>();
            }
            if (_legacyLabelText == null && _tmpLabelText == null)
            {
                _legacyLabelText = GetComponentInChildren<Text>();
            }

            if (_background == null)
            {
                _background = GetComponentInChildren<Image>();
            }

            // If still missing text and TextLabelPrefab is provided, instantiate it
            if (_tmpLabelText == null && _legacyLabelText == null && TextLabelPrefab != null)
            {
                _instantiatedTextLabel = Instantiate(TextLabelPrefab, transform);
                _instantiatedTextLabel.transform.localPosition = Vector3.zero;
                _tmpLabelText = _instantiatedTextLabel.GetComponentInChildren<TMP_Text>();
                _background = _instantiatedTextLabel.GetComponentInChildren<Image>();
            }

            // Check if label text is set
            if (string.IsNullOrEmpty(obj.text))
            {
                Debug.LogWarning("[PostItLabel] Label text not provided.");
                return false;
            }

            // Set parent
            if (!SetParent(obj))
            {
                Debug.LogWarning("[PostItLabel] Couldn't set the parent.");
                return false;
            }

            name = $"{obj.predicate}_{obj.text.Split(' ')[0]}";

            // Set text
            if (_tmpLabelText != null)
            {
                _tmpLabelText.text = obj.text;
            }
            else if (_legacyLabelText != null)
            {
                _legacyLabelText.text = obj.text;
            }

            // Set scale
            if (_rectTransform != null)
            {
                if (!obj.scale.Equals(0))
                {
                    _rectTransform.localScale = new Vector3(obj.scale, obj.scale, obj.scale) / 2048;
                }
                else
                {
                    _rectTransform.localScale = new Vector3(0.1f, 0.1f, 0.1f) / 2048;
                }
            }

            // Guide line
            if (!obj.id.Equals("UserViewport"))
            {
                if (!SetGuide(obj))
                {
                    return false;
                }
            }
            else
            {
                if (GetComponent<Billboard>() == null)
                {
                    gameObject.AddComponent<Billboard>();
                }
            }

            // Option parsing (fontSize-fontColor-backgroundColor)
            if (!string.IsNullOrEmpty(obj.option))
            {
                string[] splitArray = obj.option.Split('-');
                if (splitArray.Length > 0 && int.TryParse(splitArray[0], out int fontSize))
                {
                    if (_tmpLabelText != null) _tmpLabelText.fontSize = fontSize;
                    if (_legacyLabelText != null) _legacyLabelText.fontSize = fontSize;
                }

                if (splitArray.Length > 1 && ColorUtility.TryParseHtmlString(splitArray[1], out Color fontColor))
                {
                    if (_tmpLabelText != null) _tmpLabelText.color = fontColor;
                    if (_legacyLabelText != null) _legacyLabelText.color = fontColor;
                }

                if (splitArray.Length > 2 && _background != null && ColorUtility.TryParseHtmlString(splitArray[2], out Color bgColor))
                {
                    _background.color = bgColor;
                }
            }

            // Billboarding
            var billboard = GetComponent<Billboard>();
            if (billboard != null)
            {
                billboard.enabled = obj.billboarded;
            }

            // Add SimpleBoundingBox for manipulation in editor
            _boundingBox = gameObject.GetComponent<SimpleBoundingBox>();
            if (_boundingBox == null)
            {
                _boundingBox = gameObject.AddComponent<SimpleBoundingBox>();
            }
            _boundingBox.Target = transform;
            _boundingBox.Setup(obj);

            OnLock(_obj.poi, _obj.positionLock);
            LearningExperienceEngine.EventManager.OnAugmentationLocked += OnLock;

            GetComponent<PostItSizeController>()?.AdjustSize();

            return true;
        }

        private void OnDestroy()
        {
            LearningExperienceEngine.EventManager.OnAugmentationLocked -= OnLock;
        }

        private void OnLock(string id, bool locked)
        {
            if (_obj != null && id == _obj.poi)
            {
                _obj.positionLock = locked;
                if (_boundingBox != null)
                {
                    _boundingBox.SetEnabled(!locked);
                }
            }
        }
    }
}