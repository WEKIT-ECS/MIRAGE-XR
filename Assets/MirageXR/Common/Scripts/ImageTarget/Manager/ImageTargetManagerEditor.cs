using System.Threading.Tasks;
using UnityEngine;

public class ImageTargetManagerEditor : ImageTargetManagerBase
{
    private const string HOLDER_NAME = "ImageTrackerHolder";
    private const float STEP_LENGTH = 0.5f;

    private Transform _holder;

    public override Task<bool> InitializationAsync()
    {
        _holder = new GameObject(HOLDER_NAME).transform;

        _isInitialized = true;

        return Task.FromResult(true);
    }

    protected override void OnEnable()
    {
        foreach (var imageTarget in _images)
        {
            if (!imageTarget || !imageTarget.gameObject)
            {
                return;
            }

            imageTarget.gameObject.SetActive(true);
        }
    }

    protected override void OnDisable()
    {
        foreach (var imageTarget in _images)
        {
            if (!imageTarget || !imageTarget.gameObject)
            {
                return;
            }

            imageTarget.gameObject.SetActive(false);
        }
    }

    public override Task AddImageTarget(ImageTargetModel imageTargetModel)
    {
        var newObject = new GameObject(imageTargetModel.name);
        newObject.transform.SetParent(_holder);
        newObject.transform.Translate(_images.Count * STEP_LENGTH, 0, 0);
        var imageTarget = newObject.AddComponent<ImageTargetEditor>();
        imageTarget.Initialization(imageTargetModel);
        _images.Add(imageTarget);

        onTargetCreated.Invoke(imageTarget);

        return Task.CompletedTask;
    }
}
