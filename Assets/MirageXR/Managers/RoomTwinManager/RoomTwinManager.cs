using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using GLTFast;
using LearningExperienceEngine;

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
    public class RoomTwinManager : MonoBehaviour    //TODO: correct fields name
    {

        private bool _loadingCompleted = false;

        public bool _FullTwinBlendInCompleted = false;
        public bool _WireframeBlendInCompleted = false;

        [SerializeField]
        private bool ForceRoomTwinDisplay;

        [SerializeField]
        public string RoomFile = "esa_iss_columbus_module_1m.glb";

        [SerializeField]
        private Material RoomTwinShader;

        [SerializeField]
        public RoomTwinStyle _roomTwinStyle = RoomTwinStyle.FullTwin;

        private GltfImport gltf;
        private GameObject _roomModel;
        private Animation legacyAnimation;

        #region lerp
        Color StartColor;
        Color EndColor;
        Color lerpColor;

        static float t = 0.0f;
        // increase of value per frame interval
        static float deltaa = 1.0f;
        static float deltawf = 0.5f;
        #endregion

        // Start is called before the first frame update
        public async Task InitializationAsync()
        {
            UnityEngine.Debug.Log("Initializing [RoomTwinManager] <--");
            await LoadRoomTwinModel(Path.Combine(Application.streamingAssetsPath, RoomFile));

            UnityEngine.Debug.Log("[RoomTwinManager] registering ImmersionChanged event");
            #if UNITY_VISIONOS || VISION_OS
                var VolumeCamera = GameObject.Find("/Start").GetComponent<VolumeCamera>();
                if (VolumeCamera != null) VolumeCamera.ImmersionChanged += OnImmersionChanged;
            #endif

            UnityEngine.Debug.Log("Initializing [RoomTwinManager] -->");
        }

        // Update is called once per frame
        private void Update()
        {
            if (_loadingCompleted)
            {

                if (( _roomTwinStyle == RoomTwinStyle.FullTwin || _roomTwinStyle == RoomTwinStyle.TwinVignette) && !_FullTwinBlendInCompleted)
                {
                    var alpha = Mathf.Lerp(0, 1, t);
                    SetAlphaInChildRenderers(_roomModel, alpha);

                    t += deltaa * Time.deltaTime;

                    if (t > 1.0f)
                    {
                        _FullTwinBlendInCompleted = true;
                        _WireframeBlendInCompleted = false;

                        t = 0.0f;

                        // Add RoomShader to Child Renderers
                        AddShaderToChildRenderers(_roomModel, RoomTwinShader);

                    }

                }
                else if (_roomTwinStyle == RoomTwinStyle.TwinVignette && !_WireframeBlendInCompleted)
                {

                    var alpha = Mathf.Lerp(100, 10, t);
                    GrowVignettesInChildRenderers(_roomModel, alpha);

                    t += deltawf * Time.deltaTime;

                    if (t > 1.0f)
                    {
                        _WireframeBlendInCompleted = true;
                        t = 0.0f;
                    }

                }

            }

        }

        /// <summary>
        /// Public method for loading the Room Model,
        /// calls internal LoadGltfRoomTwin
        /// </summary>
        /// <param name="url">file path or web url of the model</param>
        public async UniTask LoadRoomTwinModel(string url)
        {
            await LoadGltfRoomTwin(url);
        }

        /// <summary>
        /// Switch digital room twin on or off
        /// </summary>
        /// <param name="show">if true, display the room twin, otherwise deactivate it</param>
        public async UniTask DisplayRoomTwin(bool show) //TODO: replace with two functions - ShowRoomTwin() and HideRoomTwin()
        {
            if (show) // show
            {
                _loadingCompleted = false;
                if (_roomModel) Destroy(_roomModel);
                SetRoomTwinStyle(_roomTwinStyle);
                await LoadRoomTwinModel(Path.Combine(Application.streamingAssetsPath, RoomFile));
                _roomModel.SetActive(true);
                
            } else // hide
            {
                _roomModel.SetActive(false);

                t = 0.0f;
                _FullTwinBlendInCompleted = true;
                _WireframeBlendInCompleted = true;
            }
        }

        /// <summary>
        /// Set the Room Twin style to the values of the settings panel
        /// </summary>
        /// <param name="TheStyle">a value from enum RoomTwinStyle</param>
        public void SetRoomTwinStyle(RoomTwinStyle TheStyle)
        {
            _roomTwinStyle = TheStyle;

            // set up the animations in the update loop accordingly
            if (_roomTwinStyle == RoomTwinStyle.FullTwin)
            {
                t = 0.0f;
                _FullTwinBlendInCompleted = false;
                _WireframeBlendInCompleted = true;
            }
            else if (_roomTwinStyle == RoomTwinStyle.TwinVignette)
            {
                t = 0.0f;
                _FullTwinBlendInCompleted = false;
                _WireframeBlendInCompleted = false;
            } 
        }

        public RoomTwinStyle GetRoomTwinStyle()
        {
            return _roomTwinStyle;
        }

        /// <summary>
        /// Load the Room Twin model from file
        /// </summary>
        /// <param name="RoomFile"></param>
        /// <returns></returns>
        private async Task<bool> LoadGltfRoomTwin(string RoomFile)
        {

            gltf = new GLTFast.GltfImport();

            // Create a settings object and configure it accordingly
            var settings = new ImportSettings
            {
                GenerateMipMaps = true,
                AnisotropicFilterLevel = 3,
                NodeNameMethod = NameImportMethod.OriginalUnique
                //, AnimationMethod = AnimationMethod.Legacy
            };

            // Load the glTF and pass along the settings
            var success = await gltf.Load(RoomFile, settings);

            if (success)
            {
                _roomModel = new GameObject("DigitalRoomTwinModel");
                _roomModel.transform.parent = transform;
                _roomModel.SetActive(false);
                _roomModel.AddComponent<MeshRenderer>();

                var instantiator = new GameObjectInstantiator(gltf, _roomModel.transform);
                await gltf.InstantiateMainSceneAsync(instantiator); // transform

                

                // prep for alpha lerp from transparent
                SetAlphaInChildRenderers(_roomModel, 0);

                legacyAnimation = instantiator.SceneInstance.LegacyAnimation;

                // activate
                _loadingCompleted = true;
                DisplayRoomTwin(ForceRoomTwinDisplay); 

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
                Debug.LogError("Loading glTF failed!");
            }

            return true;

        } // LoadGltfRoomTwin

        private void SetAlphaInChildRenderers(GameObject RoomTwin, float alpha)
        {

            Component[] renderers = RoomTwin.GetComponentsInChildren(typeof(Renderer));
            foreach (Renderer childRenderer in renderers)
            {
                childRenderer.material.SetOverrideTag("RenderType", "Transparent");
                childRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                childRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                childRenderer.material.SetInt("_ZWrite", 0);
                childRenderer.material.DisableKeyword("_ALPHATEST_ON");
                childRenderer.material.EnableKeyword("_ALPHABLEND_ON");
                childRenderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                childRenderer.material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

                var Col = childRenderer.material.color;
                Col.a = alpha;
                childRenderer.material.color = Col;
            }

        }

        private void AddShaderToChildRenderers(GameObject RoomTwin, Material TheShader)
        {

            //RoomTwin.GetComponent<MeshRenderer>().material = TheShader; // not needed?

            Component[] renderers = RoomTwin.GetComponentsInChildren(typeof(Renderer));
            foreach (Renderer childRenderer in renderers)
            {
                childRenderer.material = TheShader;
                //Material[] rendererMaterials = childRenderer.materials;
                //foreach (Material mat in rendererMaterials)
                //{
                //    mat.shader = Shader.Find("Shader Graphs/LD_DigitalTwinHologram");
                //}
            }

        }

        private void GrowVignettesInChildRenderers(GameObject RoomTwin, float alpha)
        {

            Component[] renderers = RoomTwin.GetComponentsInChildren(typeof(Renderer));
            foreach (Renderer childRenderer in renderers)
            {
                childRenderer.material.SetFloat("_Fade_Distance", alpha);
            }

        }

        /// <summary>
        /// Hook a dimmer up with the digital crown (supported in VisionOS 2)
        /// </summary>
        /// <param name="amount"></param>
        void OnImmersionChanged(double amount)
        {
            Debug.LogInfo($"Immersion amount: {amount:P0}");
            if (_loadingCompleted) {
                if (amount == 1.0) {
                    SetRoomTwinStyle(RoomTwinStyle.FullTwin);
                    DisplayRoomTwin(true);
                } if (amount == 0.0) {
                    SetRoomTwinStyle(RoomTwinStyle.TwinVignette);
                    AddShaderToChildRenderers(_roomModel, RoomTwinShader);
                    DisplayRoomTwin(false);
                } else {
                    _roomModel.SetActive(true);
                    _roomTwinStyle = RoomTwinStyle.TwinVignette;
                    GrowVignettesInChildRenderers(_roomModel, (float)amount);
                }
            }
        }

    }

}
