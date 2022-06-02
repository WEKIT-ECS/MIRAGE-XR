using Microsoft.MixedReality.Toolkit.UI;
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
        private static Text textbox; // the label textMesh
        private Image _triggerIcon;

        private ToggleObject _myAnnotation;

        /// <summary>
        /// Initialization method.
        /// </summary>
        /// <param name="obj">Action toggle object.</param>
        /// <returns>Returns true if initialization succesfull.</returns>
        public override bool Init (ToggleObject obj)
        {
            // Check if the label text is set.
            if (string.IsNullOrEmpty (obj.text))
            {
                Debug.Log ("Label text not provided.");
                return false;
            }

            // Try to set the parent and if it fails, terminate initialization.
            if (!SetParent (obj))
            {
                Debug.Log ("Couldn't set the parent.");
                return false;
            }

            _myAnnotation = obj;

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

            // Set scaling if defined in action configuration.
            var myPoiEditor = transform.parent.gameObject.GetComponent<PoiEditor>();
            transform.parent.localScale = GetPoiScale(myPoiEditor, Vector3.one);

            // If everything was ok, return base result.
            return base.Init(obj);
        }

        private void InstantiateTextLabel()
        {
            if (textLabel == null)
            {
                textLabel = Instantiate(TextLabelPrefab);
                _triggerIcon = textLabel.GetComponentsInChildren<Image>()[1]; // TODO: possible NRE
                if (_triggerIcon && RootObject.Instance.activityManager.ActiveAction.triggers.Find(t => t.id == _myAnnotation.poi) != null)
                {
                    _triggerIcon.enabled = true;
                }
            }

            textbox = textLabel.GetComponentInChildren<Text>();
        }

    }
}