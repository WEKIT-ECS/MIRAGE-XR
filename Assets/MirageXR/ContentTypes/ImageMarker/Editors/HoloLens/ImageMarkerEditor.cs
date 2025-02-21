using LearningExperienceEngine;
using System.IO;
using System.Threading.Tasks;
using MirageXR;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_WSA
using System.Linq;
using System.Collections;
#endif

public class ImageMarkerEditor : MonoBehaviour
{
    private static ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;

    [SerializeField] private Button captureButton;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Scrollbar cropScroll;
    [SerializeField] private AudioSource shutterPlayer;
    [SerializeField] private Image previewImage;
    [SerializeField] private Text processingText;
    [SerializeField] private InputField Size;
    [SerializeField] private Transform annotationStartingPoint;
    [SerializeField] private RectTransform cropImage;
    [SerializeField] private GameObject imageMarkerMobile;

    private LearningExperienceEngine.Action action;
    private LearningExperienceEngine.ToggleObject annotationToEdit;
    private string saveFileName;
    private Texture2D _capturedImage;

    public void SetAnnotationStartingPoint(Transform startingPoint)
    {
        annotationStartingPoint = startingPoint;
    }

    #region TakePhoto

    public bool IsThumbnail
    {
        get; set;
    }

#if UNITY_WSA
    private UnityEngine.Windows.WebCam.PhotoCapture photoCaptureObject = null;
#endif

    private void OnPictureTaken(bool result, Texture2D texture2D)
    {
        PlayCameraSound();
        if (result)
        {
            _capturedImage = texture2D;
            Debug.Log("Image W: " + _capturedImage.width);
            Debug.Log("Image H: " + _capturedImage.height);

            // _capturedImage.Resize((_capturedImage.width / 2), (_capturedImage.height / 2));
            Debug.Log("Image W: " + _capturedImage.width);
            Debug.Log("Image H: " + _capturedImage.height);
            var sprite = Sprite.Create(_capturedImage, new Rect(0, 0, _capturedImage.width, _capturedImage.height),
                new Vector2(0.5f, 0.5f), 100.0f);

            previewImage.sprite = sprite;
            previewImage.SetNativeSize();

            while (previewImage.rectTransform.sizeDelta.x > 100)
            {
                previewImage.rectTransform.sizeDelta /= 2;
            }
        }

        RootObject.Instance.ImageTargetManager.enabled = true;

        processingText.text = string.Empty;
        processingText.transform.parent.gameObject.SetActive(false);
        captureButton.gameObject.SetActive(true);
        acceptButton.gameObject.SetActive(true);
        closeButton.gameObject.SetActive(true);
    }

    private async Task ShowCountdown()
    {
        processingText.transform.parent.gameObject.SetActive(true);
        for (int i = 3; i > 0; i--)
        {
            processingText.text = i.ToString();
            await Task.Delay(1000);
        }

        processingText.text = "...";
    }

#if UNITY_WSA
    public async void CaptureImageAsync()
#else
    public void CaptureImageAsync()
#endif
    {
        Maggie.Speak("Taking a photo in 3 seconds");

        RootObject.Instance.ImageTargetManager.enabled = false;

        captureButton.gameObject.SetActive(false);
        acceptButton.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(false);

#if UNITY_WSA
        await ShowCountdown();
#endif
        if (_capturedImage) Destroy(_capturedImage);
        NativeCameraController.TakePicture(OnPictureTaken, IsThumbnail);
    }

    public void PlayCameraSound()
    {
        shutterPlayer.Play();
    }
    #endregion

    #region Crop Photo

    private Texture2D Cropper(Texture2D sourceTexture)
    {
        float squareSizeW = sourceTexture.width * cropScroll.value;
        float squareSizeH = sourceTexture.height * cropScroll.value;

        float x = previewImage.transform.localPosition.x + previewImage.rectTransform.rect.width;
        float xper = x / (previewImage.rectTransform.sizeDelta.x * 2);
        xper = 1f - xper;
        float xPos = xper * sourceTexture.width;
        xPos -= squareSizeW / 2;
        float y = previewImage.transform.localPosition.y + previewImage.rectTransform.rect.height;
        float yper = y / (previewImage.rectTransform.sizeDelta.y * 2);
        yper = 1f - yper;
        float yPos = yper * sourceTexture.height;
        yPos -= squareSizeH / 2;

        Color[] c = sourceTexture.GetPixels((int)xPos, (int)yPos, (int)squareSizeW, (int)squareSizeH);
        Texture2D croppedTexture = new Texture2D((int)squareSizeW, (int)squareSizeH);
        croppedTexture.SetPixels(c);
        croppedTexture.Apply();
        return croppedTexture;
    }

    public void scroll()
    {
        float scrollVal = cropScroll.value;
        float HW = scrollVal * 60;

        cropImage.sizeDelta = new Vector2(HW, HW);
    }
    #endregion

    private void SaveImageMarker(Texture2D tex)
    {
        saveFileName = $"MirageXR_ImageMarker_{System.DateTime.Now.ToFileTimeUtc()}.jpg";
        string outputPath = Path.Combine(activityManager.ActivityPath, saveFileName);

        Debug.Log("[ImageMarkerEditor] outpath = " + activityManager.ActivityPath);

        byte[] jpgBytes = tex.EncodeToJPG();
        File.WriteAllBytes(outputPath, jpgBytes);
    }

    public void OnAccept()
    {
        SaveImageMarker(Cropper(previewImage.sprite.texture));

        if (annotationToEdit != null)
        {
            LearningExperienceEngine.EventManager.DeactivateObject(annotationToEdit);
        }
        else
        {
            var workplaceManager = LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager;
            Detectable detectable = workplaceManager.GetDetectable(workplaceManager.GetPlaceFromTaskStationId(action.id));
            GameObject originT = GameObject.Find(detectable.id);

            var offset = MirageXR.Utilities.CalculateOffset(annotationStartingPoint.transform.position,
                annotationStartingPoint.transform.rotation,
                originT.transform.position,
                originT.transform.rotation);

            annotationToEdit = LearningExperienceEngine.LearningExperienceEngine.Instance.AugmentationManager.AddAugmentation(action, offset);
            annotationToEdit.predicate = "imagemarker";
        }

        Debug.Log("Annotation to edit: " + annotationToEdit.predicate);

        annotationToEdit.url = "resources://" + saveFileName;
        float size;
        if (Size.text == "")
        {
            size = 1f;
        }
        else
        {
            size = float.Parse(Size.text) / 100;
        }

        annotationToEdit.scale = size;

        LearningExperienceEngine.EventManager.ActivateObject(annotationToEdit);
        LearningExperienceEngine.EventManager.NotifyActionModified(action);
        Close();
    }

    public void Close()
    {
        action = null;
        annotationToEdit = null;
        saveFileName = string.Empty;
        gameObject.SetActive(false);

        Destroy(gameObject);
    }

    public void Open(Action action, ToggleObject annotation)
    {
        gameObject.SetActive(true);
        this.action = action;
        annotationToEdit = annotation;
        processingText.transform.parent.gameObject.SetActive(false);
        captureButton.gameObject.SetActive(true);
        acceptButton.gameObject.SetActive(true);
        closeButton.gameObject.SetActive(true);
        previewImage.sprite = null;
        cropScroll.value = 1;
    }

    public void mobiletest()
    {
        imageMarkerMobile.SetActive(true);
    }
}
