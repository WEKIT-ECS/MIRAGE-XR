using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using UnityEngine;

public class ImageTargetManagerWrapper : MonoBehaviour
{
    [Serializable]
    public enum ForceManagerType
    {
        Default,
        Editor,
        ARFoundation,
        Vuforia,
    }

    [SerializeField] private ForceManagerType _forceManagerType;
    [SerializeField] private List<ImageTargetModel> _images;
    [SerializeField] private UnityEventImageTarget _onTargetCreated = new UnityEventImageTarget();
    [SerializeField] private UnityEventImageTarget _onTargetFound = new UnityEventImageTarget();
    [SerializeField] private UnityEventImageTarget _onTargetLost = new UnityEventImageTarget();

    public UnityEventImageTarget onTargetCreated => _onTargetCreated;

    public UnityEventImageTarget onTargetFound => _onTargetFound;

    public UnityEventImageTarget onTargetLost => _onTargetLost;

    private ImageTargetManagerBase _imageTargetManager;
    private Dictionary<string, IImageTarget> _imagesMap = new Dictionary<string, IImageTarget>();

    public async Task<IImageTarget> AddImageTarget(ImageTargetModel model, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!model.texture2D.isReadable)
            {
                throw new Exception("ImageTargetManagerWrapper: Texture must be readable");
            }

            var newModel = new ImageTargetModel
            {
                name = model.name,
                prefab = model.prefab,
                width = model.width,
                texture2D = model.texture2D,
                useLimitedTracking = model.useLimitedTracking,
            };

            if (_imagesMap.ContainsKey(newModel.name))
            {
                newModel.name = $"{model.name}_{Guid.NewGuid()}";
            }

            var target = await _imageTargetManager.AddImageTarget(newModel, cancellationToken);
            _imagesMap.Add(newModel.name, target);

            return target;
        }
        catch (OperationCanceledException e)
        {
            AppLog.LogInfo(e.ToString());
            return null;
        }
        catch (Exception e)
        {
            AppLog.LogError(e.ToString());
            return null;
        }
    }

    public void RemoveImageTarget(IImageTarget imageTarget)
    {
        try
        {
            if (_imagesMap.ContainsKey(imageTarget.imageTargetName))
            {
                _imageTargetManager.RemoveImageTarget(imageTarget as ImageTargetBase);
                _imagesMap.Remove(imageTarget.imageTargetName);
            }
            else
            {
                throw new Exception($"ImageTargetManagerWrapper: Can't find imageTarget by name: '{imageTarget.imageTargetName}'");
            }
        }
        catch (Exception e)
        {
            AppLog.LogError(e.ToString());
        }
    }

    private void OnEnable()
    {
        if (_imageTargetManager)
        {
            _imageTargetManager.enabled = true;
        }
    }

    private void OnDisable()
    {
        if (_imageTargetManager)
        {
            _imageTargetManager.enabled = false;
        }
    }

    public async Task InitializationAsync()
    {
        _imageTargetManager = CreateImageTargetManager();

        if (!_imageTargetManager)
        {
            return;
        }

        try
        {
            var result = await _imageTargetManager.InitializationAsync();
            if (!result)
            {
                AppLog.LogInfo("ImageTargetManagerWrapper: unable to initialize");
                return;
            }
        }
        catch (Exception e)
        {
            AppLog.LogInfo(e.ToString());
            return;
        }

        _imageTargetManager.onTargetCreated.AddListener(_onTargetCreated.Invoke);
        _imageTargetManager.onTargetFound.AddListener(_onTargetFound.Invoke);
        _imageTargetManager.onTargetLost.AddListener(_onTargetLost.Invoke);
        foreach (var image in _images)
        {
            try
            {
                var target = await AddImageTarget(image);
                image.name = target.imageTargetName;
            }
            catch (Exception e)
            {
                AppLog.LogError(e.ToString());
            }
        }
    }

    private void OnDestroy()
    {
        if (_imageTargetManager)
        {
            _imageTargetManager.onTargetCreated.AddListener(_onTargetCreated.Invoke);
            _imageTargetManager.onTargetFound.RemoveListener(_onTargetFound.Invoke);
            _imageTargetManager.onTargetLost.RemoveListener(_onTargetLost.Invoke);
        }
    }

    private ImageTargetManagerBase CreateImageTargetManager()
    {
        switch (_forceManagerType)
        {
            case ForceManagerType.Default:
                return CreateDefaultImageTargetManager();
            case ForceManagerType.Editor:
                return gameObject.AddComponent<ImageTargetManagerEditor>();
            case ForceManagerType.ARFoundation:
                return gameObject.AddComponent<ImageTargetManagerARFoundation>();
            case ForceManagerType.Vuforia:
                return gameObject.AddComponent<ImageTargetManagerVuforia>();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private ImageTargetManagerBase CreateDefaultImageTargetManager()
    {
#if UNITY_EDITOR
        var manager = gameObject.AddComponent<ImageTargetManagerEditor>();
#elif UNITY_IOS || UNITY_ANDROID
        var manager = gameObject.AddComponent<ImageTargetManagerARFoundation>();
#else
        var manager = gameObject.AddComponent<ImageTargetManagerVuforia>();
#endif
        return manager;
    }
}
