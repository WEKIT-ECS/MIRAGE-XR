using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class GlyphItems : MirageXRPrefab
    {
        private static ActivityManager activityManager => RootObject.Instance.activityManager;
        private ToggleObject _obj;
        [SerializeField] private GameObject icon;


        private void OnEnable()
        {
            EventManager.OnEditModeChanged += SetEditorState;
        }

        private void OnDisable()
        {
            EventManager.OnEditModeChanged -= SetEditorState;
            EventManager.OnAugmentationLocked -= OnLock;
        }

        private void Start()
        {
            SetEditorState(activityManager.EditModeActive);
        }

        private void SetEditorState(bool editModeActive)
        {
            if (icon)
            {
                icon.SetActive(editModeActive);
            }

            SetBoundsControl(editModeActive);
            OnLock(_obj.poi, _obj.positionLock);
        }

        public override bool Init(ToggleObject obj)
        {
            _obj = obj;

            // Try to set the parent and if it fails, terminate initialization.
            if (!SetParent(obj))
            {
                Debug.LogWarning("Couldn't set the parent.");
                return false;
            }

            name = obj.predicate;
            obj.text = name;
            transform.localScale = obj.scale != 0 ? new Vector3(obj.scale, obj.scale, obj.scale) : Vector3.one;

            EventManager.OnAugmentationLocked += OnLock;

            StartCoroutine(waitForEndOfFrame());

            // If everything was ok, return base result.
            return base.Init(obj);
        }

        private IEnumerator waitForEndOfFrame()
        {
            yield return 0;

            OnLock(_obj.poi, _obj.positionLock);
        }

        private void OnLock(string id, bool locked)
        {
            if (id == _obj.poi)
            {
                _obj.positionLock = locked;

                SetBoundsControl(!_obj.positionLock);

                if (gameObject.GetComponent<ObjectManipulator>())
                {
                    gameObject.GetComponent<ObjectManipulator>().enabled = !_obj.positionLock;
                }

                GetComponentInParent<PoiEditor>().IsLocked(_obj.positionLock, false);
            }
        }

        private void SetBoundsControl(bool bounds)
        {
            var boundsControl = GetComponent<BoundsControl>();
            if (boundsControl != null)
            {
                boundsControl.Active = bounds;
            }
        }

        public bool isLocked()
        {
            return _obj.positionLock;
        }
    }
}