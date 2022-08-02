using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using MirageXR;
using System;
using System.Linq;
using System.Threading.Tasks;

public class ERobsonImageMarkerController : MonoBehaviour
{
#if UNITY_ANDROID || UNITY_IOS
    [SerializeField] private XRReferenceImageLibrary _imageLibrary;

    private ARTrackedImageManager trackImageManager;


    private Dictionary<string,GameObject> spawnedObjects;


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

        spawnedObjects = new Dictionary<string, GameObject>();
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

        //foreach (var trackedImage in eventArgs.removed)
        //{
        //    spawnedObjects.Remove(trackedImage);
        //    Destroy(trackedImage.gameObject);
        //}
    }


    private async void UpdateDetectedObject(ARTrackedImage trackedImage)
    {
        var name = trackedImage.referenceImage.name;

        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            if (!spawnedObjects.ContainsKey(name))
            {
                var erobsonObject = await SpawnItem(trackedImage);
                spawnedObjects.Add(name, erobsonObject);
            }

            var eRobsonItem = spawnedObjects.Values.ToList().Find(o => o.name == name);

            eRobsonItem.transform.position = trackedImage.transform.position;
            eRobsonItem.transform.rotation = trackedImage.transform.rotation * Quaternion.Euler(0, 90, 0);

        }
    }


    private async Task<GameObject> SpawnItem(ARTrackedImage trackedImage)
    {
        var markerName = trackedImage.referenceImage.name;

        //Remove digits frem the marker name (eg. i3button1 -> i3button)
        char[] digits = { '1', '2', '3', '4', '5' };
        var MarkerNameWithoutDigits = markerName.TrimEnd(digits);

        // Get the prefab from the references
        var markerPrefab = await ReferenceLoader.GetAssetReferenceAsync<GameObject>($"eROBSON/Prefabs/{MarkerNameWithoutDigits}");
        // if the prefab reference has been found successfully
        if (markerPrefab != null)
        {
            var erobsonItem = Instantiate(markerPrefab, trackedImage.transform.position, trackedImage.transform.rotation);
            erobsonItem.name = markerName;
            erobsonItem.transform.SetParent(null);
            return erobsonItem;
        }

        return null;
    }

#else
        Debug.Log("Image detection of eRobson augmentation works only on Android and iOS.");
#endif

}
