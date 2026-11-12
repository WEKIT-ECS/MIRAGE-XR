using LearningExperienceEngine;
using UnityEngine;

namespace MirageXR
{
    public class GlyphItems : MirageXRPrefab
    {
        private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;
        private LearningExperienceEngine.ToggleObject _obj;
        
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
            SetEditorState(activityManager.EditModeActive);
        }

        private void SetEditorState(bool editModeActive)
        {
            if (icon)
            {
                icon.SetActive(editModeActive);
            }

            SetBoundsControl(editModeActive);

            if (_obj != null)
            {
                OnLock(_obj.poi, _obj.positionLock);
            }
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
            if (_obj != null && id == _obj.poi)
            {
                _obj.positionLock = locked;

                SetBoundsControl(!_obj.positionLock);

                var poiEditor = GetComponentInParent<PoiEditor>();
                if (poiEditor != null)
                {
                    poiEditor.IsLocked(_obj.positionLock);
                }
            }
        }

        private void SetBoundsControl(bool bounds)
        {
            var simpleBoundingBox = GetComponent<SimpleBoundingBox>();
            if (simpleBoundingBox != null)
            {
                simpleBoundingBox.SetEnabled(bounds);
                simpleBoundingBox.RotationHandlesConfig.ShowHandleForX = bounds;
                simpleBoundingBox.RotationHandlesConfig.ShowHandleForY = bounds;
                simpleBoundingBox.RotationHandlesConfig.ShowHandleForZ = bounds;
            }
        }
    }
}