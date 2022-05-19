using MirageXR;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;
using System.Collections;

public class ImageMarkerEditor : MonoBehaviour
{
    [SerializeField] private Button captureButton;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Scrollbar cropScroll;
    [SerializeField] private AudioSource shutterPlayer;
    [SerializeField] private UnityEngine.UI.Image previewImage;
    [SerializeField] private Text processingText;
    [SerializeField] private InputField Size;


    [SerializeField] private Texture2D TestImage;


    [SerializeField] private Transform annotationStartingPoint;
    [SerializeField] private RectTransform cropImage;
    [SerializeField] private GameObject ObjectSelect;
    [SerializeField] private GameObject imageMarkerMobile;

    private Action action;
    private ToggleObject annotationToEdit;

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

            while (previewImage.rectTransform.sizeDelta.x > 100) {
                previewImage.rectTransform.sizeDelta /= 2;    
            }
        }

        VuforiaBehaviour.Instance.enabled = true;

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

    public async void CaptureImageAsync()
    {
        Maggie.Speak("Taking a photo in 3 seconds");

        Debug.Log("\nStartPhotoCapture\n");

        VuforiaBehaviour.Instance.enabled = false;
        Debug.Log("Vuforia enabled?: " + VuforiaBehaviour.Instance.enabled);

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
        xPos -= (squareSizeW / 2);
        float y = previewImage.transform.localPosition.y + previewImage.rectTransform.rect.height;
        float yper = y / (previewImage.rectTransform.sizeDelta.y * 2);
        yper = 1f - yper;
        float yPos = yper * sourceTexture.height;
        yPos -= (squareSizeH / 2);

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
        string outputPath = Path.Combine(ActivityManager.Instance.Path, saveFileName);

        Debug.Log("THIS IS THE OUTPATH: " + ActivityManager.Instance.Path);

        byte[] jpgBytes = tex.EncodeToJPG();
        File.WriteAllBytes(outputPath, jpgBytes);
    }

    public void OnAccept()
    {
        SaveImageMarker(Cropper(previewImage.sprite.texture));
       // SaveImageMarker(TestImage);

        if (annotationToEdit != null)
        {
            EventManager.DeactivateObject(annotationToEdit);
        }
        else
        {
            Detectable detectable = WorkplaceManager.Instance.GetDetectable(WorkplaceManager.Instance.GetPlaceFromTaskStationId(action.id));
            GameObject originT = GameObject.Find(detectable.id);

            var offset = Utilities.CalculateOffset(annotationStartingPoint.transform.position,
                annotationStartingPoint.transform.rotation,
                originT.transform.position,
                originT.transform.rotation);

            annotationToEdit = ActivityManager.Instance.AddAugmentation(action, offset);
            annotationToEdit.predicate = "imagemarker";
        }

        Debug.Log("Annotation to edit: " + annotationToEdit.predicate);

        annotationToEdit.url = "resources://" + saveFileName;
        float s = float.Parse(Size.text) / 100;
        annotationToEdit.scale = s;

        EventManager.ActivateObject(annotationToEdit);
        EventManager.NotifyActionModified(action);
        Close();
    }

    public void Close()
    {
        action = null;
        annotationToEdit = null;
        saveFileName = "";
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

    public void mobiletest() {
        imageMarkerMobile.SetActive(true);
    }
}
