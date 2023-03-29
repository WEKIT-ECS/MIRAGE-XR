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
        _addImageButton.onClick.AddListener(OnAddImageButtonClicked);
        _removeImageButton.onClick.AddListener(OnRemoveImageButtonClicked);
        _addImageButton.gameObject.SetActive(true);
        _removeImageButton.gameObject.SetActive(false);
    }

    private void OnAddImageButtonClicked()
    {
        _addImageButton.gameObject.SetActive(false);
        _removeImageButton.gameObject.SetActive(true);
        AddImageTargetAsync().AsAsyncVoid();
    }

    private void OnRemoveImageButtonClicked()
    {
        _addImageButton.gameObject.SetActive(true);
        _removeImageButton.gameObject.SetActive(false);
        _wrapper.RemoveImageTarget(imageTargetName);
    }

    private async Task AddImageTargetAsync()
    {
        imageTargetName = await _wrapper.AddImageTarget(_imageTargetModel);
    }
}
