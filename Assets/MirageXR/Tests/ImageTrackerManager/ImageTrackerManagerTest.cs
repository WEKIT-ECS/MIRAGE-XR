using System.Threading.Tasks;
using MirageXR;
using UnityEngine;
using UnityEngine.UI;

public class ImageTrackerManagerTest : MonoBehaviour
{
    [SerializeField] private ImageTargetManagerWrapper _wrapper;
    [SerializeField] private Button _addImageButton;
    [SerializeField] private Button _removeImageButton;
    [SerializeField] private ImageTargetModel _imageTargetModel;

    private string imageTargetName;

    private void Start()
    {
        _wrapper.InitializationAsync().AsAsyncVoid();

        _addImageButton.onClick.AddListener(OnAddImageButtonClicked);
        _removeImageButton.onClick.AddListener(OnRemoveImageButtonClicked);
        _addImageButton.gameObject.SetActive(true);
        _removeImageButton.gameObject.SetActive(false);
    }

    private void OnAddImageButtonClicked()
    {
        AddImageTargetAsync().AsAsyncVoid();
    }

    private void OnRemoveImageButtonClicked()
    {
        _addImageButton.gameObject.SetActive(true);
        _removeImageButton.gameObject.SetActive(false);
        _wrapper.TryRemoveImageTarget(imageTargetName);
    }

    private async Task AddImageTargetAsync()
    {
        bool result;
        (result, imageTargetName) = await _wrapper.TryAddImageTarget(_imageTargetModel);
        if (result)
        {
            _addImageButton.gameObject.SetActive(false);
            _removeImageButton.gameObject.SetActive(true);
        }
    }
}
