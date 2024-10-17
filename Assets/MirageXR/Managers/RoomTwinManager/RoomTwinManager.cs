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

        public string RoomFile = "esa_iss_columbus_module_1m.glb";
        public Material RoomTwinShader;
        private RoomTwinStyle _roomTwinStyle = RoomTwinStyle.WireframeVignette;
        private GameObject _roomModel;

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
                Debug.Log("Twin Vignette not yet implemented");
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

            } else // hide
            {
                _roomModel.SetActive(false);
            }
        }

        /// <summary>
        /// Set the Room Twin style to the values of the settings panel
        /// </summary>
        /// <param name="TheStyle">a value from enum RoomTwinStyle</param>
        public void SetRoomTwinStyle(RoomTwinStyle TheStyle)
        {
            _roomTwinStyle = TheStyle;
        }

        /// <summary>
        /// Load the Room Twin model from file
        /// </summary>
        /// <param name="RoomFile"></param>
        /// <returns></returns>
        private async Task<bool> LoadGltfRoomTwin(string RoomFile)
        {

            var gltf = new GLTFast.GltfImport();

            // Create a settings object and configure it accordingly
            var settings = new ImportSettings
            {
                GenerateMipMaps = true,
                AnisotropicFilterLevel = 3,
                NodeNameMethod = NameImportMethod.OriginalUnique
            };

            // Load the glTF and pass along the settings
            var success = await gltf.Load(RoomFile, settings);

            if (success)
            {
                _roomModel = new GameObject("DigitalRoomTwinModel");
                _roomModel.AddComponent<MeshRenderer>();
                _roomModel.SetActive(false);
                await gltf.InstantiateMainSceneAsync(_roomModel.transform);
                _roomModel.transform.parent = transform;

                // prep for alpha lerp from transparent
                SetAlphaInChildRenderers(_roomModel, 0);

                // activate
                _roomModel.SetActive(true); // needs to be replaced with the default view value once we have the view hooked up
                _loadingCompleted = true;

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

