using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModelListItem : MonoBehaviour
{
    [SerializeField] private Button _btn;
    [SerializeField] private Button _threeDotBtn;
    [SerializeField] private Image _image;
    [SerializeField] private Image _imageDownloaded;
    [SerializeField] private Image _imageProgress;
    [SerializeField] private TMP_Text _label;

    public bool isDownloaded
    {
        get => _isDownloaded;
        set
        {
            _isDownloaded = value;
            _imageDownloaded.gameObject.SetActive(value);
        }
    }
    public ModelPreviewItem previewItem => _previewItem;
    public bool interactable
    {
        get => _btn.interactable;
        set => _btn.interactable = value;
    }

    private bool _isDownloaded;
    private bool _isImageDownloaded;
    private ModelPreviewItem _previewItem;
    private string _token;
    private bool _hasImageBeenDownloaded;
    private Action<ModelListItem> _downloadAction;
    private Action<ModelListItem> _acceptAction;
    private Action<ModelListItem> _removeAction;

    public void Init(ModelPreviewItem item, bool downloaded, Action<ModelListItem> downloadAction, Action<ModelListItem> acceptAction, Action<ModelListItem> removeAction)
    {
        _downloadAction = downloadAction;
        _acceptAction = acceptAction;
        _removeAction = removeAction;
        _previewItem = item;
        isDownloaded = downloaded;
        _imageProgress.gameObject.SetActive(false);
        _label.text = _previewItem.name;
        _btn.onClick.AddListener(OnButtonClicked);
        _threeDotBtn.onClick.AddListener(OnThreeDotButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (_isDownloaded)
        {
            _acceptAction(this);
        }
        else
        {
            _downloadAction(this);
        }
    }

    private void OnThreeDotButtonClicked()
    {
        RootView_v2.Instance.dialog.ShowMiddleMultiline(
            "Actions",
            ("Rename object", RenameObject, false),
            ("Delete object", DeleteObject, false),
            ("Cancel", null, true));
    }

    private void RenameObject()
    {
        // TODO
    }

    private void DeleteObject()
    {
        _removeAction(this);
    }


    public async Task LoadImage()
    {
        if (!_isImageDownloaded)
        {
            var (result, sprite) = await MirageXR.Sketchfab.LoadSpriteAsync(_previewItem.resourceImage.url);
            if (result)
            {
                if (_image) _image.sprite = sprite;
                _isImageDownloaded = true;
            }
        }
    }

    public void OnBeginDownload()
    {
        _imageProgress.gameObject.SetActive(true);
        _imageProgress.fillAmount = 0;
        _btn.interactable = false;
    }

    public void OnDownload(float progress)
    {
        _imageProgress.fillAmount = progress;
    }

    public void OnEndDownload()
    {
        _imageProgress.gameObject.SetActive(false);
        _btn.interactable = true;
    }

    private void OnDestroy()
    {
        if (_image.sprite)
        {
            Destroy(_image.sprite);
        }
    }
}