using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTargetManagerARFoundation : ImageTargetManagerBase
{
    private ARTrackedImageManager _arTrackedImageManager;
    private Dictionary<string, ImageTargetModel> _map = new Dictionary<string, ImageTargetModel>();

    public override async Task<bool> InitializationAsync()
    {
        var result = await ARFoundationInitialization();
        if (!result)
        {
            return false;
        }

        _arTrackedImageManager.referenceLibrary ??= _arTrackedImageManager.CreateRuntimeLibrary();
        _arTrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        _arTrackedImageManager.requestedMaxNumberOfMovingImages = 0;
        _arTrackedImageManager.enabled = true;

        _isInitialized = true;

        return true;
    }

    private async Task<bool> ARFoundationInitialization()
    {
        var mainCamera = Camera.main;
        if (!mainCamera)
        {
            Debug.LogError("Unable to find main camera");
            return false;
        }

        var cameraParent = mainCamera.transform.parent;
        if (!cameraParent)
        {
            Debug.LogWarning("Unable to find main camera's parent");
            cameraParent = mainCamera.transform;
        }

        var arSession = FindObjectOfType<ARSession>();
        if (!arSession)
        {
            var arSessionObject = new GameObject("ARSession", typeof(ARSession), typeof(ARInputManager));
        }

        await Task.Yield();

        _arTrackedImageManager = FindObjectOfType<ARTrackedImageManager>();
        if (!_arTrackedImageManager)
        {
            var arSessionOrigin = cameraParent.gameObject.GetComponent<ARSessionOrigin>();
            if (!arSessionOrigin)
            {
                arSessionOrigin = cameraParent.gameObject.AddComponent<ARSessionOrigin>();
            }

            arSessionOrigin.camera = mainCamera;

            _arTrackedImageManager = cameraParent.gameObject.AddComponent<ARTrackedImageManager>();
        }

        await Task.Yield();

        var arCameraManager = FindObjectOfType<ARCameraManager>();
        if (!arCameraManager)
        {
            arCameraManager = mainCamera.gameObject.AddComponent<ARCameraManager>();
        }

        var arCameraBackground = FindObjectOfType<ARCameraBackground>();
        if (!arCameraBackground)
        {
            arCameraBackground = mainCamera.gameObject.AddComponent<ARCameraBackground>();
        }

        var arPoseDriver = FindObjectOfType<ARPoseDriver>();
        if (!arPoseDriver)
        {
            arPoseDriver = mainCamera.gameObject.AddComponent<ARPoseDriver>();
        }

        return true;
    }

    private void OnDestroy()
    {
        if (_isInitialized && _arTrackedImageManager)
        {
            _arTrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var image in eventArgs.added)
        {
            if (!_map.ContainsKey(image.referenceImage.name))
            {
                return;
            }

            var model = _map[image.referenceImage.name];
            var imageTarget = image.gameObject.AddComponent<ImageTargetARFoundation>();
            _images.Add(imageTarget);
            imageTarget.Initialization(model);
            imageTarget.onTargetFound.AddListener(value => onTargetFound.Invoke(value));
            imageTarget.onTargetLost.AddListener(value => onTargetLost.Invoke(value));

            onTargetCreated.Invoke(imageTarget);
        }

        foreach (var image in eventArgs.removed)
        {
            var imageTarget = image.GetComponent<ImageTargetARFoundation>();
            _images.Remove(imageTarget);
            Destroy(imageTarget);
        }
    }

    protected override void OnEnable()
    {
        if (_isInitialized)
        {
            _arTrackedImageManager.enabled = true;
        }
    }

    protected override void OnDisable()
    {
        if (_isInitialized)
        {
            _arTrackedImageManager.enabled = false;
        }
    }

    public override async Task AddImageTarget(ImageTargetModel imageTargetModel)
    {
        if (_arTrackedImageManager.descriptor.supportsMutableLibrary && _arTrackedImageManager.referenceLibrary is MutableRuntimeReferenceImageLibrary mutableLibrary)
        {
            if (_map.ContainsKey(imageTargetModel.name))
            {
                imageTargetModel.name = $"{imageTargetModel.name}_{Guid.NewGuid()}";
            }

            _map.Add(imageTargetModel.name, imageTargetModel);
            var job = mutableLibrary.ScheduleAddImageWithValidationJob(imageTargetModel.texture2D, imageTargetModel.name, imageTargetModel.width);
            while (job.status is AddReferenceImageJobStatus.Pending or AddReferenceImageJobStatus.None)
            {
                await Task.Yield();
            }

            _arTrackedImageManager.requestedMaxNumberOfMovingImages += 1;
        }
        else
        {
            throw new Exception("The reference image library is not mutable.");
        }
    }
}
