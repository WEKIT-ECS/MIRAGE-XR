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


    private Dictionary<ARTrackedImage,GameObject> spawnedObjects;


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

        spawnedObjects = new Dictionary<ARTrackedImage, GameObject>();
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

        foreach (var trackedImage in eventArgs.removed)
        {
            spawnedObjects.Remove(trackedImage);
            Destroy(trackedImage.gameObject);
        }
    }


    private async void UpdateDetectedObject(ARTrackedImage trackedImage)
    {
        var name = trackedImage.referenceImage.name;

        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            if (!spawnedObjects.ContainsKey(trackedImage))
            {
                var erobsonObject = await SpawnItem(trackedImage);
                spawnedObjects.Add(trackedImage, erobsonObject);
            }

            var eRobsonItem = spawnedObjects.Values.ToList().Find(o => o.name == trackedImage.referenceImage.name);

            eRobsonItem.transform.position = trackedImage.transform.position;
            eRobsonItem.transform.rotation = trackedImage.transform.rotation/* * Quaternion.Euler(90, 0, 0)*/;

        }
    }


    private async Task<GameObject> SpawnItem(ARTrackedImage trackedImage)
    {
        var markerName = trackedImage.referenceImage.name;
        // Get the prefab from the references
        var markerPrefab = await ReferenceLoader.GetAssetReferenceAsync<GameObject>($"eROBSON/Prefabs/{markerName}");
        // if the prefab reference has been found successfully
        if (markerPrefab != null)
        {
            var erobsonItem = Instantiate(markerPrefab, trackedImage.transform.position, trackedImage.transform.rotation);
            erobsonItem.name = markerName;
            return erobsonItem;
        }

        return null;
    }

#else
        Debug.Log("Image detection of eRobson augmentation works only on Android and iOS.");
#endif

}
