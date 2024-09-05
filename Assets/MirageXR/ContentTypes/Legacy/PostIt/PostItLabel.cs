using i5.Toolkit.Core.VerboseLogging;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class PostItLabel : MirageXRPrefab
    {
        // Label components.
        private Text _labelText;
        private RectTransform _transform;

        /// <summary>
        /// Initialization method.
        /// </summary>
        /// <param name="obj">Action toggle object.</param>
        /// <returns>Returns true if initialization succesfull.</returns>
        public override bool Init(LearningExperienceEngine.ToggleObject obj)
        {
            // Attach label text object.
            _labelText = transform.Find("Text").gameObject.GetComponent<Text>();
            _transform = GetComponent<RectTransform>();

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

            // Set name.
            name = obj.predicate;

            // Set scale, if defined in the action step configuration.
            if (!obj.scale.Equals(0))
                _transform.localScale = new Vector3(obj.scale, obj.scale, obj.scale) / 2048;

            // If scaling is not set, default to 10 cm height.
            else
                _transform.localScale = new Vector3(0.1f, 0.1f, 0.1f) / 2048;

            // Set label text.
            _labelText.text = obj.text;

            if (!obj.id.Equals("UserViewport"))
            {
                // Setup guide line feature.
                if (!SetGuide(obj))
                    return false;
            }

            else
            {
                gameObject.AddComponent<Billboard>();
            }

            // If everything was ok, return true.
            return true;
        }
    }
}