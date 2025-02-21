using LearningExperienceEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using UnityEngine;

namespace MirageXR
{
    public class GlyphItems : MirageXRPrefab
    {
        private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;
        private LearningExperienceEngine.ToggleObject _obj;
        private ObjectManipulator _objectManipulator;
        
        [SerializeField] private GameObject icon;


        private void OnEnable()
        {
            LearningExperienceEngine.EventManager.OnEditModeChanged += SetEditorState;
        }

        private void OnDisable()
        {
            LearningExperienceEngine.EventManager.OnEditModeChanged -= SetEditorState;
            LearningExperienceEngine.EventManager.OnAugmentationLocked -= OnLock;
        }

        private void Start()
        {
            _objectManipulator = GetComponent<ObjectManipulator>();
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

        public override bool Init(LearningExperienceEngine.ToggleObject obj)
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

            LearningExperienceEngine.EventManager.OnAugmentationLocked += OnLock;

            // If everything was ok, return base result.
            return base.Init(obj);
        }

        private void OnLock(string id, bool locked)
        {
            if (id == _obj.poi)
            {
                _obj.positionLock = locked;

                SetBoundsControl(!_obj.positionLock);

                if (_objectManipulator)
                {
                    _objectManipulator.enabled = !_obj.positionLock;
                }

                GetComponentInParent<PoiEditor>().IsLocked(_obj.positionLock);
            }
        }

        private void SetBoundsControl(bool bounds)
        {
            var boundsControl = GetComponent<BoundsControl>();
            if (boundsControl != null)
            {
                boundsControl.enabled = bounds;
                boundsControl.RotationHandlesConfig.ShowHandleForX = bounds;
                boundsControl.RotationHandlesConfig.ShowHandleForY = bounds;
                boundsControl.RotationHandlesConfig.ShowHandleForZ = bounds;
            }
        }
    }
}