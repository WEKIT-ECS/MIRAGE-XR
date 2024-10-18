using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
        WireframeVignette = 0,
        TwinVignette = 1,
        FullTwin = 2
    }

    /// <summary>
    /// RoomTwinManager is responsible for loading, calibrating, and configuring
    /// the 3D model of a remote room (the 'room twin'). This uses the RoomShader
    /// to unveil the model.
    /// </summary>
    public class RoomTwinManager : MonoBehaviour
    {

        private GltfImport _gltf;
        private bool _loadingCompleted = false;
        private bool _blendInCompleted = false;
        private bool _growVignettesCompleted = false;

        [SerializeField]
        public bool ForceRoomTwinDisplay;

        [SerializeField]
        public string RoomFile = "esa_iss_columbus_module_1m.glb";

        [SerializeField]
        public Material RoomTwinShader;

        private RoomTwinStyle _roomTwinStyle = RoomTwinStyle.WireframeVignette;

        private GltfImport gltf;
        private GameObject _roomModel;
        private Animation legacyAnimation;

        #region lerp
        Color StartColor;
        Color EndColor;
        Color lerpColor;

        static float t = 0.0f;
        // increase of alpha color value per second
        static float deltaa = 1.0f;
        static float deltawf = 0.5f;
        #endregion


        // Start is called before the first frame update
        public async Task InitializationAsync()
        {
            await LoadRoomTwinModel(Path.Combine(Application.streamingAssetsPath, RoomFile));
        }

        private void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            if (!_blendInCompleted && _loadingCompleted)
            {
                var alpha = Mathf.Lerp(0, 1, t);
                SetAlphaInChildRenderers(_roomModel, alpha);

                t += deltaa * Time.deltaTime;

                if (t > 1.0f)
                {
                    _blendInCompleted = true;
                    _growVignettesCompleted = false;

                    t = 0.0f;

                    // Add RoomShader to Child Renderers
                    AddShaderToChildRenderers(_roomModel, RoomTwinShader);

                }

            } else if (_roomTwinStyle == RoomTwinStyle.WireframeVignette && !_growVignettesCompleted && _blendInCompleted && _loadingCompleted)
            {

                var alpha = Mathf.Lerp(100, 10, t);
                GrowVignettesInChildRenderers(_roomModel, alpha);

                t += deltawf * Time.deltaTime;

                if (t > 1.0f)
                {
                    _growVignettesCompleted = true;
                    t = 0.0f;
                }

            } else if (_roomTwinStyle == RoomTwinStyle.TwinVignette && !_growVignettesCompleted && _blendInCompleted && _loadingCompleted)
            {
                Debug.Log("Twin Vignette style not yet implemented");
            }

        }

        /// <summary>
        /// Public method for loading the Room Model,
        /// calls internal LoadGltfRoomTwin
        /// </summary>
        /// <param name="url">file path or web url of the model</param>
        public async Task LoadRoomTwinModel(string url)
        {
            await LoadGltfRoomTwin(url);
            _blendInCompleted = false;
        }

        /// <summary>
        /// Switch digital room twin on or off
        /// </summary>
        /// <param name="Show">if true, display the room twin, otherwise deactivate it</param>
        public void DisplayRoomTwin(bool Show)
        {
            if (Show) // show
            {
                _roomModel.SetActive(true);
                SetRoomTwinStyle(_roomTwinStyle);
                
            } else // hide
            {
                _roomModel.SetActive(false);

                t = 0.0f;
                _blendInCompleted = true;
                _growVignettesCompleted = true;
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
            if (_roomTwinStyle == RoomTwinStyle.WireframeVignette)
            {
                t = 0.0f;
                _blendInCompleted = true;
                _growVignettesCompleted = false;
            } else if (_roomTwinStyle == RoomTwinStyle.FullTwin)
            {
                t = 0.0f;
                _blendInCompleted = false;
                _growVignettesCompleted = true;
            } else if (_roomTwinStyle == RoomTwinStyle.TwinVignette)
            {
                Debug.Log("Twin Vignette not yet implemented");
            }
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
                NodeNameMethod = NameImportMethod.OriginalUnique,
                AnimationMethod = AnimationMethod.Legacy
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
                var Col = childRenderer.material.color;
                Col.a = alpha;
                childRenderer.material.color = Col;
            }

        }

        private void AddShaderToChildRenderers(GameObject RoomTwin, Material TheShader)
        {

            RoomTwin.GetComponent<MeshRenderer>().material = TheShader; // not needed?

            Component[] renderers = RoomTwin.GetComponentsInChildren(typeof(Renderer));
            foreach (Renderer childRenderer in renderers)
            {
                childRenderer.material = TheShader;
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


    }

}