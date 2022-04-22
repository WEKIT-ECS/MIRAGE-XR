using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
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
        public GameObject TextLabelPrefab;
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
            name = obj.predicate + "_" + obj.text.Split (' ') [0];


            // Set scale, if defined in the action step configuration.
            //if (!obj.scale.Equals (0))
            //    transform.localScale = new Vector3 (obj.scale, obj.scale, obj.scale);

            // If scaling is not set, default to 10 cm height.
            //else
            //    transform.localScale = new Vector3 (0.1f, 0.1f, 0.1f);

            InstantiateTxtLbl();
            textLabel.transform.parent = gameObject.transform;
            textLabel.transform.localPosition = Vector3.zero;

            // Set label text.
            textbox.text = obj.text;
            //GetComponent<TextMesh> ().text = obj.text;

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


            // Set scaling if defined in action configuration.
            PoiEditor myPoiEditor = transform.parent.gameObject.GetComponent<PoiEditor>();
            transform.parent.localScale = GetPoiScale(myPoiEditor, Vector3.one);

            // If everything was ok, return base result.
            return base.Init(obj);
        }


        public void InstantiateTxtLbl()
        {
            if (textLabel == null)
            {
                textLabel = Instantiate(TextLabelPrefab);
                _triggerIcon = textLabel.GetComponentsInChildren<Image>()[1]; //TriggerIcon
                if (_triggerIcon && ActivityManager.Instance.ActiveAction.triggers.Find(t => t.id == _myAnnotation.poi) != null) _triggerIcon.enabled = true;
            }

            textbox = textLabel.GetComponentInChildren<Text>();
        }

    }
}