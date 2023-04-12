using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#if !(UNITY_ANDROID || UNITY_IOS)
using UnityEngine.Events;
using Vuforia;
#endif

namespace MirageXR
{
    public class ImageMarkerController : MirageXRPrefab
    {
        private string ImgMName;
        private ToggleObject _obj;
        private Texture2D _ImageMarkerImage;
        private Detectable detectable;
        private GameObject detectableOB;



#if UNITY_ANDROID || UNITY_IOS
        [SerializeField] private XRReferenceImageLibrary serializedLibrary;
        private ARTrackedImageManager trackImageManager;
#else
        private GameObject IM;
        private ObjectTracker objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        private TrackableBehaviour trackableBehaviour;
#endif

        private void Awake()
        {

#if UNITY_ANDROID || UNITY_IOS
            GameObject tracker = GameObject.Find("MixedRealityPlayspace");
            trackImageManager = tracker.GetComponent<ARTrackedImageManager>() ?
                tracker.GetComponent<ARTrackedImageManager>() :
                tracker.AddComponent<ARTrackedImageManager>();

            trackImageManager.referenceLibrary = trackImageManager.CreateRuntimeLibrary(serializedLibrary);
            trackImageManager.maxNumberOfMovingImages = 1;
            trackImageManager.enabled = true;
            trackImageManager.trackedImagesChanged += OnTrackedImagesChanged;
#endif
        }

        private void Start()
        {
            var workplaceManager = RootObject.Instance.workplaceManager;
            detectable = workplaceManager.GetDetectable(workplaceManager.GetPlaceFromTaskStationId(_obj.id));
            detectableOB = GameObject.Find(detectable.id);
#if UNITY_ANDROID || UNITY_IOS
            trackImageManager.trackedImagePrefab = detectableOB;
#endif
        }

        public override bool Init(ToggleObject obj)
        {
            _obj = obj;

            Debug.Log("Object ID: " + _obj.id);

            // Check that url is not empty.
            if (string.IsNullOrEmpty(_obj.url))
            {
                Debug.Log("Content URL not provided.");
                return false;
            }

            // Try to set the parent and if it fails, terminate initialization.
            if (!SetParent(_obj))
            {
                Debug.Log("Couldn't set the parent.");
                return false;
            }

            // Get the last bit of the url.
            var id = _obj.url.Split('/')[_obj.url.Split('/').Length - 1];

            // Rename with the predicate + id to get unique name.
            name = _obj.predicate + "_" + id;

            // Load from resources.
            if (_obj.url.StartsWith("resources://"))
            {
                // Set image url.
                ImgMName = _obj.url.Replace("resources://", "");
            }

            // Load from external url.
            else
            {
                // Set image url.
                ImgMName = _obj.url;
            }


            // Set scaling if defined in action configuration.
            PoiEditor myPoiEditor = transform.parent.gameObject.GetComponent<PoiEditor>();
            Vector3 defaultScale = new Vector3(0.5f, 0.5f, 0.5f);
            transform.parent.localScale = GetPoiScale(myPoiEditor, defaultScale);

            if (!GameObject.Find(ImgMName))
            {
                StartCoroutine(nameof(LoadImage));
            }
            else
            {
                detectableAsChild();

            }

            return base.Init(_obj);
        }

        private IEnumerator LoadImage()
        {
            byte[] byteArray = File.ReadAllBytes(Path.Combine(RootObject.Instance.activityManager.ActivityPath, ImgMName));
            // Find and load the image to be used for createing an image marker

            Texture2D loadTexture = new Texture2D(2, 2);
            // the size of the texture will be replaced by image size

            bool isLoaded = loadTexture.LoadImage(byteArray);
            // convert loaded Byte array into a Texture2D

            yield return isLoaded;

            if (isLoaded)
            {
                _ImageMarkerImage = loadTexture;
#if UNITY_ANDROID || UNITY_IOS

                MutableRuntimeReferenceImageLibrary mutableRuntimeReferenceImageLibrary = trackImageManager.referenceLibrary as MutableRuntimeReferenceImageLibrary;

                var jobHandle = mutableRuntimeReferenceImageLibrary.ScheduleAddImageJob(loadTexture, ImgMName, _obj.scale);

#else

                VuforiaARController.Instance.RegisterVuforiaStartedCallback(HoloLensCreateImageTargetFromImageFile);
                // calls the method to create an image marker using Vuforia for non-mobile builds
#endif
            }
            else
            {
                // debugLog.text += "Failed to load image";
                Debug.Log("Failed to load image");
            }
        }

#if UNITY_ANDROID || UNITY_IOS
        void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
        {
            foreach (ARTrackedImage trackedImage in eventArgs.added)
            {
                trackedImage.transform.Rotate(Vector3.up, 180);
            }

            foreach (ARTrackedImage trackedImage in eventArgs.updated)
            {
                trackedImage.transform.Rotate(Vector3.up, 180);
            }
        }

        void OnDisable()
        {
            trackImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }
#else
        private void HoloLensCreateImageTargetFromImageFile()
        {

            objectTracker.Start();

            Debug.Log("is tracker active = " + objectTracker.IsActive);


            var runtimeImageSource = objectTracker.RuntimeImageSource;
            bool result = runtimeImageSource.SetImage(_ImageMarkerImage, _obj.scale, ImgMName);
            // get the runtime image source and set the texture to load

            Debug.Log("Result: " + result);

            var dataset = objectTracker.CreateDataSet();

            if (result)
            {
                trackableBehaviour = dataset.CreateTrackable(runtimeImageSource, ImgMName);
                // use dataset and use the source to create a new trackable image target called ImageTarget

                Debug.Log(trackableBehaviour.name);
                IM = trackableBehaviour.gameObject;
                IM.AddComponent<TrackableEventHandlerEvents>();
                // IM.AddComponent<DefaultTrackableEventHandler>();
                // add the DefaultTrackableEventHandler to the newly created game object

                GameObject detectableParentObj = GameObject.Find("Detectables");
                IM.transform.parent = detectableParentObj.transform;
                detectableAsChild();
                // move the Image marker to be a child of the Detectables object in the player scene and set the current detectable to be a child of the newly created Image marker
            }

            objectTracker.ActivateDataSet(dataset);

        }
#endif

        public void detectableAsChild()
        {
            //IM.GetComponent<TrackableEventHandlerEvents>().augmentation = GameObject.Find(detectable.id); ;


            var workplaceManager = RootObject.Instance.workplaceManager;
            Detectable detectable = workplaceManager.GetDetectable(workplaceManager.GetPlaceFromTaskStationId(_obj.id));

            Debug.Log("Detecable ID: " + detectable.id);

            GameObject augmentation = GameObject.Find(detectable.id);

            augmentation.transform.parent = GameObject.Find(ImgMName).transform;

            augmentation.transform.localPosition = new Vector3(0, 0.1f, 0);

        }

        public void PlatformOnDestroy()
        {
#if UNITY_ANDROID || UNITY_IOS
            trackImageManager.referenceLibrary = trackImageManager.CreateRuntimeLibrary(serializedLibrary);
            Destroy(gameObject);
#else
            // Get the last bit of the url.
            Detectable detectable = RootObject.Instance.workplaceManager.GetDetectable(RootObject.Instance.workplaceManager.GetPlaceFromTaskStationId(_obj.id));

            GameObject detectableObj = GameObject.Find(detectable.id);
            GameObject detectableParentObj = GameObject.Find("Detectables");

            // as Vuforia doesn't allow image markers to be destroyed at run time the detectable is moved instead leaving the marker still in the scene but removeing its content
            detectableObj.transform.parent = detectableParentObj.transform;

#endif
        }


        public override void Delete()
        {
            // changed Delete to a virtual method so I could overide it for Image markers as they were being deleted twice when changing activities causeing the new activity not to load
        }
    }
}

#if !(UNITY_ANDROID || UNITY_IOS)
[RequireComponent(typeof(TrackableBehaviour))]
public class TrackableEventHandlerEvents : MonoBehaviour
{
    [SerializeField] private TrackableBehaviour _trackableBehaviour;

    public UnityEvent onTrackingFound;
    public UnityEvent onTrackingLost;

    public GameObject augmentation;
    private bool tracked;

    private void Awake()
    {
        if (!_trackableBehaviour) _trackableBehaviour = GetComponent<TrackableBehaviour>();

        if (!_trackableBehaviour)
        {
            Debug.LogError($"This component requires a {nameof(TrackableBehaviour)} !", this);
            return;
        }

        _trackableBehaviour.RegisterOnTrackableStatusChanged(OnTrackableStateChanged);
        tracked = false;
    }

    /// <summary>
    /// called when the tracking state changes.
    /// </summary>
    private void OnTrackableStateChanged(TrackableBehaviour.StatusChangeResult status) // , TrackableBehaviour.Status newStatus)
    {

        switch (status.NewStatus)
        {
            case TrackableBehaviour.Status.DETECTED:
            case TrackableBehaviour.Status.TRACKED:
            case TrackableBehaviour.Status.EXTENDED_TRACKED:
                OnTrackingFound();
                break;

            default:
                OnTrackingLost();
                break;
        }
    }

    protected virtual void OnTrackingFound()
    {
        Debug.Log("Trackable " + _trackableBehaviour.TrackableName + " found");
        // onTrackingFound.Invoke();
        // augmentation.transform.position = _trackableBehaviour.transform.position;//new Vector3(0, 0, 0);
        tracked = true;
    }

    protected virtual void OnTrackingLost()
    {
        Debug.Log("Trackable " + _trackableBehaviour.TrackableName + " lost");
        // onTrackingLost.Invoke();
        tracked = false;
    }
}
#endif
