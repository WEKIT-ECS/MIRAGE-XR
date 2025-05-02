using System;
using System.Threading;
using System.Threading.Tasks;
using MirageXR;
using UnityEngine;

public class ImageTargetManagerEditor : ImageTargetManagerBase
{
    private const string HOLDER_NAME = "ImageTrackerHolder";
    private const float STEP_LENGTH = 0.5f;

    private Transform _holder;

    public override Task<bool> InitializationAsync(IViewManager viewManager)
    {
        _holder = new GameObject(HOLDER_NAME).transform;

        _isInitialized = true;

        return Task.FromResult(true);
    }

    public override async Task<bool> ResetAsync()
    {
        foreach (var pair in _images)
        {
            Destroy(pair.Value.gameObject);
        }

        _images.Clear();

        await Task.Yield();

        return true;
    }

    public override Task<ImageTargetBase> AddImageTarget(ImageTargetModel imageTargetModel, CancellationToken cancellationToken = default)
    {
        var newObject = new GameObject(imageTargetModel.name);
        newObject.transform.SetParent(_holder);
        newObject.transform.Translate(_images.Count * STEP_LENGTH, 0, 0);
        var imageTarget = newObject.AddComponent<ImageTargetEditor>();
        imageTarget.Initialization(imageTargetModel);
        _images.Add(imageTargetModel.name, imageTarget);

        onTargetCreated.Invoke(imageTarget);

        return Task.FromResult<ImageTargetBase>(imageTarget);
    }

    protected override void OnEnable()
    {
        foreach (var imageTarget in _images)
        {
            if (!imageTarget.Value || !imageTarget.Value.gameObject)
            {
                return;
            }

            imageTarget.Value.gameObject.SetActive(true);
        }
    }

    protected override void OnDisable()
    {
        foreach (var imageTarget in _images)
        {
            if (!imageTarget.Value || !imageTarget.Value.gameObject)
            {
                return;
            }

            imageTarget.Value.gameObject.SetActive(false);
        }
    }

    public override void RemoveImageTarget(ImageTargetBase imageTarget)
    {
        if (_images.ContainsKey(imageTarget.imageTargetName))
        {
            Destroy(imageTarget.gameObject);
            _images.Remove(imageTarget.imageTargetName);
        }
        else
        {
            throw new NullReferenceException($"Can't find {imageTarget.imageTargetName}");
        }
    }
}
