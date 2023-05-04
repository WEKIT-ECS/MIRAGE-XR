using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Vuforia;

public class ImageTargetManagerVuforia : ImageTargetManagerBase
{
    public override async Task<bool> InitializationAsync()
    {
        var vuforiaRuntime = VuforiaRuntime.Instance;

        if (vuforiaRuntime.InitializationState == VuforiaRuntime.InitState.NOT_INITIALIZED)
        {
            vuforiaRuntime.InitVuforia();
        }

        while (vuforiaRuntime.InitializationState != VuforiaRuntime.InitState.INITIALIZED)
        {
            await Task.Yield();
        }

        var vuforiaBehaviour = FindObjectOfType<VuforiaBehaviour>();
        if (!vuforiaBehaviour)
        {
            var mainCamera = Camera.main;
            if (!mainCamera)
            {
                throw new NullReferenceException("Unable to find main camera");
            }

            mainCamera.gameObject.AddComponent<VuforiaBehaviour>();
            mainCamera.gameObject.AddComponent<DefaultInitializationErrorHandler>();
        }

        var manager = VuforiaManager.Instance;
        while (!manager.Initialized)
        {
            await Task.Yield();
        }

        _isInitialized = true;
        return true;
    }

    public override Task<ImageTargetBase> AddImageTarget(ImageTargetModel imageTargetModel, CancellationToken cancellationToken = default)
    {
        var behaviour = CreateImageTargetFromTexture(imageTargetModel.texture2D, imageTargetModel.width, imageTargetModel.name);
        var imageTarget = behaviour.gameObject.AddComponent<ImageTargetVuforia>();
        imageTarget.Initialization(imageTargetModel);
        imageTarget.onTargetFound.AddListener(value => onTargetFound.Invoke(value));
        imageTarget.onTargetLost.AddListener(value => onTargetLost.Invoke(value));
        _images.Add(imageTargetModel.name, imageTarget);

        onTargetCreated.Invoke(imageTarget);

        return Task.FromResult<ImageTargetBase>(imageTarget);
    }

    public override void RemoveImageTarget(ImageTargetBase imageTarget)
    {
        if (!_images.ContainsKey(imageTarget.imageTargetName) || imageTarget is not ImageTargetVuforia imageTargetVuforia)
        {
            throw new NullReferenceException($"Can't find {imageTarget.imageTargetName}");
        }

        var trackable = imageTargetVuforia.imageTargetBehaviour.Trackable;
        var objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        var dataset = objectTracker.GetDataSets().FirstOrDefault(t => t.GetTrackables().Contains(trackable));
        objectTracker.DeactivateDataSet(dataset);
        objectTracker.DestroyDataSet(dataset, true);
        _images.Remove(imageTarget.imageTargetName);
    }

    protected override void OnEnable()
    {
        if (_isInitialized)
        {
            VuforiaBehaviour.Instance.enabled = true;
        }
    }

    protected override void OnDisable()
    {
        var behaviour = VuforiaBehaviour.Instance;
        if (_isInitialized && behaviour)
        {
            behaviour.enabled = false;
        }
    }

    private static ImageTargetBehaviour CreateImageTargetFromTexture(Texture2D texture, float widthInMeters, string targetName)
    {
        var objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        var runtimeImageSource = objectTracker.RuntimeImageSource;
        runtimeImageSource.SetImage(texture, widthInMeters, targetName);

        var dataset = objectTracker.CreateDataSet();

        if (dataset == null)
        {
            throw new Exception("Can't create dataset");
        }

        var trackableBehaviour = dataset.CreateTrackable(runtimeImageSource, targetName);

        objectTracker.ActivateDataSet(dataset);

        return trackableBehaviour as ImageTargetBehaviour;
    }
}
