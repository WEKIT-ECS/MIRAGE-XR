using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using MirageXR;
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
    private HashSet<string> _names = new HashSet<string>();

    public async Task<string> AddImageTarget(ImageTargetModel model)
    {
        var newName = await AddImageTargetAsync(model);

        if (newName != null)
        {
            _images.Add(model);
        }

        return newName;
    }

    public void RemoveImageTarget(string imageTargetName)
    {
        try
        {
            var model = _images.FirstOrDefault(t => t.name == imageTargetName);
            if (_names.Contains(imageTargetName) && model != null)
            {
                _imageTargetManager.RemoveImageTarget(model);
                _names.Remove(imageTargetName);
            }
            else
            {
                AppLog.LogError($"ImageTargetManagerWrapper: Can't find {imageTargetName}");
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

    private void Start()
    {
        InitializationAsync().AsAsyncVoid();
    }

    private async Task InitializationAsync()
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
            var newName = await AddImageTargetAsync(image);
            if (!string.IsNullOrEmpty(newName))
            {
                image.name = newName;
            }
        }

        AppLog.LogInfo("ImageTargetManagerWrapper: Initialization completed");
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

    private async Task<string> AddImageTargetAsync(ImageTargetModel model)
    {
        if (!model.texture2D.isReadable)
        {
            AppLog.LogError("ImageTargetManagerWrapper: Texture must be readable");
            return null;
        }

        if (_names.Contains(model.name))
        {
            model.name = $"{model.name}_{Guid.NewGuid()}";
        }

        try
        {
            await _imageTargetManager.AddImageTarget(model);
            _names.Add(model.name);
        }
        catch (Exception e)
        {
            AppLog.LogError(e.ToString());
        }

        return model.name;
    }
}
