using Cysharp.Threading.Tasks;
using GLTFast;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace MirageXR
{
    /// <summary>
    /// The different styles the room twin can be set to:
    /// WireframeVignette: display the wireframe in a round vignette,
    /// TwinVignette: display see-thru VR vignette
    /// FullTwin: Full VR mode
    /// </summary>
    public enum RoomTwinStyle:ushort
    {
        TwinVignette = 1,
        FullTwin = 2
    }

    /// <summary>
    /// RoomTwinManager is responsible for loading, calibrating, and configuring
    /// the 3D model of a remote room (the 'room twin'). This uses the RoomShader
    /// to unveil the model.
    /// </summary>
    public class RoomTwinManager : MonoBehaviour, IThumbnailProvider    //TODO: correct fields name
    {
        private const string roomScanBaseEndpoint = "http://repository.wekit-ecs.com:8001/digitalTwin/";


        public bool _FullTwinBlendInCompleted = false;
        public bool _WireframeBlendInCompleted = false;

        [SerializeField] private bool ForceRoomTwinDisplay;
        [SerializeField] public string defaultScanName = "esa_iss_columbus_module_1m.glb";
        [SerializeField] private Material RoomTwinShader;
        [SerializeField] public RoomTwinStyle _roomTwinStyle = RoomTwinStyle.FullTwin;

        private LearningExperienceEngine.IAuthorizationManager _authorizationManager;
        private Dictionary<string, Texture2D> thumbnailCache = new Dictionary<string, Texture2D>();
        private static readonly SemaphoreSlim _networkLock = new SemaphoreSlim(1, 1);

        private GltfImport _gltf;
        private GameObject _roomModel;
        private Animation _legacyAnimation;

        private string ListEndpoint
        {
            get => roomScanBaseEndpoint + "list";
        }

        private string ThumbnailEndpoint
        {
            get => roomScanBaseEndpoint + "thumbnail/get";
        }

        private string ModelEndpoint
        {
            get => roomScanBaseEndpoint + "get";
        }

        #region lerp
        private Color _startColor;
        private Color _endColor;
        private Color _lerpColor;

        private bool _loadingCompleted = false;

        private static float _t = 0.0f;
        // increase of value per frame interval
        private const float DeltaA = 1.0f;
        private const float DeltaWF = 0.5f;

        #endregion

        // Start is called before the first frame update
        public async UniTask InitializationAsync(LearningExperienceEngine.IAuthorizationManager authorizationManager)
        {
            UnityEngine.Debug.Log("Initializing [RoomTwinManager] <--");

            _authorizationManager = authorizationManager;

            // load a local model first as the default
            await LoadRoomTwinModelFromURLAsync(Path.Combine(Application.streamingAssetsPath, defaultScanName));

            UnityEngine.Debug.Log("[RoomTwinManager] registering ImmersionChanged event");
#if UNITY_VISIONOS
            Unity.PolySpatial.VolumeCamera.ImmersionChanged += OnImmersionChanged;
#endif
            UnityEngine.Debug.Log("Initializing [RoomTwinManager] -->");
        }

        // Update is called once per frame
        private void Update()
        {
            if (!_loadingCompleted)
            {
                return;
            }

            switch (_roomTwinStyle)
            {
                case RoomTwinStyle.FullTwin or RoomTwinStyle.TwinVignette when !_FullTwinBlendInCompleted:   //TODO: use DOTween
                {
                    var alpha = Mathf.Lerp(0, 1, _t);
                    SetAlphaInChildRenderers(_roomModel, alpha);

                    _t += DeltaA * Time.deltaTime;

                    if (_t > 1.0f)
                    {
                        _FullTwinBlendInCompleted = true;
                        _WireframeBlendInCompleted = false;

                        _t = 0.0f;

                        // Add RoomShader to Child Renderers
                        AddShaderToChildRenderers(_roomModel, RoomTwinShader);
                    }

                    break;
                }
                case RoomTwinStyle.TwinVignette when !_WireframeBlendInCompleted:   //TODO: use DOTween
                {
                    var alpha = Mathf.Lerp(100, 10, _t);
                    GrowVignettesInChildRenderers(_roomModel, alpha);

                    _t += DeltaWF * Time.deltaTime;

                    if (_t > 1.0f)
                    {
                        _WireframeBlendInCompleted = true;
                        _t = 0.0f;
                    }

                    break;
                }
            }
        }

        public async Task<string[]> GetListOfRoomScansAsync()
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(ListEndpoint))
            {
                webRequest.SetRequestHeader("Authorization", $"Bearer {_authorizationManager.AccessToken}");
                await webRequest.SendWebRequest();
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    AvatarListReponse avatars = JsonConvert.DeserializeObject<AvatarListReponse>(webRequest.downloadHandler.text);
                    avatars.Data.Sort();
                    return avatars.Data.ToArray();
                }
                else
                {
                    Debug.LogError("Error fetching avatar list: " + webRequest.error);
                    return new string[0];
                }
            }
        }

        /// <summary>
        /// Public method for loading the Room Model,
        /// calls internal LoadGltfRoomTwin
        /// </summary>
        /// <param name="url">file path or web url of the model</param>
        public async UniTask LoadRoomTwinModelFromURLAsync(string url)
        {
            await LoadGltfRoomTwinAsync(url);
        }

        public async UniTask LoadRoomTwinModelFromId(string id)
        {
            string url = ModelEndpoint + "?digitaltwin_name=" + id;

            await LoadGltfRoomTwinAsync(url);
        }

        /// <summary>
        /// Switch digital room twin on or off
        /// </summary>
        /// <param name="show">if true, display the room twin, otherwise deactivate it</param>
        public void SetRoomTwinVisibility(bool show) //TODO: replace with two functions - ShowRoomTwin() and HideRoomTwin()
        {
            if (show) // show
            {
                //_loadingCompleted = false;
                //if (_roomModel)
                //{
                //    Destroy(_roomModel);
                //}
                SetRoomTwinStyle(_roomTwinStyle);
                //await LoadRoomTwinModelFromURLAsync(Path.Combine(Application.streamingAssetsPath, defaultScanName));
                _roomModel.SetActive(true);
                
            } else // hide
            {
                _roomModel.SetActive(false);

                _t = 0.0f;
                _FullTwinBlendInCompleted = true;
                _WireframeBlendInCompleted = true;
            }
        }

        /// <summary>
        /// Set the Room Twin style to the values of the settings panel
        /// </summary>
        /// <param name="theStyle">a value from enum RoomTwinStyle</param>
        public void SetRoomTwinStyle(RoomTwinStyle theStyle)
        {
            _roomTwinStyle = theStyle;

            switch (_roomTwinStyle)
            {
                // set up the animations in the update loop accordingly
                case RoomTwinStyle.FullTwin:
                    _t = 0.0f;
                    _FullTwinBlendInCompleted = false;
                    _WireframeBlendInCompleted = true;
                    break;
                case RoomTwinStyle.TwinVignette:
                    _t = 0.0f;
                    _FullTwinBlendInCompleted = false;
                    _WireframeBlendInCompleted = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public RoomTwinStyle GetRoomTwinStyle()
        {
            return _roomTwinStyle;
        }

        /// <summary>
        /// Load the Room Twin model from a url
        /// </summary>
        /// <param name="url"></param>
        private async UniTask<bool> LoadGltfRoomTwinAsync(string url)
        {
            _gltf = new GltfImport();

            // Create a settings object and configure it accordingly
            var settings = new ImportSettings
            {
                GenerateMipMaps = true,
                AnisotropicFilterLevel = 3,
                NodeNameMethod = NameImportMethod.OriginalUnique
                //, AnimationMethod = AnimationMethod.Legacy
            };

            // Load the glTF and pass along the settings
            var success = await _gltf.Load(url, settings);

            if (success)
            {
                if (_roomModel != null)
                {
                    Destroy(_roomModel);
                }

                _roomModel = new GameObject("DigitalRoomTwinModel");
                _roomModel.transform.parent = transform;
                _roomModel.SetActive(false);
                _roomModel.AddComponent<MeshRenderer>();

                var instantiator = new GameObjectInstantiator(_gltf, _roomModel.transform);
                await _gltf.InstantiateMainSceneAsync(instantiator); // transform

                // prep for alpha lerp from transparent
                SetAlphaInChildRenderers(_roomModel, 0);

                _legacyAnimation = instantiator.SceneInstance.LegacyAnimation;

                // activate
                _loadingCompleted = true;
                SetRoomTwinVisibility(true);

                //if (legacyAnimation != null)
                //{
                //    var clips = gltf.GetAnimationClips();
                //    if (clips != null && clips.Length > 0 && clips[0] != null)
                //    {
                //        legacyAnimation.clip = clips[0];
                //        legacyAnimation.clip.wrapMode = UnityEngine.WrapMode.Loop;
                //        legacyAnimation.clip.legacy = true;
                //        legacyAnimation.clip.EnsureQuaternionContinuity();
                //        legacyAnimation.playAutomatically = true;
                //        legacyAnimation.Play(clips[0].name);
                //    }
                //}

            }
            else
            {
                Debug.LogError("Loading glTF failed!", this);
            }

            return true;
        } // LoadGltfRoomTwin

        private static void SetAlphaInChildRenderers(GameObject roomTwin, float alpha)
        {
            var renderers = roomTwin.GetComponentsInChildren(typeof(Renderer));
            foreach (var component in renderers)
            {
                var childRenderer = (Renderer)component;
                childRenderer.material.SetOverrideTag("RenderType", "Transparent");
                childRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                childRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                childRenderer.material.SetInt("_ZWrite", 0);
                childRenderer.material.DisableKeyword("_ALPHATEST_ON");
                childRenderer.material.EnableKeyword("_ALPHABLEND_ON");
                childRenderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                childRenderer.material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

                var col = childRenderer.material.color;
                col.a = alpha;
                childRenderer.material.color = col;
            }
        }

        private static void AddShaderToChildRenderers(GameObject roomTwin, Material theShader)
        {
            //RoomTwin.GetComponent<MeshRenderer>().material = TheShader; // not needed?
            var renderers = roomTwin.GetComponentsInChildren(typeof(Renderer));
            foreach (var component in renderers)
            {
                var childRenderer = (Renderer)component;
                childRenderer.material = theShader;
                //Material[] rendererMaterials = childRenderer.materials;
                //foreach (Material mat in rendererMaterials)
                //{
                //    mat.shader = Shader.Find("Shader Graphs/LD_DigitalTwinHologram");
                //}
            }
        }

        private static void GrowVignettesInChildRenderers(GameObject roomTwin, float alpha)
        {
            var renderers = roomTwin.GetComponentsInChildren(typeof(Renderer));
            foreach (var component in renderers)
            {
                var childRenderer = (Renderer)component;
                childRenderer.material.SetFloat("_Fade_Distance", alpha);
            }
        }

        /// <summary>
        /// Hook a dimmer up with the digital crown (supported in VisionOS 2)
        /// </summary>
        private void OnImmersionChanged(double? oldAmount, double? newAmount)
        {
            Debug.LogInfo($"Immersion amount: {newAmount:P0}");
            if (!_loadingCompleted || !newAmount.HasValue)
            {
                return;
            }

            if (Math.Abs(newAmount.Value - 1.0) < 0.01d)
            {
                SetRoomTwinStyle(RoomTwinStyle.FullTwin);
                SetRoomTwinVisibility(true);
            }
            if (newAmount == 0.0d)
            {
                SetRoomTwinStyle(RoomTwinStyle.TwinVignette);
                AddShaderToChildRenderers(_roomModel, RoomTwinShader);
                SetRoomTwinVisibility(false);
            }
            else
            {
                _roomModel.SetActive(true);
                _roomTwinStyle = RoomTwinStyle.TwinVignette;
                GrowVignettesInChildRenderers(_roomModel, (float)newAmount);
            }
        }

        public async UniTask<Texture2D> GetThumbnailAsync(string scanId, CancellationToken cancellationToken = default)
        {
            if (thumbnailCache.TryGetValue(scanId, out Texture2D cachedTexture))
            {
                return cachedTexture;
            }

            await _networkLock.WaitAsync(cancellationToken);

            try
            {
                // re-check if the thumbnail was loaded in the meantime
                if (thumbnailCache.TryGetValue(scanId, out Texture2D doubleCheck))
                {
                    return doubleCheck;
                }

                string thumbnailUrl = ThumbnailEndpoint + "?digitaltwin_name=" + scanId;
                using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(thumbnailUrl))
                {
                    webRequest.SetRequestHeader("Authorization", $"Bearer {_authorizationManager.AccessToken}");
                    await webRequest.SendWebRequest().WithCancellation(cancellationToken);

                    if (webRequest.result == UnityWebRequest.Result.Success)
                    {
                        Texture2D thumbnail = DownloadHandlerTexture.GetContent(webRequest);
                        thumbnailCache[scanId] = thumbnail;
                        return thumbnail;
                    }
                    else
                    {
                        Debug.LogError("Error fetching thumbnail: " + webRequest.error);
                        return null;
                    }
                }
            }
            finally
            {
                // release network lock
                _networkLock.Release();
            }
        }
    }
}
