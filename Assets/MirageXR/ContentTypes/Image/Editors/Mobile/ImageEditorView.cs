using LearningExperienceEngine;
using System;
using System.IO;
using DG.Tweening;
using MirageXR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class ImageEditorView : PopupEditorBase
{
    private const float HIDED_SIZE = 100f;
    private const float HIDE_ANIMATION_TIME = 0.5f;
    private const int MAX_PICTURE_SIZE = 1024;
    private const float IMAGE_HEIGHT = 630f;

    private static LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;

    private static LearningExperienceEngine.AugmentationManager augmentationManager => LearningExperienceEngine.LearningExperienceEngine.Instance.AugmentationManager;

    public override LearningExperienceEngine.ContentType editorForType => LearningExperienceEngine.ContentType.IMAGE;

    private const string LANDSCAPE = "L";
    private const string PORTRAIT = "P";
    private bool _orientation;

    [SerializeField] private Transform _imageHolder;
    [SerializeField] private Image _image;
    [SerializeField] private Button _btnCaptureImage;
    [SerializeField] private Button _btnOpenGallery;
    [SerializeField] private Toggle _toggleOrientation;

    //Caption handling variables <start>
    [Space]
    [SerializeField] private TMP_InputField _captionText;
    [SerializeField] private TMP_Text _captionPreview;
    [SerializeField] private Button _captionAdd;
    [SerializeField] private Button _captionDone;
    [SerializeField] private Button _captionEditBtn;
    [SerializeField] private Button _captionSaveBtn;
    [SerializeField] private Button _captionSaveBackBtn;
    [SerializeField] private GameObject TopPanel;
    [SerializeField] private GameObject BottomPanel;
    [SerializeField] private GameObject ImageCaptionEditorPrefab;
    [SerializeField] private GameObject ImageCaptionPreviewPrefab;
    //Caption handling variables <end>
    [Space]
    [SerializeField] private Button _btnArrow;
    [SerializeField] private RectTransform _panel;
    [SerializeField] private GameObject _arrowDown;
    [SerializeField] private GameObject _arrowUp;
    [Space]
    [SerializeField] private HintViewWithButtonAndToggle _hintPrefab;

    private string _text;
    private string _imageCaption = string.Empty;
    private Texture2D _capturedImage;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        _showBackground = false;
        base.Initialization(onClose, args);
        UpdateView();
        _btnCaptureImage.onClick.AddListener(OnCaptureImage);
        _btnOpenGallery.onClick.AddListener(OpenGallery);
        _btnArrow.onClick.AddListener(OnArrowButtonPressed);
        
        _arrowDown.SetActive(true);
        _arrowUp.SetActive(false);

        //Caption button events <start>
        _captionAdd.onClick.AddListener(OnCaptionAddClicked);
        _captionDone.onClick.AddListener(OnDoneAddingCaption);
        _captionEditBtn.onClick.AddListener(OnEditButtonClicked);
        _captionSaveBtn.onClick.AddListener(OnDoneButtonSaveCaption);
        _captionSaveBackBtn.onClick.AddListener(OnDoneButtonClick);

        //Caption button events <end>

        _toggleOrientation.onValueChanged.AddListener(OnToggleOrientationValueChanged);
        _toggleOrientation.isOn = _orientation;

        RootView_v2.Instance.HideBaseView();
    }

    private void OnDestroy()
    {
        if (_capturedImage) Destroy(_capturedImage);

        RootView_v2.Instance.ShowBaseView();
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
            LearningExperienceEngine.EventManager.DeactivateObject(_content);

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

        // TODO add rename window:
        if (!LearningExperienceEngine.UserSettings.dontShowNewAugmentationHint)
        {
            PopupsViewer.Instance.Show(_hintPrefab);
        }

        _content.key = _capturedImage.width > _capturedImage.height ? LANDSCAPE : PORTRAIT;

        var saveFileName = $"MirageXR_Image_{DateTime.Now.ToFileTimeUtc()}.jpg";
        var outputPath = Path.Combine(activityManager.ActivityPath, saveFileName);
        File.WriteAllBytes(outputPath, _capturedImage.EncodeToJPG());

        _content.url = HTTP_PREFIX + saveFileName;
        _content.scale = 0.5f;
        _content.caption = _imageCaption;
        LearningExperienceEngine.EventManager.ActivateObject(_content);

        base.OnAccept();
        Close();
    }

    private void UpdateView()
    {
        if (_content != null && !string.IsNullOrEmpty(_content.url))
        {
            var originalFileName = Path.GetFileName(_content.url.Remove(0, HTTP_PREFIX.Length));
            var originalFilePath = Path.Combine(activityManager.ActivityPath, originalFileName);

            if (!File.Exists(originalFilePath)) return;

            var texture2D = MirageXR.Utilities.LoadTexture(originalFilePath);
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

    private void OpenGallery()
    {
        PickImage(MAX_PICTURE_SIZE);
    }

    private void PickImage(int maxSize)
    {
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
        {
            Debug.Log("Image path: " + path);
            if (path != null)
            {
                // Create Texture from selected image
                Texture2D texture2D = NativeGallery.LoadImageAtPath(path, maxSize, false);

                if (texture2D == null)
                {
                    Debug.Log("Couldn't load texture from " + path);
                    return;
                }

                // Set picture
                var sprite = MirageXR.Utilities.TextureToSprite(texture2D);
                SetPreview(sprite.texture);
            }
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
    public void OnCaptionAddClicked()
    {
        TopPanel.SetActive(false);
        BottomPanel.SetActive(false);
        ImageCaptionEditorPrefab.SetActive(true);

    }
    public void OnDoneAddingCaption()
    {
        ImageCaptionPreviewPrefab.SetActive(true);
        ImageCaptionEditorPrefab.SetActive(false);
        _text = _captionText.text;
        _captionPreview.text = _text;
        //Debug.Log(text);
    }
    public void OnEditButtonClicked()
    {

        ImageCaptionPreviewPrefab.SetActive(false);
        ImageCaptionEditorPrefab.SetActive(true);
    }
    private void OnDoneButtonClick()
    {
        TopPanel.SetActive(true);
        BottomPanel.SetActive(true);
        ImageCaptionEditorPrefab.SetActive(false);
    }
    private void OnDoneButtonSaveCaption()
    {
        TopPanel.SetActive(true);
        BottomPanel.SetActive(true);
        ImageCaptionEditorPrefab.SetActive(false);
        ImageCaptionPreviewPrefab.SetActive(false);
        _imageCaption = _captionText.text;
    }
    private void OnArrowButtonPressed()
    {
        if (_arrowDown.activeSelf)
        {
            var hidedSize = HIDED_SIZE;
            _panel.DOAnchorPosY(-_panel.rect.height + hidedSize, HIDE_ANIMATION_TIME);
            _arrowDown.SetActive(false);
            _arrowUp.SetActive(true);
        }
        else
        {
            _panel.DOAnchorPosY(0.0f, HIDE_ANIMATION_TIME);
            _arrowDown.SetActive(true);
            _arrowUp.SetActive(false);
        }
    }
}
