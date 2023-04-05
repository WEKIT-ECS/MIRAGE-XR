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
        //private TrackableBehaviour _trackableComponent;

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

        private void Awake()
        {
            //_trackableComponent = GetComponent<TrackableBehaviour>();
        }

        private void OnEnable()
        {
            EventManager.OnPlayerReset += PlayerReset;
        }

        private void OnDisable()
        {
            EventManager.OnPlayerReset -= PlayerReset;

            //DetachTrackable();
        }

        private void OnDestroy()
        {
            //DetachTrackable();
        }

        private void DetachTrackable()
        {
            //if (_trackableComponent != null)
            //{
            //    TrackerManager.Instance.GetStateManager().DestroyTrackableBehavioursForTrackable(_trackableComponent.Trackable);
            //    TrackerManager.Instance.GetTracker<ObjectTracker>().DeactivateDataSet(Dataset);
            //    Dataset.Destroy(_trackableComponent.Trackable, true);
            //}
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

        private void Delete()
        {
            if (!CompareTag("Permanent"))
            {
                Destroy(gameObject);
            }
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

                    // If the TrackableType is Vuforia image target or marker...
                    case TrackableType.Image:
                    case TrackableType.Marker:
                        // Set is detected state based on the renderer check hack
                        IsDetected = _rendererCheck.enabled;

                        IsWorking = true;

                        // Behave according to set tracking style...
                        switch (Style)
                        {
                            // If normal Vuforia tracking is set...
                            case (TrackingStyle.Raw):
                            default:
                                // ...update the position of the attached object.
                                AttachedObject.transform.localPosition = _origin.position;
                                AttachedObject.transform.localRotation = _origin.rotation;

                                // ... if detectable is visible
                                if (IsDetected)
                                {
                                    // Now we should have a reference of the location in the Hololens space.
                                    if (!IsLocated)
                                    {
                                        // ...which means that we can safely enable the gaze guiding.
                                        IsLocated = true;
                                    }

                                    // ... tell attached object to show content
                                    AttachedObject.SendMessage("ShowContent", SendMessageOptions.DontRequireReceiver);
                                }

                                // ... and if detectable is lost
                                else
                                {
                                    // ... tell attached object to hide content
                                    AttachedObject.SendMessage("HideContent", SendMessageOptions.DontRequireReceiver);
                                }
                                break;

                            // If handheld tracking style is set...
                            case TrackingStyle.Handheld:

                                // If not yet active and if the distance between the detectable position and the attached object position is greater than Tolerance...
                                if (!IsActive && Mathf.Abs(Vector3.Distance(transform.position, AttachedObject.transform.position)) > Tolerance)
                                {
                                    // ...update the position of the attached object.
                                    AttachedObject.transform.localPosition = _origin.position;
                                    AttachedObject.transform.localRotation = _origin.rotation;

                                    // ... tell attached object to show content
                                    AttachedObject.SendMessage("ShowContent", SendMessageOptions.DontRequireReceiver);

                                    // Set active flag
                                    IsActive = true;
                                }

                                // If active and within active area (arms length), update attached object transform if movement Tolerance is exceeded...
                                if (IsActive &&
                                    Mathf.Abs(Vector3.Distance(_userPosition.position, AttachedObject.transform.position)) < Radius &&
                                    Mathf.Abs(Vector3.Distance(transform.position, AttachedObject.transform.position)) > Tolerance)
                                {
                                    // ...update the position of the attached object.
                                    AttachedObject.transform.localPosition = _origin.position;
                                    AttachedObject.transform.localRotation = _origin.rotation;
                                }

                                // If detectable is lost, tell attached object to hide content and set inactive flag
                                if (!IsDetected)
                                {
                                    AttachedObject.SendMessage("HideContent", SendMessageOptions.DontRequireReceiver);
                                    IsActive = false;
                                }

                                break;

                            // If fixed object tracking style is set...
                            case TrackingStyle.Fixed:

                                // Now we should have a reference of the location in the Hololens space.
                                if (!IsLocated)
                                {
                                    IsLocated = true;
                                }

                                // If not yet active and if the distance between the detectable position and the attached object position is greater than Tolerance...
                                if (!IsActive && Mathf.Abs(Vector3.Distance(transform.position, AttachedObject.transform.position)) > Tolerance)
                                {
                                    // ...update the position of the attached object.
                                    AttachedObject.transform.localPosition = _origin.position;
                                    AttachedObject.transform.localRotation = _origin.rotation;

                                    // ... tell attached object to show content
                                    AttachedObject.SendMessage("ShowContent", SendMessageOptions.DontRequireReceiver);

                                    // Set active flag
                                    IsActive = true;
                                }

                                // If user has moved outside the fixed object active area...
                                if (Mathf.Abs(Vector3.Distance(_userPosition.position, AttachedObject.transform.position)) > Radius)
                                {
                                    // ... tell attached object to hide content
                                    AttachedObject.SendMessage("HideContent", SendMessageOptions.DontRequireReceiver);

                                    // Set inactive flag
                                    IsActive = false;
                                }
                                break;
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