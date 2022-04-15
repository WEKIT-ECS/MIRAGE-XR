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
            get;set;
        }
        public RotationHandlesConfiguration CustomRotationHandlesConfiguration
        {
            get;set;
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
            EditModeState(ActivityManager.Instance.EditModeActive);
        }

        public async Task<BoundsControl> AddBoundingBox(ToggleObject annotationToggleObject, BoundsCalculationMethod boundsCalculationMethod, bool hasConstraintManager = false, bool addListeners = true, BoundingRotationType boundingRotationType = BoundingRotationType.ALL, bool AddManipulator = false)
        {

            if(!hasConstraintManager && !GetComponent<ConstraintManager>())
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
                boundsControl.ScaleStopped.AddListener(delegate { annotationToggleObject.scale = transform.localScale.x; });
                boundsControl.RotateStopped.AddListener(delegate { annotationToggleObject.rotation = transform.localRotation.ToString(); });
            }

            var minMaxScaleConstraint = GetComponent<MinMaxScaleConstraint>();
            if(!minMaxScaleConstraint)
                 minMaxScaleConstraint = gameObject.AddComponent<MinMaxScaleConstraint>();

            if (boundsControl != null && boundingRotationType != BoundingRotationType.ALL)
                OnlyRotateAround(boundsControl, boundingRotationType);

            boundsControl.CalculationMethod = boundsCalculationMethod;

            if(AddManipulator && !GetComponent<ObjectManipulator>())
            {
                var objectManipulator = gameObject.AddComponent<ObjectManipulator>();
                objectManipulator.HostTransform = transform;
                objectManipulator.TwoHandedManipulationType = Microsoft.MixedReality.Toolkit.Utilities.TransformFlags.Move;
            }

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
    }

}
