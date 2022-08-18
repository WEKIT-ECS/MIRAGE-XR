using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using MirageXR;
using System;


public class ERobsonImageMarkerController : MonoBehaviour
{
#if UNITY_ANDROID || UNITY_IOS
    [SerializeField] private XRReferenceImageLibrary _imageLibrary;

    private ARTrackedImageManager trackImageManager;

    private ObjectFactory _objectFactory;

    private void Awake()
    {
        GameObject tracker = GameObject.Find("MixedRealityPlayspace");
        if (tracker.GetComponent<ARTrackedImageManager>())
        {
            trackImageManager = tracker.GetComponent<ARTrackedImageManager>();
        }
        else
        {
            trackImageManager = tracker.AddComponent<ARTrackedImageManager>();
        }

        trackImageManager.referenceLibrary = trackImageManager.CreateRuntimeLibrary(_imageLibrary);
        trackImageManager.requestedMaxNumberOfMovingImages = 4;
        trackImageManager.enabled = true;

    }

    IEnumerator Start()
    {
        while (_objectFactory == null)
        {
            _objectFactory = FindObjectOfType<ObjectFactory>();
            yield return null;
        }
        
    }

    private void OnEnable()
    {
        trackImageManager.trackedImagesChanged += OnImageChanged;
    }


    private void OnDisable()
    {
        trackImageManager.trackedImagesChanged -= OnImageChanged;
    }

    private void OnImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {

        foreach (var trackedImage in eventArgs.updated)
        {
            UpdateDetectedObject(trackedImage);
        }

    }


    private void UpdateDetectedObject(ARTrackedImage trackedImage)
    {
        var name = trackedImage.referenceImage.name;

        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            ToggleObject erobsonToggleObject = RootObject.Instance.activityManager.ActiveAction.enter.activates.Find(t => t.option == name && t.sensor == "MarkerDetection");
            if (erobsonToggleObject == null)
            {
                erobsonToggleObject = SpawnItem(trackedImage);
            }

            Debug.LogError(erobsonToggleObject.option);
            Debug.LogError(erobsonToggleObject.sensor);
            Debug.LogError(erobsonToggleObject.predicate);

            var eRobsonGameObject = GameObject.Find(erobsonToggleObject.poi);

            if(eRobsonGameObject == null)
            {
                return;
            }

            if (Vector3.Distance(trackedImage.transform.position, eRobsonGameObject.transform.position) > 0.04f)
            {
                var eRobsonItem = eRobsonGameObject.GetComponentInChildren<eROBSONItems>();

                if (eRobsonItem)
                {
                    var ports = eRobsonItem.Ports; //The number of ports are usally only 2
                    foreach (var port in ports)
                    {
                        if (port.Connected)
                        {
                            return;
                        }
                    }
                }
            }


            eRobsonGameObject.transform.position = trackedImage.transform.position;
            eRobsonGameObject.transform.rotation = trackedImage.transform.rotation * Quaternion.Euler(0, 90, 0);

        }
    }


    private ToggleObject SpawnItem(ARTrackedImage trackedImage)
    {
        var markerName = trackedImage.referenceImage.name;

        var activeAction = RootObject.Instance.activityManager.ActiveAction;
        var eRobsonToggleObject = RootObject.Instance.augmentationManager.AddAugmentation(activeAction, trackedImage.transform.position);

        eRobsonToggleObject.option = trackedImage.referenceImage.name;
        eRobsonToggleObject.sensor = "MarkerDetection";

        // Remove digits frem the marker name (eg. i3button1 -> i3button)
        char[] digits = { '1', '2', '3', '4', '5' };
        var markerNameWithoutDigits = markerName.TrimEnd(digits);
        eRobsonToggleObject.predicate = $"eRobson:{markerNameWithoutDigits}";

        EventManager.ActivateObject(eRobsonToggleObject);
        EventManager.NotifyActionModified(activeAction);

        return eRobsonToggleObject;
    }

#else
        Debug.Log("Image detection of eRobson augmentation works only on Android and iOS.");
#endif

}
