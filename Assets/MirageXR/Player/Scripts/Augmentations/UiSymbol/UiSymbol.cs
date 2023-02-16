using i5.Toolkit.Core.VerboseLogging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class UiSymbol : MirageXRPrefab
    {
        private void Awake()
        {
            // We don't need the gaze guide for UI symbols.
            UseGuide = false;
        }

        /// <summary>
        /// Initialization method.
        /// </summary>
        /// <param name="content">Action toggle object.</param>
        /// <returns>Returns true if initialization succesfull.</returns>
        public override bool Init(ToggleObject content)
        {
            // Try to fetch the symbol sprite from the resources.
            var symbol = Resources.Load<Sprite>(content.predicate);

            // If symbol couldn't be found, terminate initialization.
            if (symbol == null)
            {
                AppLog.LogWarning("Symbol couldn't be found. " + content.predicate);
                return false;
            }

            // Set the displayed sprite to the one just loaded.
            GetComponent<Image>().sprite = symbol;

            // Set parent.
            transform.SetParent(GameObject.FindGameObjectWithTag("ActionUi.Symbols").transform);

            // Get the rect transform component.
            var rectTransform = GetComponent<RectTransform>();

            // Set initial transform.
            rectTransform.localPosition = Vector3.zero;

            // Set initial rotation.
            rectTransform.localEulerAngles = Vector3.zero;

            // Set initial scale.
            rectTransform.localScale = Vector3.one;

            // Set final position, if defined in action step.
            if (!string.IsNullOrEmpty(content.position))
                rectTransform.localPosition = Utilities.ParseStringToVector3(content.position);

            // Set final rotation, if defined in action step.
            if (!string.IsNullOrEmpty(content.rotation))
                rectTransform.localEulerAngles = Utilities.ParseStringToVector3(content.rotation);

            // Set name.
            name = content.predicate;

            return true;
        }
    }
}