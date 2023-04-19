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
    private Dictionary<string, ImageTargetModel> _map = new Dictionary<string, ImageTargetModel>();
    private bool _libraryIsBusy = false;
    private object _syncObject = new object();

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

        var alreadyExists = false;

        if (_map.ContainsKey(imageTargetModel.name))
        {
            if (imageTargetModel.texture2D == _map[imageTargetModel.name].texture2D)
            {
                alreadyExists = true;
            }
            else
            {
                lock (_syncObject)
                {
                    _map.Remove(imageTargetModel.name);
                }
            }
        }

        if (!alreadyExists)
        {
            lock (_syncObject)
            {
                _map.Add(imageTargetModel.name, imageTargetModel);
            }

            while (_libraryIsBusy)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();
            }

            _libraryIsBusy = true;
            var job = mutableLibrary.ScheduleAddImageWithValidationJob(imageTargetModel.texture2D, imageTargetModel.name, imageTargetModel.width);
            while (job.status is AddReferenceImageJobStatus.Pending or AddReferenceImageJobStatus.None)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _libraryIsBusy = false;
                    _map.Remove(imageTargetModel.name);
                    cancellationToken.ThrowIfCancellationRequested();
                }

                await Task.Yield();
            }

            _libraryIsBusy = false;
        }

        lock (_syncObject)
        {
            _arTrackedImageManager.requestedMaxNumberOfMovingImages += 1;
        }

        while (!_images.ContainsKey(imageTargetModel.name))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                lock (_syncObject)
                {
                    _map.Remove(imageTargetModel.name);
                }

                cancellationToken.ThrowIfCancellationRequested();
            }

            await Task.Yield();
        }

        return _images[imageTargetModel.name];
    }

    public override void RemoveImageTarget(ImageTargetBase imageTarget)
    {
        if (!_images.ContainsKey(imageTarget.imageTargetName))
        {
            return;
        }

        if (imageTarget.targetObject)
        {
            Destroy(imageTarget.targetObject);
        }

        _arTrackedImageManager.requestedMaxNumberOfMovingImages -= 1;
        imageTarget.gameObject.SetActive(false);
        Destroy(imageTarget); // right now the api doesn't allow us to delete the object itself
        _map.Remove(imageTarget.imageTargetName);
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
                image.gameObject.SetActive(false);
                return;
            }

            var model = _map[image.referenceImage.name];
            var imageTarget = image.gameObject.AddComponent<ImageTargetARFoundation>();
            _images.Add(image.referenceImage.name, imageTarget);
            imageTarget.Initialization(model);
            imageTarget.onTargetFound.AddListener(value => onTargetFound.Invoke(value));
            imageTarget.onTargetLost.AddListener(value => onTargetLost.Invoke(value));

            onTargetCreated.Invoke(imageTarget);
        }

        foreach (var image in eventArgs.removed)
        {
            var imageTarget = image.GetComponent<ImageTargetARFoundation>();
            _images.Remove(image.referenceImage.name);
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
        if (_isInitialized && _arTrackedImageManager)
        {
            _arTrackedImageManager.enabled = false;
        }
    }
}
