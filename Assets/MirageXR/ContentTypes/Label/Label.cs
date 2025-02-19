using System.Globalization;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    /// <summary>
    /// Class for text label prefabs.
    /// </summary>
    public class Label : MirageXRPrefab
    {
        [SerializeField] private GameObject TextLabelPrefab;
        private GameObject textLabel;
        private static TMP_Text textbox; // the label textMesh
        private Image _triggerIcon;
        private Image _labelBackground;

        private LearningExperienceEngine.ToggleObject _obj;

        /// <summary>
        /// Initialization method.
        /// </summary>
        /// <param name="obj">Action toggle object.</param>
        /// <returns>Returns true if initialization succesfull.</returns>
        public override bool Init(LearningExperienceEngine.ToggleObject obj)
        {
            // Check if the label text is set.
            if (string.IsNullOrEmpty(obj.text))
            {
                Debug.LogWarning("Label text not provided.");
                return false;
            }

            // Try to set the parent and if it fails, terminate initialization.
            if (!SetParent(obj))
            {
                Debug.LogWarning("Couldn't set the parent.");
                return false;
            }

            _obj = obj;

            // Set name.
            name = $"{obj.predicate}_{obj.text.Split(' ')[0]}";

            InstantiateTextLabel();
            textLabel.transform.SetParent(transform);
            textLabel.transform.localPosition = Vector3.zero;

            // Set label text.
            textbox.text = obj.text;
            // GetComponent<TextMesh> ().text = obj.text;

            if (!obj.id.Equals("UserViewport"))
            {
                // Setup guide line feature.
                if (!SetGuide(obj))
                {
                    return false;
                }
            }
            else
            {
                gameObject.AddComponent<Billboard>();
            }

            if (obj.option != "")
            {
                string[] splitArray = obj.option.Split(char.Parse("-"));

                textbox.fontSize = int.Parse(splitArray[0]);

                textbox.color = GetColorFromString(splitArray[1]);
                _labelBackground.color = GetColorFromString(splitArray[2]);
            }

            GetComponentInChildren<Billboard>().enabled = obj.billboarded;

            // Set scaling if defined in action configuration.
            var myPoiEditor = transform.parent.gameObject.GetComponent<PoiEditor>();


            if (!_obj.billboarded)
            {
                myPoiEditor = transform.parent.gameObject.GetComponent<PoiEditor>();
                transform.localEulerAngles = GetPoiRotation(myPoiEditor);
            }

            OnLock(_obj.poi, _obj.positionLock);
            LearningExperienceEngine.EventManager.OnAugmentationLocked += OnLock;

            // If everything was ok, return base result.
            return base.Init(obj);
        }

        private void InstantiateTextLabel()
        {
            if (textLabel == null)
            {
                textLabel = Instantiate(TextLabelPrefab);
                _triggerIcon = textLabel.GetComponentsInChildren<Image>()[1]; // TODO: possible NRE
                if (_triggerIcon && LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.ActiveAction.triggers.Find(t => t.id == _obj.poi) != null)
                {
                    _triggerIcon.enabled = true;
                }
            }

            textbox = textLabel.GetComponentInChildren<TMP_Text>();
            _labelBackground = textLabel.GetComponentInChildren<Image>();
        }

        private void OnLock(string id, bool locked)
        {
            if (id == _obj.poi)
            {
                _obj.positionLock = locked;

                GetComponentInParent<PoiEditor>().IsLocked(_obj.positionLock);
            }
        }

        private Color GetColorFromString(string rgb)
        {
            string[] rgba = rgb.Substring(5, rgb.Length - 6).Split(", ");
            Color color = new Color(float.Parse(rgba[0], CultureInfo.InvariantCulture), float.Parse(rgba[1], CultureInfo.InvariantCulture), float.Parse(rgba[2], CultureInfo.InvariantCulture), float.Parse(rgba[3], CultureInfo.InvariantCulture));

            return color;
        }

        private void OnDisable()
        {
            LearningExperienceEngine.EventManager.OnAugmentationLocked -= OnLock;
        }
    }
}
