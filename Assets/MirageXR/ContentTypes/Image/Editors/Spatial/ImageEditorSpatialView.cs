using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using MirageXR;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class ImageEditorSpatialView : PopupBase
{
    private const float HIDED_SIZE = 100f;
    private const float HIDE_ANIMATION_TIME = 0.5f;
    private const int MAX_PICTURE_SIZE = 1024;
    private const float IMAGE_HEIGHT = 630f;

    public ContentType Type => ContentType.Image;

    [SerializeField] private Transform _imageHolder;
    [SerializeField] private Image _image;
    [SerializeField] private Button _btnCaptureImage;
    [SerializeField] private Button _btnOpenGallery;    
    [SerializeField] private Button _btnApply;
    [SerializeField] private RectTransform _panel;
    [SerializeField] private GameObject _arrowDown;
    [SerializeField] private GameObject _arrowUp;
    [Space]
    [SerializeField] private HintViewWithButtonAndToggle _hintPrefab;

    private string _text;
    private Texture2D _capturedImage;
    private Content<ImageContentData> _content;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        _showBackground = false;
        base.Initialization(onClose, args);
        UpdateView();
        _btnCaptureImage.onClick.AddListener(OnCaptureImage);
        _btnOpenGallery.onClick.AddListener(OpenGallery);
        _btnApply.onClick.AddListener(OnAccept);

        _arrowDown.SetActive(true);
        _arrowUp.SetActive(false);
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        if (args is { Length: 1 } && args[0] is Content<ImageContentData> obj)
        {
            _content = obj;
        }

        return true;
    }

    private void OnDestroy()
    {
        if (_capturedImage) Destroy(_capturedImage);
    }

    private void OnAccept()
    {
        OnAcceptAsync().Forget();
    }

    private async UniTask OnAcceptAsync()
    {
        if (_capturedImage == null)
        {
            return;
        }

        var step = RootObject.Instance.StepManager.CurrentStep;
        var activityId = RootObject.Instance.ActivityManager.ActivityId;
        var fileId = _content?.ContentData?.Image?.Id ?? Guid.NewGuid();
        
        _content ??= new Content<ImageContentData>
        {
            Id = Guid.NewGuid(),
            CreationDate = DateTime.UtcNow,
            IsVisible = true,
            Steps = new List<Guid> { step.Id },
            Type = ContentType.Image,
            Version = Application.version,
            ContentData = new ImageContentData
            {
                IsBillboarded = false,
            },
            Location = Location.GetIdentityLocation()
        };

        _content.Location.Scale = CalculateScale(_capturedImage.width, _capturedImage.height);

        await SaveImageAsync(activityId, _content.Id, fileId);
        _content.ContentData.Image = await RootObject.Instance.AssetsManager.CreateFileAsync(activityId, _content.Id, fileId);
        RootObject.Instance.ContentManager.AddContent(_content);
        RootObject.Instance.AssetsManager.UploadFileAsync(activityId, _content.Id, fileId);

        Close();
    }

    private Vector3 CalculateScale(int textureWidth, int textureHeight)
    {
        if (textureWidth == textureHeight)
        {
            return Vector3.one;
        } 

        return textureWidth > textureHeight
            ? new Vector3(textureWidth / (float)textureHeight, 1, 0.05f)
            : new Vector3(1, textureHeight / (float)textureWidth, 0.05f);
    }
    
    private async UniTask SaveImageAsync(Guid activityId, Guid contentId, Guid fileId)
    {
        if (_capturedImage == null || _content == null)
        {
            return;
        }

        var bytes = _capturedImage.EncodeToPNG();
        var folder = RootObject.Instance.AssetsManager.GetFolderPath(activityId, contentId, fileId);
        Directory.CreateDirectory(folder);
        var filePath = Path.Combine(folder, "image.png");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        await File.WriteAllBytesAsync(filePath, bytes);
    }

    private void UpdateView()
    {
        if (_content != null)
        {
            var activityId = RootObject.Instance.ActivityManager.ActivityId;
            var folder = RootObject.Instance.AssetsManager.GetFolderPath(activityId, _content.Id, _content.ContentData.Image.Id);
            var imagePath = Path.Combine(folder, "image.png");
            if (!File.Exists(imagePath))
            {
                return;
            }

            var texture2D = MirageXR.Utilities.LoadTexture(imagePath);
            SetPreview(texture2D);
        }
    }

    private void OnCaptureImage()
    {
        CaptureImage();
    }

    private void OpenGallery()
    {
        PickImage(MAX_PICTURE_SIZE);
    }

    private void PickImage(int maxSize)
    {
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (path == null)
            {
                return;
            }

            var texture2D = NativeGallery.LoadImageAtPath(path, maxSize, false);

            if (texture2D == null)
            {
                Debug.Log("Couldn't load texture from " + path);
                return;
            }

            var sprite = MirageXR.Utilities.TextureToSprite(texture2D);
            SetPreview(sprite.texture);
        });
    }

    private void CaptureImage()
    {
        RootObject.Instance.ImageTargetManager.enabled = false;
        NativeCameraController.TakePicture(OnPictureTaken);
    }

    private void OnPictureTaken(bool result, Texture2D texture2D)
    {
        RootObject.Instance.ImageTargetManager.enabled = true;
        if (!result)
        {
            return;
        }

        SetPreview(texture2D);
    }

    private void SetPreview(Texture2D texture2D)
    {
        if (_capturedImage)
        {
            Destroy(_capturedImage);
        }

        _capturedImage = texture2D;

        var sprite = MirageXR.Utilities.TextureToSprite(_capturedImage);
        _image.sprite = sprite;

        var rtImageHolder = (RectTransform)_imageHolder.transform;
        var rtImage = (RectTransform)_image.transform;
        var width = (float)_capturedImage.width / _capturedImage.height * IMAGE_HEIGHT;

        rtImageHolder.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, IMAGE_HEIGHT);
        rtImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }
}
