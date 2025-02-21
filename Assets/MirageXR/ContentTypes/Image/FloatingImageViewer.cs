using i5.Toolkit.Core.VerboseLogging;
using LearningExperienceEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace MirageXR
{
    public class FloatingImageViewer : MirageXRPrefab
    {
        private const string MAIN_TEXTUERE = "_MainTex";

        private LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;

        [Tooltip("Image file. .jpg and .png formats supported")]

        [SerializeField] private string imageName = "image.jpg";
        [Tooltip("Set to false to read from project's 'Resources' folder; set to true to read from applications 'LocalState' folder on HoloLens, or online, if filename starts with 'http'")]
        [SerializeField] private bool useExternalSource = false;
        [SerializeField] private bool InPanel;
        [SerializeField] private GameObject BackgroundPortrait;
        [SerializeField] private GameObject BackgroundLandscape;
        [SerializeField] private GameObject FramePortrait;
        [SerializeField] private GameObject FrameLandscape;

        [SerializeField] private GameObject Background;
        [Space]
        //Variables for caption handling <start>
        [SerializeField] TMP_Text _captionTextLandscape;
        [SerializeField] private GameObject _captionObjectLandscape;
        [SerializeField] private TMP_Text _captionTextPortrait;
        [SerializeField] private GameObject _captionObjectPortrait;
        //Variable for caption handling <end>

        private Vector3 _originalPosition = Vector3.zero;
        private Quaternion _originalRotation = Quaternion.identity;
        private Vector3 _originalScale = Vector3.one;
        private bool _originalGuideState;
        private LearningExperienceEngine.ToggleObject _obj;
        private GameObject _thinLine;
        private GameObject _contentObject;
        private Texture2D _texture;
       
        public LearningExperienceEngine.ToggleObject ToggleObject => _obj;

        /// <summary>
        /// Initialization method.
        /// </summary>
        /// <param name="obj">Action toggle object.</param>
        /// <returns>Returns true if initialization succesfull.</returns>
        public override bool Init(LearningExperienceEngine.ToggleObject obj)
        {
            _obj = obj;

            // Check that url is not empty.
            if (string.IsNullOrEmpty(obj.url))
            {
                Debug.LogWarning("Content URL not provided.");
                return false;
            }

            // Try to set the parent and if it fails, terminate initialization.
            if (!SetParent(obj))
            {
                Debug.LogWarning("Couldn't set the parent.");
                return false;
            }

            if (obj.key == "P")
            {
                SetOrientation(FramePortrait, FrameLandscape, BackgroundPortrait);
            }
            else
            {
                SetOrientation(FrameLandscape, FramePortrait, BackgroundLandscape);
            }

            // Get the last bit of the url.
            var id = obj.url.Split('/').LastOrDefault();

            // Rename with the predicate + id to get unique name.
            name = $"{obj.predicate}_{id}";

            // Load from resources.
            if (obj.url.StartsWith("resources://"))
            {
                // Set image url.
                imageName = obj.url.Replace("resources://", string.Empty);

                // Create image viewer. Defaults to 4:3 landscape images for now.
                CreateImageViewer(1.0f, 0.75f, false);
            }
            else // Load from external url.
            {
                // Set image url.
                imageName = obj.url;

                // Create image viewer. Defaults to 4:3 landscape images for now.
                CreateImageViewer(1.0f, 0.75f, true);
            }

            // load scaling
            transform.localScale = obj.scale != 0 ? new Vector3(obj.scale, obj.scale, obj.scale) : Vector3.one;

            if (!obj.id.Equals("UserViewport"))
            {
                // Setup guide line feature.
                if (!SetGuide(obj))
                {
                    return false;
                }

                _thinLine = transform.FindDeepChild("ThinLine").gameObject;
            }

            var poiEditor = GetComponentInParent<PoiEditor>();
            if (poiEditor)
            {
                poiEditor.UpdateManipulationOptions(gameObject);
            }

            OnLock(_obj.poi, _obj.positionLock);
            LearningExperienceEngine.EventManager.OnAugmentationLocked += OnLock;
       
            //caption logic
            var caption = obj.caption;

            if (caption != string.Empty)
            {
                _captionObjectLandscape.SetActive(true);
                _captionObjectPortrait.SetActive(true);
                _captionTextLandscape.text = caption;
                _captionTextPortrait.text = caption;
            }

            return base.Init(obj);
        }

        /// <summary>
        /// This method creates a plane mesh and points an image texture to it defined by "imageName", which should be set before calling the method
        /// </summary>
        /// <param name="width">Use to set image aspect ratio. If 0 or negative, default 4:3 is used.</param>
        /// <param name="height">Use to set image aspect ratio. If 0 or negative, default 4:3 is used.</param>
        /// <param name="useExternalImageSource">If true, load image from application's LocalState folder, if false, load from project resources.</param>
        public void CreateImageViewer(float width, float height, bool useExternalImageSource)
        {
            useExternalSource = useExternalImageSource;

            var meshRenderer = Background.GetComponent<MeshRenderer>();
            meshRenderer.material.shader = Shader.Find("Unlit/Texture");

            if (useExternalSource)
            {
                LoadImage().AsAsyncVoid();
            }
            else
            {
                // If the image name has a suffix, remove it
                if (imageName.EndsWith(".png") || imageName.EndsWith(".jpg"))
                {
                    imageName = imageName.Substring(0, imageName.Length - 4);
                }

                var imageTex = Resources.Load(imageName, typeof(Texture2D)) as Texture2D;
                meshRenderer.sharedMaterial.SetTexture(MAIN_TEXTUERE, imageTex);
            }
        }

        private async Task LoadImage()
        {
            var meshRenderer = Background.GetComponent<MeshRenderer>();
            string path;
            if (imageName.StartsWith("http"))
            {
                if (!imageName.Contains('/'))
                {
                    Debug.LogError($"Can't parse file name '{imageName}'");
                }

                var fileName = imageName.Split('/').LastOrDefault();
                path = Path.Combine(activityManager.ActivityPath, fileName);
            }
            else
            {
                path = Path.Combine(Application.persistentDataPath, imageName);
            }

            if (!File.Exists(path))
            {
                Debug.LogError($"File {path} doesn't exists");
                return;
            }

            var data = await File.ReadAllBytesAsync(path);
            _texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
            _texture.LoadImage(data);
            meshRenderer.sharedMaterial.SetTexture(MAIN_TEXTUERE, _texture);
        }

        private void SetOrientation(GameObject activeFrame, GameObject unusedFrame, GameObject background)
        {
            activeFrame.SetActive(true);
            // set selected frame

            unusedFrame.SetActive(false);
            unusedFrame.transform.localScale = new Vector3(0, 0, 0);
            unusedFrame.transform.localPosition = activeFrame.transform.localPosition;
            // set unused frame to not active and resize/relocate as to not affect the object bounding box

            Background = background;

            var boundsControl = gameObject.GetComponent<BoundsControl>();
            boundsControl.enabled = false;
            boundsControl.enabled = true;
            // required to reset the bounding boxes of the frame used so that it displays correctly
        }

        public void ToggleInPanel(bool inPanel)
        {
            if (inPanel)
            {
                _originalPosition = transform.position;
                _originalRotation = transform.rotation;
                _originalScale = transform.localScale;

                if (_thinLine != null)
                {
                    _originalGuideState = _thinLine.activeSelf;
                    _thinLine.SetActive(false);
                }

                InPanel = true;
            }
            else
            {
                InPanel = false;
                transform.position = _originalPosition;
                transform.rotation = _originalRotation;
                transform.localScale = _originalScale;

                if (_thinLine != null)
                    _thinLine.SetActive(_originalGuideState);
            }
        }

        private void Update()
        {
            if (InPanel)
            {
                transform.localScale = Vector3.one * 0.35f;
            }
        }

        private void OnDestroy()
        {
            if (_contentObject != null)
            {
                Destroy(_contentObject);
            }

            if (_texture)
            {
                Destroy(_texture);
            }
            LearningExperienceEngine.EventManager.OnAugmentationLocked -= OnLock;
        }

        private void OnLock(string id, bool locked)
        {
            if (id == _obj.poi)
            {
                _obj.positionLock = locked;

                GetComponentInParent<PoiEditor>().IsLocked(_obj.positionLock);

                if (gameObject.GetComponent<ObjectManipulator>())
                {
                    gameObject.GetComponent<ObjectManipulator>().enabled = !_obj.positionLock;
                }
            }
        }

        public override void Delete()
        {

        }
    }
}
