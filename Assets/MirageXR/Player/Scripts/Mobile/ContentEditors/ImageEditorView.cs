using System;
using System.CodeDom.Compiler;
using System.IO;
using FakeItEasy;
using IBM.Watson.NaturalLanguageUnderstanding.V1.Model;
using System.Runtime.InteropServices.Expando;
using MirageXR;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;
using IBM.Watsson.Examples;

public class ImageEditorView : PopupEditorBase
{
    private const int MAX_PICTURE_SIZE = 1024;
    private static ActivityManager activityManager => RootObject.Instance.activityManager;

    private static AugmentationManager augmentationManager => RootObject.Instance.augmentationManager;

    public override ContentType editorForType => ContentType.IMAGE;

    private const string LANDSCAPE = "L";
    private const string PORTRAIT = "P";
    private bool _orientation;

    [SerializeField] private Transform _imageHolder;
    [SerializeField] private Image _image;
    [SerializeField] private UnityEngine.UI.Button _btnCaptureImage;
    [SerializeField] private UnityEngine.UI.Button _btnOpenGallery;
    [SerializeField] private UnityEngine.UI.Toggle _toggleOrientation;
    private Texture2D _capturedImage;
    private string text;
    [SerializeField] private TMP_InputField _captionText;
    [SerializeField] private TMP_Text _captionPreview;

    [SerializeField] private UnityEngine.UI.Button _captionAdd;
    [SerializeField] private UnityEngine.UI.Button _captionDone;
    [SerializeField] private UnityEngine.UI.Button _captionEditBtn;
    [SerializeField] private UnityEngine.UI.Button _captionSaveBtn;
    [SerializeField] private UnityEngine.UI.Button _captionSaveBackBtn;

    [SerializeField] private GameObject obj1;
    [SerializeField] private GameObject obj2;
    [SerializeField] private GameObject obj3;
    [SerializeField] private GameObject obj4;

    
    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);
        UpdateView();
        _btnCaptureImage.onClick.AddListener(OnCaptureImage);
        _btnOpenGallery.onClick.AddListener(OpenGallery);

        _captionAdd.onClick.AddListener(OnCaptionAddClicked);
        _captionDone.onClick.AddListener(DoneaddingCaption);
        _captionEditBtn.onClick.AddListener(OnEditButtonClicked);
        _captionSaveBtn.onClick.AddListener(OnDoneButtonClick);
        _captionSaveBackBtn.onClick.AddListener(OnDoneButtonClick);

        _toggleOrientation.onValueChanged.AddListener(OnToggleOrientationValueChanged);
        _toggleOrientation.isOn = _orientation;
    }

    private void OnDestroy()
    {
        if (_capturedImage) Destroy(_capturedImage);
    }
    private string GetTextFilePath(string imagePath)
    {
        return Path.ChangeExtension(imagePath, ".txt");
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
                var sprite = Utilities.TextureToSprite(texture2D);
                SetPreview(sprite.texture);
            }
        });
    }

    private void CaptureImage()
    {
        RootObject.Instance.imageTargetManager.enabled = false;
        NativeCameraController.TakePicture(OnPictureTaken);
    }

    private void OnPictureTaken(bool result, Texture2D texture2D)
    {
        RootObject.Instance.imageTargetManager.enabled = true;
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
        var sprite = Utilities.TextureToSprite(_capturedImage);
        _image.sprite = sprite;

        var rtImageHolder = (RectTransform)_imageHolder.transform;
        var rtImage = (RectTransform)_image.transform;
        var height = (rtImage.rect.width / _capturedImage.width * _capturedImage.height) + (rtImage.sizeDelta.y * -1);
        rtImageHolder.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }

    public void OnCaptionAddClicked()
    {
        obj1.SetActive(false);
        obj2.SetActive(false);
        obj3.SetActive(true);
        
    }
    public void DoneaddingCaption()
    {
        
        obj4.SetActive(true);
        obj3.SetActive(false);
        text = _captionText.text;
        Debug.Log(text);
    }
    public void OnEditButtonClicked()
    {
        
        obj4.SetActive(false);
        obj3.SetActive(true);
        _captionPreview.text = text;
        Debug.Log(text);
    }
    private void OnDoneButtonClick()
    {
        obj1.SetActive(true);
        obj2.SetActive(true);
        obj3.SetActive(false);
        SaveTextToFile();
    }
    
    private void SaveTextToFile()
    {
        string inputText = _captionText.text;
        string captionfilePath = Path.Combine(activityManager.ActivityPath, $"MirageXR_Image_{DateTime.Now.ToFileTimeUtc()}.txt");
        // Check if the input text is not empty
        if (!string.IsNullOrEmpty(inputText))
        {
            // Write the input text to the text file
            using (StreamWriter writer = new StreamWriter(captionfilePath, true))
            {
                writer.WriteLine(inputText);
            }

            Debug.Log("Text saved to file: " + captionfilePath);
        }
        else
        {
            Debug.LogWarning("Input text is empty. Cannot save to file.");
        }
    }
}
