using System;
using System.IO;
using MirageXR;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class ThumbnailEditorView : PopupBase
{
    private const string THUMBNAIL_FILE_NAME = "thumbnail.jpg";

    [SerializeField] private Transform _imageHolder;
    [SerializeField] private Image _image;
    [SerializeField] private Button _btnCaptureImage;
    [SerializeField] protected Button _btnAccept;
    [SerializeField] protected Button _btnClose;

    private string _imagePath;
    private Action<string> _onAccept;
    private Texture2D _texture2D;

    protected override bool TryToGetArguments(params object[] args)
    {
        try
        {
            _onAccept = (Action<string>)args[0];
        }
        catch (Exception)
        {
            return false;
        }

        try
        {
            _imagePath = (string)args[1];
        }
        catch (Exception) { /*ignore*/ }
        return true;
    }

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);
        _btnCaptureImage.onClick.AddListener(OnCaptureImage);
        _btnAccept.onClick.AddListener(OnAccept);
        _btnClose.onClick.AddListener(Close);
        if (File.Exists(_imagePath))
        {
            SetPreview(MirageXR.Utilities.LoadTexture(_imagePath));
        }
    }

    private void OnAccept()
    {
        if (_texture2D == null)
        {
            Toast.Instance.Show("The image has not been taken.");
            return;
        }
        var path = Path.Combine(LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.ActivityPath, THUMBNAIL_FILE_NAME);
        File.WriteAllBytes(path, _texture2D.EncodeToJPG());
        _onAccept.Invoke(path);
        Close();
    }

    public override void Close()
    {
        if (_texture2D) Destroy(_texture2D);
        base.Close();
    }

    private void OnCaptureImage()
    {
        CaptureImage();
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
        if (_texture2D) Destroy(_texture2D);

        _texture2D = texture2D;
        var sprite = MirageXR.Utilities.TextureToSprite(_texture2D);
        _image.sprite = sprite;

        var rtImageHolder = (RectTransform)_imageHolder.transform;
        var rtImage = (RectTransform)_image.transform;
        var height = (rtImage.rect.width / _texture2D.width * _texture2D.height) + (rtImage.sizeDelta.y * -1);
        rtImageHolder.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }
}