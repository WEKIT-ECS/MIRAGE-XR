using Microsoft.MixedReality.Toolkit.Experimental.Physics;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.UI.BoundsControlTypes;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace MirageXR
{
    public enum BoundingRotationType
    {
        ALL,
        X,
        Y,
        Z
    }

    public class BoundingBoxGenerator : MonoBehaviour
    {
        [Tooltip("Check this if you will disable only bounding box of some of the children of this object when editmode is disabled. Or if you do not want enable bounding box when edit mode is on. ")]
        [SerializeField] private bool manualEditModeHandling;
        private bool _isLocked = false;

        public ScaleHandlesConfiguration CustomScaleHandlesConfiguration
        {
            get; set;
        }

        public RotationHandlesConfiguration CustomRotationHandlesConfiguration
        {
            get; set;
        }

        private void OnEnable()
        {
            LearningExperienceEngine.EventManager.OnEditModeChanged += EditModeState;
        }

        private void OnDisable()
        {
            LearningExperienceEngine.EventManager.OnEditModeChanged -= EditModeState;
        }

        private void Start()
        {
            Invoke(nameof(ManipulationEvents), 0.2f);
        }

        /// <summary>
        /// add bounding box around the mesh of the toggleobject
        /// </summary>
        /// <param name="annotationToggleObject"></param>
        /// <param name="boundsCalculationMethod"></param>
        /// <param name="hasConstraintManager"></param>
        /// <param name="addListeners"></param>
        /// <param name="boundingRotationType"></param>
        /// <param name="AddManipulator"></param>
        /// <returns>Task object for asynchronous execution</returns>
        public async Task AddBoundingBox(
            LearningExperienceEngine.ToggleObject annotationToggleObject,
            BoundsCalculationMethod boundsCalculationMethod,
            bool hasConstraintManager = false,
            bool addListeners = true,
            BoundingRotationType boundingRotationType = BoundingRotationType.ALL,
            bool AddManipulator = false)
        {
            _isLocked = annotationToggleObject.positionLock;

            if (!hasConstraintManager && !GetComponent<ConstraintManager>())
            {
                gameObject.AddComponent<ConstraintManager>();
            }

            var elasticManager = GetComponent<ElasticsManager>();
            if (!elasticManager)
            {
                elasticManager = gameObject.AddComponent<ElasticsManager>();
            }

            var boundsControl = gameObject.GetComponent<BoundsControl>();
            if (!boundsControl)
            {
                boundsControl = gameObject.AddComponent<BoundsControl>();
            }

            boundsControl.Target = gameObject;
            boundsControl.ElasticsManager = elasticManager;

            // if the transform changes should be apply to the target object, otherwise it should be added manually
            if (addListeners)
            {
                boundsControl.ScaleStopped.AddListener(() => SaveTransform(annotationToggleObject));
                boundsControl.RotateStopped.AddListener(() => SaveTransform(annotationToggleObject));
            }

            var minMaxScaleConstraint = GetComponent<MinMaxScaleConstraint>();
            if (!minMaxScaleConstraint)
            {
                minMaxScaleConstraint = gameObject.AddComponent<MinMaxScaleConstraint>(); // TODO: looks useless
            }

            if (boundsControl != null && boundingRotationType != BoundingRotationType.ALL)
            {
                OnlyRotateAround(boundsControl, boundingRotationType);
            }

            boundsControl.CalculationMethod = boundsCalculationMethod;

            if (AddManipulator && !GetComponent<ObjectManipulator>())
            {
                StartCoroutine(ManipulationEvents(annotationToggleObject));
            }

            if (CustomScaleHandlesConfiguration != null)
            {
                boundsControl.ScaleHandlesConfig = CustomScaleHandlesConfiguration;
            }

            if (CustomRotationHandlesConfiguration != null)
            {
                boundsControl.RotationHandlesConfig = CustomRotationHandlesConfiguration;
            }

            await Task.Yield();
        }

        public void OnlyRotateAround(BoundsControl boundsControl, BoundingRotationType boundingRotationType)
        {
            boundsControl.RotationHandlesConfig.ShowHandleForX = false;
            boundsControl.RotationHandlesConfig.ShowHandleForY = false;
            boundsControl.RotationHandlesConfig.ShowHandleForZ = false;

            switch (boundingRotationType)
            {
                case BoundingRotationType.X:
                    boundsControl.RotationHandlesConfig.ShowHandleForX = true;
                    break;
                case BoundingRotationType.Y:
                    boundsControl.RotationHandlesConfig.ShowHandleForY = true;
                    break;
                case BoundingRotationType.Z:
                    boundsControl.RotationHandlesConfig.ShowHandleForZ = true;
                    break;
            }
        }

        private void EditModeState(bool editMode)
        {
            if (!manualEditModeHandling)
            {
                var boundsControl = GetComponent<BoundsControl>();
                var objectManipulator = GetComponent<ObjectManipulator>();

                if (boundsControl)
                {
                    if (_isLocked)
                    {
                        boundsControl.enabled = false;
                    }
                    else
                    {
                        boundsControl.enabled = editMode;
                    }
                }

                if (objectManipulator)
                {
                    if (_isLocked)
                    {
                        objectManipulator.enabled = false;
                    }
                    else
                    {
                        objectManipulator.enabled = editMode;
                    }
                }
            }
        }

        /// <summary>
        /// Add the event when every thing is parsed
        /// </summary>
        private IEnumerator ManipulationEvents(LearningExperienceEngine.ToggleObject annotation)
        {
            yield return new WaitWhile(() => transform.parent == null);

            // Disable the parent manipulator and use mine
            var parentManipulator = transform.parent.gameObject.GetComponent<ObjectManipulator>();
            if (parentManipulator)
            {
                parentManipulator.enabled = false;
            }

            var objectManipulator = gameObject.AddComponent<ObjectManipulator>();
            objectManipulator.HostTransform = transform;
            objectManipulator.TwoHandedManipulationType = Microsoft.MixedReality.Toolkit.Utilities.TransformFlags.Move;

            var gridManager = RootObject.Instance.GridManager;
            objectManipulator.OnManipulationStarted.AddListener(eventData => gridManager.onManipulationStarted(eventData.ManipulationSource));
            objectManipulator.OnManipulationEnded.AddListener(eventData => OnManipulationEnded(eventData, annotation));

            var boundsControl = GetComponent<BoundsControl>();
            if (boundsControl)
            {
                boundsControl.RotateStarted.AddListener(() => gridManager.onRotateStarted?.Invoke(boundsControl.Target));
                boundsControl.RotateStopped.AddListener(OnTranslateStopped);
                boundsControl.ScaleStarted.AddListener(() => gridManager.onScaleStarted?.Invoke(boundsControl.Target));
                boundsControl.ScaleStopped.AddListener(OnTranslateStopped);
                boundsControl.TranslateStarted.AddListener(() => gridManager.onTranslateStarted?.Invoke(boundsControl.Target));
                boundsControl.TranslateStopped.AddListener(OnTranslateStopped);
            }

            EditModeState(LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.EditModeActive);
        }

        private void OnManipulationEnded(ManipulationEventData eventData, LearningExperienceEngine.ToggleObject annotation)
        {
            var gridManager = RootObject.Instance.GridManager;
            gridManager.onManipulationEnded(eventData.ManipulationSource);

            SaveTransform(annotation);

            LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.SaveData();
        }

        private void OnTranslateStopped()
        {
            var boundsControl = GetComponent<BoundsControl>();
            var gridManager = RootObject.Instance.GridManager;
            gridManager.onTranslateStopped?.Invoke(boundsControl.Target);

            LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.SaveData();
        }

        private void SaveTransform(LearningExperienceEngine.ToggleObject annotation)
        {
            annotation.position = transform.localPosition.ToString();
            annotation.rotation = transform.localRotation.ToString();
            annotation.scale = transform.localScale.x;
        }
    }
}
