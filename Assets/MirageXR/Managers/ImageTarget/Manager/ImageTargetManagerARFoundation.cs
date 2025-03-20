using System;
using System.Threading;
using System.Threading.Tasks;
using MirageXR;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTargetManagerARFoundation : ImageTargetManagerBase
{
    private const int DELAY = 500;

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

    public override async Task<bool> ResetAsync()
    {
        foreach (var arTrackedImage in _arTrackedImageManager.trackables)
        {
            Destroy(arTrackedImage.gameObject);
        }

        Destroy(_arTrackedImageManager);

        foreach (var pair in _images)
        {
            Destroy(pair.Value.gameObject);
        }

        _images.Clear();

        _onTargetCreated.RemoveAllListeners();
        _onTargetFound.RemoveAllListeners();
        _onTargetLost.RemoveAllListeners();

        await Task.Yield();

        await InitializationAsync();

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
        var mainCamera = RootObject.Instance.BaseCamera;
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

        var arSession = MirageXR.Utilities.FindOrCreateComponent<ARSession>();
        var arInputManager = MirageXR.Utilities.FindOrCreateComponent<ARInputManager>(arSession.gameObject);

        await Task.Yield();

        //var arSessionOrigin = MirageXR.Utilities.FindOrCreateComponent<ARSessionOrigin>(cameraParent.gameObject);
        var arSessionOrigin = MirageXR.Utilities.FindOrCreateComponent<XROrigin>(cameraParent.gameObject);
        _arTrackedImageManager = MirageXR.Utilities.FindOrCreateComponent<ARTrackedImageManager>(cameraParent.gameObject);

        // arSessionOrigin.Camera = mainCamera;
        arSessionOrigin.Camera = mainCamera;
        await Task.Yield();

        var arCameraManager = MirageXR.Utilities.FindOrCreateComponent<ARCameraManager>(mainCamera.gameObject);
        var arCameraBackground = MirageXR.Utilities.FindOrCreateComponent<ARCameraBackground>(mainCamera.gameObject);
        //var arPoseDriver = MirageXR.Utilities.FindOrCreateComponent<ARPoseDriver>(mainCamera.gameObject);
        //var arPoseDriver = MirageXR.Utilities.FindOrCreateComponent<TrackedPoseDriver>(mainCamera.gameObject);

        //await Task.DelayInMilliseconds(DELAY);

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
