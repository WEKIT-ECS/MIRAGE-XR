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


    private Dictionary<string,ToggleObject> spawnedObjects;

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

        spawnedObjects = new Dictionary<string, ToggleObject>();
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
        EventManager.OnActivateAction += OnStepChanged;
        EventManager.OnAugmentationDeleted += OnDeleted;
    }


    private void OnDisable()
    {
        trackImageManager.trackedImagesChanged -= OnImageChanged;
        EventManager.OnActivateAction -= OnStepChanged;
        EventManager.OnAugmentationDeleted -= OnDeleted;
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
            ToggleObject erobsonToggleObject = null;
            if (!spawnedObjects.ContainsKey(name))
            {
                erobsonToggleObject = SpawnItem(trackedImage);
                spawnedObjects.Add(name, erobsonToggleObject);
            }
            else
            {
                erobsonToggleObject = spawnedObjects[name];
            }

            var eRobsonGameObject = GameObject.Find(erobsonToggleObject.poi);

            if(eRobsonGameObject == null)
            {
                return;
            }

            if (Vector3.Distance(trackedImage.transform.position, eRobsonGameObject.transform.position) > 0.04f)
            {
                var eRobsonItem = eRobsonGameObject.GetComponentInChildren<eROBSONItems>();
                var ports = eRobsonItem.Ports; //The number of ports are usally only 2
                foreach (var port in ports)
                {
                    if (port.Connected)
                    {
                        return;
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

        // Remove digits frem the marker name (eg. i3button1 -> i3button)
        char[] digits = { '1', '2', '3', '4', '5' };
        var markerNameWithoutDigits = markerName.TrimEnd(digits);
        var activeAction = RootObject.Instance.activityManager.ActiveAction;
        var eRobsonToggleObject = RootObject.Instance.augmentationManager.AddAugmentation(activeAction, trackedImage.transform.position);
        eRobsonToggleObject.predicate = $"eRobson:{markerNameWithoutDigits}";
        eRobsonToggleObject.option = markerName;
        EventManager.ActivateObject(eRobsonToggleObject);
        EventManager.NotifyActionModified(activeAction);

        return eRobsonToggleObject;
    }


    private void OnStepChanged(string action) {
        spawnedObjects.Clear();
    }


    private void OnDeleted(ToggleObject poi)
    {
        spawnedObjects.Remove(poi.option);
    }

#else
        Debug.Log("Image detection of eRobson augmentation works only on Android and iOS.");
#endif

}
