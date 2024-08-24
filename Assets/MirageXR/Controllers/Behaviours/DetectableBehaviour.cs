using LearningExperienceEngine;
using UnityEngine;
//using Vuforia;

namespace MirageXR
{
    /// <summary>
    /// Class for defining how a detectable should behave.
    /// </summary>
    public class DetectableBehaviour : MonoBehaviour
    {
        [SerializeField] private bool IsWorking;
        [SerializeField] private bool IsLocated;
        [SerializeField] private bool IsDetected;
        [SerializeField] private bool IsActive;
        [SerializeField] private bool ExtendedTrackingActive = true;
        private Transform _userPosition;
        private Transform _origin;
        private bool _isAttached;
        private Renderer _rendererCheck;
        private Transform _trackable;

        //public DataSet Dataset { get; set; }

        // Is detectable ready for action or not
        public bool IsDetectableReady { get; set; }

        // Definitions of all the supported trackable types
        public enum TrackableType
        {
            Anchor,
            Marker,
            Image
        }

        // Definitions of all the available tracking styles
        public enum TrackingStyle
        {
            Raw,
            Handheld,
            Fixed,
            Extended
        }

        [Tooltip("Trackable TrackableType of this trackable. Set by the workplace file through the WorkplaceParser.")]
        public TrackableType Type = TrackableType.Anchor;

        [Tooltip("Tracking style of this trackable. Set by the workplace file through the WorkplaceParser.")]
        public TrackingStyle Style = TrackingStyle.Raw;

        [Tooltip("The game object that should be moved by this trackable.")]
        public GameObject AttachedObject;

        [Tooltip("Radius for defining fixed and handheld object active area (in meters).")]
        public float Radius = 1.5f;

        [Tooltip("Tolerance for handheld movements (in meters).")]
        public float Tolerance = 0.01f;

        private void OnEnable()
        {
            EventManager.OnPlayerReset += PlayerReset;
        }

        private void OnDisable()
        {
            EventManager.OnPlayerReset -= PlayerReset;
        }

        public void SetTrackable(Transform trackable)
        {
            _trackable = trackable;
            Type = TrackableType.Marker;
        }

        public void RemoveTrackable()
        {
            _trackable = null;
            Type = TrackableType.Anchor;
        }

        private void Start()
        {
            // Hololens camera position is the same as user position.
            _userPosition = GameObject.FindGameObjectWithTag("MainCamera").transform;

            // Attach _origin object
            if (transform.Find("Origin") != null)
            {
                _origin = transform.Find("Origin").transform;
            }

            // Attach the renderer check
            if (transform.Find("RendererCheck") != null)
            {
                _rendererCheck = transform.Find("RendererCheck").GetComponent<Renderer>();
            }

            IsDetectableReady = true;
        }

        private void PlayerReset()
        {
            IsLocated = false;
            IsActive = false;
            IsDetectableReady = false;
            AttachedObject = null;

            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;
        }

        public void AttachAnchor()
        {
            AttachedObject.transform.position = transform.position;
            AttachedObject.transform.rotation = transform.rotation;

            _isAttached = true;
        }

        private void Update()
        {
            // Detectable behaviour can behave only if a tracker is attached...
            if (IsDetectableReady && AttachedObject != null)
            {
                // Behave based on the trackable TrackableType...
                switch (Type)
                {
                    // If the TrackableType is Hololens anchor...
                    case TrackableType.Anchor:
                        AttachedObject.transform.position = transform.position;
                        AttachedObject.transform.rotation = transform.rotation;
                        break;

                    case TrackableType.Image:
                    case TrackableType.Marker:
                        if (_trackable)
                        {
                            AttachedObject.transform.position = _trackable.position;
                            //Adjust for task staions position
                            AttachedObject.transform.position += AttachedObject.transform.position - AttachedObject.transform.FindDeepChild("PlayerTaskStation(Clone)").position;

                            AttachedObject.transform.rotation = Quaternion.AngleAxis(_trackable.rotation.eulerAngles.y, Vector3.up);
                        }

                        break;

                    // If the TrackableType is not supported...
                    default:
                        EventManager.DebugLog("Error: Detectable behaviour: " + name + ": Unknown sensor TrackableType");
                        break;
                }
            }
            else
            {
                IsWorking = false;
            }
        }
    }
}