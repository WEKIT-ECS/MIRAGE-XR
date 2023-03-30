using System;
using System.Collections.Generic;
using System.Linq;
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
    private HashSet<string> _names = new HashSet<string>();

    public async Task<(bool, string)> TryAddImageTarget(ImageTargetModel model)
    {
        try
        {
            var newName = await AddImageTargetAsync(model);
            _images.Add(model);
            return (true, newName);
        }
        catch (Exception e)
        {
            AppLog.LogError(e.ToString());
            return (false, null);
        }
    }

    public bool TryRemoveImageTarget(string imageTargetName)
    {
        try
        {
            RemoveImageTarget(imageTargetName);
            return true;
        }
        catch (Exception e)
        {
            AppLog.LogError(e.ToString());
            return false;
        }
    }

    private void RemoveImageTarget(string imageTargetName)
    {
        var model = _images.FirstOrDefault(t => t.name == imageTargetName);
        if (_names.Contains(imageTargetName) && model != null)
        {
            _imageTargetManager.RemoveImageTarget(model);
            _names.Remove(imageTargetName);
        }
        else
        {
            throw new Exception($"ImageTargetManagerWrapper: Can't find imageTarget by name: '{imageTargetName}'");
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
                var newName = await AddImageTargetAsync(image);
                image.name = newName;
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

    private async Task<string> AddImageTargetAsync(ImageTargetModel model)
    {
        if (!model.texture2D.isReadable)
        {
            throw new Exception("ImageTargetManagerWrapper: Texture must be readable");
        }

        if (_names.Contains(model.name))
        {
            model.name = $"{model.name}_{Guid.NewGuid()}";
        }

        await _imageTargetManager.AddImageTarget(model);
        _names.Add(model.name);

        return model.name;
    }
}
