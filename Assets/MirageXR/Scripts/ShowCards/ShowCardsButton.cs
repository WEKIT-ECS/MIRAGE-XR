using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class ShowCardsButton : MirageXRPrefab
    {
        private UiManager manager;

        public override bool Init(LearningExperienceEngine.ToggleObject obj)
        {
            // Try to set the parent and if it fails, terminate initialization.
            if (!SetParent(obj))
            {
                Debug.Log("Couldn't set the parent.");
                return false;
            }

            // Set name.
            name = obj.predicate;

            // Get rect transform
            var rectTransform = GetComponent<RectTransform>();

            // Set scale, if defined in the action step configuration.
            if (!obj.scale.Equals(0))
                rectTransform.localScale = new Vector3(obj.scale, obj.scale, obj.scale) / 2048;

            // If scaling is not set, default to 5 cm symbols.
            else
                rectTransform.localScale = new Vector3(0.05f, 0.05f, 0.05f) / 2048;

            // Everything is fine!
            return true;
        }

        void Start()
        {
            manager = GameObject.Find("UiManager").GetComponent<UiManager>();

            var btn = transform.GetComponentInChildren<Button>();
            btn.onClick.AddListener(OnClick);
        }

        void OnClick()
        {
            manager.ToggleMenu();
        }
    }
}