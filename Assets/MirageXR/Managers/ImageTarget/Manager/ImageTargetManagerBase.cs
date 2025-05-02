using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MirageXR;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class ImageTargetModel
{
    public string name;
    public Texture2D texture2D;
    public float width = 1f;
    public GameObject prefab;
    public bool useLimitedTracking;
}

[Serializable]
public class UnityEventImageTarget : UnityEvent<IImageTarget> { }

public abstract class ImageTargetManagerBase : MonoBehaviour
{
    protected UnityEventImageTarget _onTargetCreated = new UnityEventImageTarget();
    protected UnityEventImageTarget _onTargetFound = new UnityEventImageTarget();
    protected UnityEventImageTarget _onTargetLost = new UnityEventImageTarget();
    protected Dictionary<string, ImageTargetBase> _images = new Dictionary<string, ImageTargetBase>();

    protected bool _isInitialized;

    public bool isInitialized => _isInitialized;

    public UnityEventImageTarget onTargetCreated => _onTargetCreated;

    public UnityEventImageTarget onTargetFound => _onTargetFound;

    public UnityEventImageTarget onTargetLost => _onTargetLost;

    public abstract Task<bool> InitializationAsync(IViewManager viewManager);

    public abstract Task<bool> ResetAsync();

    protected abstract void OnEnable();

    protected abstract void OnDisable();

    public abstract Task<ImageTargetBase> AddImageTarget(ImageTargetModel imageTargetModel, CancellationToken cancellationToken = default);

    public abstract void RemoveImageTarget(ImageTargetBase imageTarget);

    public ImageTargetBase GetImageTarget(string imageName)
    {
        return _images.ContainsKey(imageName) ? _images[imageName] : null;
    }
}
