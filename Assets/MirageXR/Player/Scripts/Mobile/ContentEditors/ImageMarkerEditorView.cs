using System;
using System.Globalization;
using System.IO;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;
using Image = UnityEngine.UI.Image;

public class ImageMarkerEditorView : PopupEditorBase
{
    public override ContentType editorForType => ContentType.IMAGEMARKER;

    [SerializeField] private Transform _imageHolder;
    [SerializeField] private Image _image;
    [SerializeField] private Button _btnCaptureImage;
    [SerializeField] private Button _btnOpenGallery;
    [SerializeField] private TMP_InputField _tmpInputSize;

    private Texture2D _capturedImage;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);
        UpdateView();
        _btnCaptureImage.onClick.AddListener(OnCaptureImage);
        _btnOpenGallery.onClick.AddListener(OpenGallery);
    }

    private void OnDestroy()
    {
        if (_capturedImage) Destroy(_capturedImage);
    }

    protected override void OnAccept()
    {
        // close without saving if no image was taken
        if (_capturedImage == null)
        {
            Toast.Instance.Show("The image has not been captured");
            return;
        }

        var result = float.TryParse(_tmpInputSize.text, out var size);
        if (!result || size < 0)
        {
            Toast.Instance.Show("Please check the marker size");
            return;
        }

        if (_content != null)
        {
            EventManager.DeactivateObject(_content);

            // delete the previous image file
            var imageName = _content.url;
            var originalFileName = Path.GetFileName(imageName.Remove(0, HTTP_PREFIX.Length));
            var originalFilePath = Path.Combine(activityManager.ActivityPath, originalFileName);
            if (File.Exists(originalFilePath))
            {
                File.Delete(originalFilePath);
            }
        }
        else
        {
            _content = augmentationManager.AddAugmentation(_step, GetOffset());
            _content.predicate = editorForType.GetPredicate();
        }

        var saveFileName = $"MirageXR_Image_{DateTime.Now.ToFileTimeUtc()}.jpg";
        var outputPath = Path.Combine(activityManager.ActivityPath, saveFileName);
        File.WriteAllBytes(outputPath, _capturedImage.EncodeToJPG());

        _content.url = RESOURCES_PREFIX + saveFileName;
        _content.scale = size / 100;

        EventManager.ActivateObject(_content);
        EventManager.NotifyActionModified(_step);
        Close();
    }

    private void UpdateView()
    {
        if (_content != null && !string.IsNullOrEmpty(_content.url))
        {
            var originalFileName = Path.GetFileName(_content.url.Remove(0, RESOURCES_PREFIX.Length));
            var originalFilePath = Path.Combine(activityManager.ActivityPath, originalFileName);

            if (!File.Exists(originalFilePath)) return;

            _tmpInputSize.text = (_content.scale * 100).ToString(CultureInfo.InvariantCulture);

            var texture2D = Utilities.LoadTexture(originalFilePath);
            SetPreview(texture2D);
        }
    }

    private void OnCaptureImage()
    {
        CaptureImage();
    }

    private void OpenGallery()
    {

    }

    private void CaptureImage()
    {
        VuforiaBehaviour.Instance.enabled = false;
        NativeCameraController.TakePicture(OnPictureTaken);
    }

    private void OnPictureTaken(bool result, Texture2D texture2D)
    {
        VuforiaBehaviour.Instance.enabled = true;
        if (!result) return;
        SetPreview(texture2D);
    }

    private void SetPreview(Texture2D texture2D)
    {
        if (_capturedImage) Destroy(_capturedImage);

        _capturedImage = texture2D;
        var sprite = Utilities.TextureToSprite(_capturedImage);
        _image.sprite = sprite;

        var rtImageHolder = (RectTransform)_imageHolder.transform;
        var rtImage = (RectTransform)_image.transform;
        var height = rtImage.rect.width / _capturedImage.width * _capturedImage.height + (rtImage.sizeDelta.y * -1);
        rtImageHolder.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }
}