using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    protected List<ImageTargetBase> _images = new List<ImageTargetBase>();

    protected bool _isInitialized;

    public bool isInitialized => _isInitialized;

    public UnityEventImageTarget onTargetCreated => _onTargetCreated;

    public UnityEventImageTarget onTargetFound => _onTargetFound;

    public UnityEventImageTarget onTargetLost => _onTargetLost;

    public abstract Task<bool> InitializationAsync();

    protected abstract void OnEnable();

    protected abstract void OnDisable();

    public abstract Task AddImageTarget(ImageTargetModel imageTargetModel);

    public virtual void RemoveImageTarget(ImageTargetModel imageTargetModel)
    {
        var obj = _images.FirstOrDefault(t => t.imageTargetName == imageTargetModel.name);
        if (obj)
        {
            _images.Remove(obj);
            Destroy(obj.gameObject);
        }
        else
        {
            throw new NullReferenceException($"Can't find {imageTargetModel.name}");
        }
    }
}
