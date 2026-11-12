using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using LearningExperienceEngine;
using Utilities;

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

        private void OnEnable()
        {
            LearningExperienceEngine.EventManager.OnEditModeChanged += EditModeState;
        }

        private void OnDisable()
        {
            LearningExperienceEngine.EventManager.OnEditModeChanged -= EditModeState;
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
            BoundsUtilities.BoundsCalculationMethod boundsCalculationMethod = BoundsUtilities.BoundsCalculationMethod.RendererOverCollider,
            bool hasConstraintManager = false,
            bool addListeners = true,
            BoundingRotationType boundingRotationType = BoundingRotationType.ALL,
            bool AddManipulator = false)
        {
            _isLocked = annotationToggleObject.positionLock;

            var simpleBoundingBox = gameObject.GetComponent<SimpleBoundingBox>();
            if (!simpleBoundingBox)
            {
                simpleBoundingBox = gameObject.AddComponent<SimpleBoundingBox>();
            }

            simpleBoundingBox.Target = transform;
            simpleBoundingBox.Setup(annotationToggleObject);

            if (addListeners)
            {
                simpleBoundingBox.ScaleStopped.AddListener(() => SaveTransform(annotationToggleObject));
                simpleBoundingBox.RotateStopped.AddListener(() => SaveTransform(annotationToggleObject));
                simpleBoundingBox.TranslateStopped.AddListener(() => SaveTransform(annotationToggleObject));
            }

            if (boundingRotationType != BoundingRotationType.ALL)
            {
                OnlyRotateAround(simpleBoundingBox, boundingRotationType);
            }

            var gridManager = RootObject.Instance ? RootObject.Instance.GridManager : null;
            if (gridManager != null)
            {
                simpleBoundingBox.RotateStarted.AddListener(() => gridManager.onRotateStarted?.Invoke(simpleBoundingBox.Target.gameObject));
                simpleBoundingBox.RotateStopped.AddListener(() => OnTranslateStopped(simpleBoundingBox));
                simpleBoundingBox.ScaleStarted.AddListener(() => gridManager.onScaleStarted?.Invoke(simpleBoundingBox.Target.gameObject));
                simpleBoundingBox.ScaleStopped.AddListener(() => OnTranslateStopped(simpleBoundingBox));
                simpleBoundingBox.TranslateStarted.AddListener(() => gridManager.onTranslateStarted?.Invoke(simpleBoundingBox.Target.gameObject));
                simpleBoundingBox.TranslateStopped.AddListener(() => OnTranslateStopped(simpleBoundingBox));
            }

            EditModeState(LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.EditModeActive);

            await Task.Yield();
        }

        public void OnlyRotateAround(SimpleBoundingBox boundingBox, BoundingRotationType boundingRotationType)
        {
            if (boundingBox == null) return;
            boundingBox.OnlyRotateAround(boundingRotationType);
        }

        private void EditModeState(bool editMode)
        {
            if (!manualEditModeHandling)
            {
                var simpleBoundingBox = GetComponent<SimpleBoundingBox>();
                if (simpleBoundingBox)
                {
                    simpleBoundingBox.SetEnabled(!_isLocked && editMode);
                }
            }
        }

        private void OnTranslateStopped(SimpleBoundingBox boundingBox)
        {
            var gridManager = RootObject.Instance ? RootObject.Instance.GridManager : null;
            if (gridManager != null && boundingBox != null)
            {
                gridManager.onTranslateStopped?.Invoke(boundingBox.Target ? boundingBox.Target.gameObject : null);
            }

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
