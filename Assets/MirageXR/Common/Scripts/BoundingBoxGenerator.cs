using Microsoft.MixedReality.Toolkit.Experimental.Physics;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.UI.BoundsControlTypes;
using System.Collections;
using System.Collections.Generic;
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
            EventManager.OnEditModeChanged += EditModeState;
        }


        private void OnDisable()
        {
            EventManager.OnEditModeChanged -= EditModeState;
        }


        private void Start()
        {
            EditModeState(RootObject.Instance.activityManager.EditModeActive);

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
        /// <returns></returns>
        public async Task<BoundsControl> AddBoundingBox(ToggleObject annotationToggleObject, BoundsCalculationMethod boundsCalculationMethod, bool hasConstraintManager = false, bool addListeners = true, BoundingRotationType boundingRotationType = BoundingRotationType.ALL, bool AddManipulator = false)
        {

            if (!hasConstraintManager && !GetComponent<ConstraintManager>())
                gameObject.AddComponent<ConstraintManager>();

            var elasticManager = GetComponent<ElasticsManager>();
            if (!elasticManager)
                elasticManager = gameObject.AddComponent<ElasticsManager>();

            var boundsControl = gameObject.GetComponent<BoundsControl>();
            if(!boundsControl)
                 boundsControl = gameObject.AddComponent<BoundsControl>();

            boundsControl.Target = gameObject;
            boundsControl.ElasticsManager = elasticManager;

            //if the transform changes should be apply to the target object, otherwise it should be added manually
            if (addListeners)
            {
                boundsControl.ScaleStopped.AddListener(delegate { SaveTransform(annotationToggleObject); });
                boundsControl.RotateStopped.AddListener(delegate { SaveTransform(annotationToggleObject); });
            }

            var minMaxScaleConstraint = GetComponent<MinMaxScaleConstraint>();
            if(!minMaxScaleConstraint)
                 minMaxScaleConstraint = gameObject.AddComponent<MinMaxScaleConstraint>();

            if (boundsControl != null && boundingRotationType != BoundingRotationType.ALL)
                OnlyRotateAround(boundsControl, boundingRotationType);

            boundsControl.CalculationMethod = boundsCalculationMethod;

            if(AddManipulator && !GetComponent<ObjectManipulator>())
                StartCoroutine(ManipulationEvents(annotationToggleObject));


            if(CustomScaleHandlesConfiguration != null)
                boundsControl.ScaleHandlesConfig = CustomScaleHandlesConfiguration;

            if (CustomRotationHandlesConfiguration != null)
                boundsControl.RotationHandlesConfig = CustomRotationHandlesConfiguration;

            await Task.Delay(1);

            return boundsControl;
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
                default:
                    break;
            }

        }

     
        void EditModeState(bool editmode)
        {
            if(!manualEditModeHandling)
                GetComponent<BoundsControl>().Active = editmode;
        }

        /// <summary>
        /// Add the event when every thing is parsed
        /// </summary>
        IEnumerator ManipulationEvents(ToggleObject annotaion)
        {
            yield return new WaitForSeconds(1);

            //Disable the parent manipulator and use mine
            var parentManipulator = transform.parent.gameObject.GetComponent<ObjectManipulator>();
            if (parentManipulator)
                parentManipulator.enabled = false;


            var objectManipulator = gameObject.AddComponent<ObjectManipulator>();
            objectManipulator.HostTransform = transform;
            objectManipulator.TwoHandedManipulationType = Microsoft.MixedReality.Toolkit.Utilities.TransformFlags.Move;

            objectManipulator.OnManipulationEnded.AddListener(delegate { SaveTransform(annotaion);  });

        }


        private void SaveTransform(ToggleObject annotaion)
        {
            annotaion.position = transform.localPosition.ToString();
            annotaion.rotation = transform.localRotation.ToString();
            annotaion.scale =  transform.localScale.x;
        }
    }

}
