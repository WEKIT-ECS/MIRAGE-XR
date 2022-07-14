using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using MirageXR;
using System;

public class ERobsonImageMarkerController : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager _arTrackedImageManager;
    [SerializeField] private XRReferenceImageLibrary _imageLibrary;

    private Dictionary<string,GameObject> eRobsonObjects;

    void Start()
    {

        eRobsonObjects = new Dictionary<string, GameObject>();

        if(_arTrackedImageManager.referenceLibrary == null)
        {
            _arTrackedImageManager.referenceLibrary = _imageLibrary;
        }
    }

    private void OnEnable()
    {
        _arTrackedImageManager.trackedImagesChanged += OnImageChanged;
    }

    private void OnDisable()
    {
        _arTrackedImageManager.trackedImagesChanged -= OnImageChanged;
    }

    private void OnImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
            foreach (var trackedImage in eventArgs.updated)
            {
                UpdateDetectedMenu(trackedImage);
            }

            foreach (var trackedImage in eventArgs.removed)
            {
                Destroy(trackedImage.gameObject);
            }
    }


    private void UpdateDetectedMenu(ARTrackedImage trackedImage)
    {
        var name = trackedImage.referenceImage.name;

        var id = $"{name}{string.Format("erobson-{0:yyyy-MM-dd_HH-mm-ss}", DateTime.UtcNow)}";

        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            var eRobsonItem = eRobsonObjects[name];

            if (!eRobsonItem)
            {
                SpawnItem(name, id);
            }

            eRobsonItem.transform.position = trackedImage.transform.position;
            eRobsonItem.transform.rotation = trackedImage.transform.rotation/* * Quaternion.Euler(90, 0, 0)*/;

            foreach (var eRobsonObject in eRobsonObjects)
            {
                if (eRobsonObject.Key != name)
                {
                    eRobsonObject.Value.SetActive(false);
                }
            }
        }
    }

    private async void SpawnItem(string markerName, string erobsonId)
    {
        // Get the prefab from the references
        var markerPrefab = await ReferenceLoader.GetAssetReferenceAsync<GameObject>(markerName);
        // if the prefab reference has been found successfully
        if (markerPrefab != null)
        {
            var erobsonItem = Instantiate(markerPrefab, Vector3.zero, Quaternion.identity);
            eRobsonObjects.Add(erobsonId, erobsonItem);
        }
    }
}
