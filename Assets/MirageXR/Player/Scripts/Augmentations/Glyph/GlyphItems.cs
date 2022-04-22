using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MirageXR
{
    public class GlyphItems : MirageXRPrefab
    {
        ToggleObject myObj;
        [SerializeField] private GameObject icon;

        private void OnEnable()
        {
            EventManager.OnEditModeChanged += SetEditorState;
        }

        private void OnDisable()
        {
            EventManager.OnEditModeChanged -= SetEditorState;
        }

        private void Start()
        {
            SetEditorState(ActivityManager.Instance.EditModeActive);
        }

        private void SetEditorState(bool editModeActive)
        {
            if(icon)
                icon.SetActive(editModeActive);

            var boundsControl = GetComponent<BoundsControl>();
            if (boundsControl != null)
                boundsControl.Active = editModeActive;
        }

        public override bool Init(ToggleObject obj)
        {
            myObj = obj;

            // Try to set the parent and if it fails, terminate initialization.
            if (!SetParent(obj))
            {
                Debug.Log("Couldn't set the parent.");
                return false;
            }

            name = obj.predicate;

            obj.text = name;

            // load scaling
            transform.localScale = obj.scale != 0 ? new Vector3(obj.scale, obj.scale, obj.scale) : Vector3.one;
            //load rotation
            if (Utilities.TryParseStringToQuaternion(obj.rotation, out Quaternion myRotation))
                transform.localRotation = myRotation;

            // If everything was ok, return base result.
            return base.Init(obj);
        }



    }
}