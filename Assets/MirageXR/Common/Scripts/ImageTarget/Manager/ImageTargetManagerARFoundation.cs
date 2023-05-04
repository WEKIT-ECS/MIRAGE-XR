using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTargetManagerARFoundation : ImageTargetManagerBase
{
    private const int DELAY = 1000;

    private ARTrackedImageManager _arTrackedImageManager;
    private bool _libraryIsBusy = false;
    private object _syncObject = new object();
    private Transform _imageTargetHolder;

    public override async Task<bool> InitializationAsync()
    {
        var result = await ARFoundationInitialization();
        if (!result)
        {
            return false;
        }

        _arTrackedImageManager.referenceLibrary ??= _arTrackedImageManager.CreateRuntimeLibrary();
        _arTrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        _arTrackedImageManager.requestedMaxNumberOfMovingImages = 10;
        _arTrackedImageManager.enabled = true;

        _isInitialized = true;

        return true;
    }

    public override async Task<ImageTargetBase> AddImageTarget(ImageTargetModel imageTargetModel, CancellationToken cancellationToken = default)
    {
        if (!_arTrackedImageManager.descriptor.supportsMutableLibrary ||
            _arTrackedImageManager.referenceLibrary is not MutableRuntimeReferenceImageLibrary mutableLibrary)
        {
            throw new Exception("The reference image library is not mutable.");
        }

        while (_libraryIsBusy)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Yield();
        }

        try
        {
            _libraryIsBusy = true;
            var job = mutableLibrary.ScheduleAddImageWithValidationJob(imageTargetModel.texture2D, imageTargetModel.name, imageTargetModel.width);
            while (job.status is AddReferenceImageJobStatus.Pending or AddReferenceImageJobStatus.None)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();
            }
        }
        finally
        {
            _libraryIsBusy = false;
        }

        lock (_syncObject)
        {
            _arTrackedImageManager.requestedMaxNumberOfMovingImages += 1;
        }

        var imageTarget = new GameObject(imageTargetModel.name).AddComponent<ImageTargetARFoundation>();
        imageTarget.transform.SetParent(_imageTargetHolder);

        imageTarget.Initialization(imageTargetModel);
        imageTarget.onTargetFound.AddListener(value => onTargetFound.Invoke(value));
        imageTarget.onTargetLost.AddListener(value => onTargetLost.Invoke(value));

        _images.Add(imageTargetModel.name, imageTarget);
        onTargetCreated.Invoke(imageTarget);

        return imageTarget;
    }

    public override void RemoveImageTarget(ImageTargetBase imageTarget)
    {
        if (!_images.ContainsKey(imageTarget.imageTargetName))
        {
            return;
        }

        lock (_syncObject)
        {
            _arTrackedImageManager.requestedMaxNumberOfMovingImages -= 1;
        }

        Destroy(imageTarget); // right now the api doesn't allow us to delete the object itself
        _images.Remove(imageTarget.imageTargetName);
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

        await Task.Delay(DELAY);

        _imageTargetHolder = new GameObject("ImageTargetHolder").transform;

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
            if (!_images.ContainsKey(image.referenceImage.name))
            {
                return;
            }

            var imageTarget = (ImageTargetARFoundation)_images[image.referenceImage.name];
            imageTarget.SetARTrackedImage(image);
        }

        foreach (var image in eventArgs.updated)
        {
            if (!_images.ContainsKey(image.referenceImage.name))
            {
                return;
            }

            ((ImageTargetARFoundation)_images[image.referenceImage.name]).CopyPose(image);
        }

        foreach (var image in eventArgs.removed)
        {
            if (!_images.ContainsKey(image.referenceImage.name))
            {
                return;
            }

            ((ImageTargetARFoundation)_images[image.referenceImage.name]).RemoveARTrackedImage();
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
        if (_isInitialized && _arTrackedImageManager)
        {
            _arTrackedImageManager.enabled = false;
        }
    }
}
