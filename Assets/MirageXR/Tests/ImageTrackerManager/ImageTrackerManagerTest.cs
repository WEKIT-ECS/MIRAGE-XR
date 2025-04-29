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

    private IImageTarget _imageTarget;

    private void Start()
    {
        return;     //TODO: update
        //_wrapper.InitializationAsync().AsAsyncVoid();

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
        if (_imageTarget != null)
        {
            _addImageButton.gameObject.SetActive(true);
            _removeImageButton.gameObject.SetActive(false);
            _wrapper.RemoveImageTarget(_imageTarget);
            _imageTarget = null;
        }
    }

    private async Task AddImageTargetAsync()
    {
        _addImageButton.interactable = false;
        _imageTarget = await _wrapper.AddImageTarget(_imageTargetModel);
        if (_imageTarget != null)
        {
            _addImageButton.interactable = true;
            _addImageButton.gameObject.SetActive(false);
            _removeImageButton.gameObject.SetActive(true);
        }
    }
}
