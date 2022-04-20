using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System.Collections;
using UnityEngine;

namespace MirageXR
{
    public class FloatingImageViewer : MirageXRPrefab
    {
        private float _width = 0.4f;
        private float _height = 0.3f;

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


        //private Transform _contentPanel;
        private Vector3 _originalPosition = Vector3.zero;
        private Quaternion _originalRotation = Quaternion.identity;
        private Vector3 _originalScale = Vector3.one;

        private bool _originalGuideState;
        private GameObject _thinLine;
        private ToggleObject _obj;

        private GameObject _contentObject;

        public ToggleObject ToggleObject => _obj;


        //private void Awake()
        //{
        //    _contentPanel = GameObject.FindGameObjectWithTag("ContentPanel").transform;
        //}

        /// <summary>
        /// Initialization method.
        /// </summary>
        /// <param name="obj">Action toggle object.</param>
        /// <returns>Returns true if initialization succesfull.</returns>
        public override bool Init(ToggleObject obj)
        {
            _obj = obj;

            // Check that url is not empty.
            if (string.IsNullOrEmpty(obj.url))
            {
                Debug.Log("Content URL not provided.");
                return false;
            }

            // Try to set the parent and if it fails, terminate initialization.
            if (!SetParent(obj))
            {
                Debug.Log("Couldn't set the parent.");
                return false;
            }

            if (obj.key == "P")
            {
                setOrientation(FramePortrait, FrameLandscape, BackgroundPortrait);
            }
            else {
                setOrientation(FrameLandscape, FramePortrait, BackgroundLandscape);
            }
            // Get the last bit of the url.
            var id = obj.url.Split('/')[obj.url.Split('/').Length - 1];

            // Rename with the predicate + id to get unique name.
            name = obj.predicate + "_" + id;

            // Load from resources.
            if (obj.url.StartsWith("resources://"))
            {
                // Set image url.
                imageName = obj.url.Replace("resources://", "");

                // Create image viewer. Defaults to 4:3 landscape images for now.
                CreateImageViewer(1.0f, 0.75f, false);
            }

            // Load from external url.
            else
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
                    return false;

                _thinLine = transform.FindDeepChild("ThinLine").gameObject;
            }

            // this ensures objectmanipulator and billboard components are set
            GetComponentInParent<PoiEditor>().UpdateManipulationOptions(gameObject);

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
            if (width > 0)
            {
                _width = width;
            }
            if (height > 0)
            {
                _height = height;
            }
            useExternalSource = useExternalImageSource;

            // Create image viewer screen        
            MeshFilter meshFilter = Background.GetComponent<MeshFilter>();
            //meshFilter.mesh = CreatePlaneMesh ();
            MeshRenderer renderer = Background.GetComponent<MeshRenderer>();
            renderer.material.shader = Shader.Find("Unlit/Texture");

            if (useExternalSource == true)
            {
                StartCoroutine(nameof(LoadImage));
            }
            else
            {
                // If the image name has a suffix, remove it
                if (imageName.EndsWith(".png") || imageName.EndsWith(".jpg"))
                {
                    imageName = imageName.Substring(0, imageName.Length - 4);
                }
                Texture2D imageTex = Resources.Load(imageName, typeof(Texture2D)) as Texture2D;
                renderer.sharedMaterial.SetTexture("_MainTex", imageTex);
            }
        }


        private IEnumerator LoadImage()
        {
            MeshRenderer renderer = Background.GetComponent<MeshRenderer>();
            if (imageName.StartsWith("http") == false)
            {
                string dataPath = Application.persistentDataPath;
                string completeImageName = "file://" + dataPath + "/" + imageName;
                Debug.Log("Trying to load image from:" + completeImageName);
                WWW www = new WWW(completeImageName);
                yield return www;
                Texture2D imageTex = new Texture2D(4, 4, TextureFormat.DXT1, false);
                www.LoadImageIntoTexture(imageTex);
                renderer.sharedMaterial.SetTexture("_MainTex", imageTex);
            }
            else
            {
                // Online files stored locally.
                var url = imageName.Split('/');
                var filename = url[url.Length - 1];

                var completeImageName = "file://" + ActivityManager.Instance.Path + "/" + filename;

                Debug.Log("Trying to load image from:" + completeImageName);

                WWW www = new WWW(completeImageName);
                yield return www;
                Texture2D imageTex = new Texture2D(4, 4, TextureFormat.DXT1, false);
                www.LoadImageIntoTexture(imageTex);
                renderer.sharedMaterial.SetTexture("_MainTex", imageTex);

                // Online files.
                /*
                WWW www = new WWW (imageName);
                yield return www;
                Texture2D imageTex = new Texture2D (4, 4, TextureFormat.DXT1, false);
                www.LoadImageIntoTexture (imageTex);
                renderer.sharedMaterial.SetTexture ("_MainTex", imageTex);
                */
            }

        }


        private void setOrientation(GameObject activeFrame, GameObject unusedFrame, GameObject background){

            activeFrame.SetActive(true);
            //set selected frame

            unusedFrame.SetActive(false);
            unusedFrame.transform.localScale = new Vector3(0, 0, 0);
            unusedFrame.transform.localPosition = activeFrame.transform.localPosition;
            //set unused frame to not active and resize/relocate as to not affect the object bounding box

            Background = background;

            BoundsControl boundsControl = gameObject.GetComponent<BoundsControl>();
            boundsControl.enabled = false;
            boundsControl.enabled = true;
            //required to reset the bounding boxes of the frame used so that it displays correctly
        }

        /// <summary>
        /// Create a simple 2-triangle rectangle mesh in standing up position
        /// </summary>
        //private Mesh CreatePlaneMesh ()
        //{
        //    Mesh m = new Mesh ();
        //    m.name = "PlaneMesh";
        //    m.vertices = new Vector3 [] {
        //    new Vector3( _width/2f, -_height/2f, 0 ),
        //    new Vector3( -_width/2f, -_height/2f, 0 ),
        //    new Vector3( -_width/2f, _height/2f, 0 ),
        //    new Vector3( _width/2f, _height/2f, 0 )
        //};
        //    m.uv = new Vector2 [] {
        //    new Vector2 (1, 0),
        //    new Vector2 (0, 0),
        //    new Vector2 (0, 1),
        //    new Vector2 (1, 1)
        //};
        //    m.triangles = new int [] { 0, 1, 2, 0, 2, 3 };
        //    m.RecalculateNormals ();

        //    return m;
        //}





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
                //GetComponent<Billboard>().enabled = false;
                //transform.position = _contentPanel.position;
                //transform.rotation = _contentPanel.rotation;
                transform.localScale = Vector3.one * 0.35f;
            }

            //else
            //GetComponent<Billboard>().enabled = true;
        }

        private void OnDestroy()
        {
            if (_contentObject != null)
                Destroy(_contentObject);
        }
    }
}