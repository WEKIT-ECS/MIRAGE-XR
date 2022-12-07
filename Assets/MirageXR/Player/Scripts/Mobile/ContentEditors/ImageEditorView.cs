using System;
using System.IO;
using MirageXR;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;
using Image = UnityEngine.UI.Image;

public class ImageEditorView : PopupEditorBase
{
    private static ActivityManager activityManager => RootObject.Instance.activityManager;
    private static AugmentationManager augmentationManager => RootObject.Instance.augmentationManager;

    public override ContentType editorForType => ContentType.IMAGE;

    private const string LANDSCAPE = "L";
    private const string PORTRAIT = "P";
    private bool _orientation;

    [SerializeField] private Transform _imageHolder;
    [SerializeField] private Image _image;
    [SerializeField] private Button _btnCaptureImage;
    [SerializeField] private Toggle _toggleOrientation;

    private Texture2D _capturedImage;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);
        UpdateView();
        _btnCaptureImage.onClick.AddListener(OnCaptureImage);

        _toggleOrientation.onValueChanged.AddListener(OnToggleOrientationValueChanged);
        _toggleOrientation.isOn = _orientation;
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

        _content.key = _orientation ? LANDSCAPE : PORTRAIT;


        var saveFileName = $"MirageXR_Image_{DateTime.Now.ToFileTimeUtc()}.jpg";
        var outputPath = Path.Combine(activityManager.ActivityPath, saveFileName);
        File.WriteAllBytes(outputPath, _capturedImage.EncodeToJPG());

        _content.url = HTTP_PREFIX + saveFileName;
        _content.scale = 0.5f;
        EventManager.ActivateObject(_content);
        EventManager.NotifyActionModified(_step);
        Close();
    }

    private void UpdateView()
    {
        if (_content != null && !string.IsNullOrEmpty(_content.url))
        {
            var originalFileName = Path.GetFileName(_content.url.Remove(0, HTTP_PREFIX.Length));
            var originalFilePath = Path.Combine(activityManager.ActivityPath, originalFileName);

            if (!File.Exists(originalFilePath)) return;

            var texture2D = Utilities.LoadTexture(originalFilePath);
            SetPreview(texture2D);
        }
    }

    private void OnToggleOrientationValueChanged(bool value)
    {
        _orientation = value;
    }

    private void OnCaptureImage()
    {
        CaptureImage();
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
        var height = (rtImage.rect.width / _capturedImage.width * _capturedImage.height) + (rtImage.sizeDelta.y * -1);
        rtImageHolder.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }
}
