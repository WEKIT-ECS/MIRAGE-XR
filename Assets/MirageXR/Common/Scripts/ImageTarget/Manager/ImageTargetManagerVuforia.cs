using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Vuforia;

public class ImageTargetManagerVuforia : ImageTargetManagerBase
{
    public override async Task<bool> InitializationAsync()
    {
        var vuforiaRuntime = VuforiaApplication.Instance;

        if (!vuforiaRuntime.IsInitialized)
        {
            vuforiaRuntime.Initialize();
        }

        while (!vuforiaRuntime.IsInitialized)
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

        Debug.Log("Deleteing Image target: " + imageTarget.name);

        Destroy(imageTarget);
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

        var trackableBehaviour = VuforiaBehaviour.Instance.ObserverFactory.CreateImageTarget(texture, widthInMeters, targetName);

        return trackableBehaviour;
        return null;
    }
}
