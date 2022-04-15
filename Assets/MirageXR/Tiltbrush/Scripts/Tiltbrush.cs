using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;
using static TiltBrush.SketchControlsScript;

namespace TiltBrush
{
    //This script was made to handle the order of execution of scripts
    //Instead of using Unity's script execution order
    public class Tiltbrush : MonoBehaviour
    {
        public static Tiltbrush Instance;

        //Only a single instance of this class can be instantiated. Which means this class has to be shared by multiple entities
        //This keeps track of gameobjects that need the Tiltbrush prefab in order to delete it when it's not needed.
        //Use SubscribeComponent and UnsubscribeComponent
        private List<MonoBehaviour> usedBy = new List<MonoBehaviour>();
        private bool destroyInitiated;
        //Time after which TIltbrush gets destroyed. In seconds
        private float destroyDelay = 5f;
        private float timeBeforeDestroy;

        public App app;
        public PointerManager pointerManager;
        public QualityControls qualityControls;
        public BrushCatalog brushCatalog;
        public TextMeasureScript textMeasureScript;
        public BrushController brushController;
        public SketchControlsScript sketchControlsScript;
        public SelectionManager selectionManager;
        public SketchSurfacePanel sketchSurfacePanel;
        public CustomColorPaletteStorage customColorPaletteStorage;
        public InputManager inputManager;
        public ViewpointScript viewpointScript;

        public GameObject panels;

        private SimpleSnapshot snapshot;

        void Awake()
        {
            if(Instance != null && Instance != this){
                Destroy(this);
                return;
            }

            Instance = this;

            var instantiateTilt = GetComponentInParent<InstantiateTiltbrush>();

            //Assumes that the MainCamera is set and the playspace is the direct parent of the camera
            var camera = CameraCache.Main;
            var playspace = camera.transform.parent;

            app.Init();
            pointerManager.Init();
            App.VrSdk.Init(playspace.gameObject, camera);
            qualityControls.Init();
            textMeasureScript.Init();
            brushController.Init();
            brushCatalog.Init();
            customColorPaletteStorage.Init();

            sketchControlsScript.Init();

            selectionManager.Init();
            sketchSurfacePanel.Init();
            inputManager.Init();
            viewpointScript.Init();
        }

        public void Destroy()
        {
            //These need to be destroyed first to avoid race conditions
            App.VrSdk.PreDestroy();
            Destroy(gameObject);
        }

        public SimpleSnapshot ImportSnapshotFromFile(string path)
        {
            SaveLoadScript.m_Instance.Load(new DiskSceneFileInfo(path));
            var res = new SimpleSnapshot();
            ClearScene();
            return res;
        }

        public SimpleSnapshot GetNewSnapshot()
        {
            return new SimpleSnapshot();
        }

        //This will create a new snapshot and save it
        //If we already have a snapshot from GetNewSnapshot, we can directly save that snapshot with snapshot.WriteToFile
        public async void ExportFile(string path)
        {
            SketchSnapshot snapshot = await SaveLoadScript.m_Instance.CreateSnapshotWithIconsAsync();
            snapshot.WriteSnapshotToFile(path);
        }

        public void SetViewOnly(bool viewOnly)
        {
            if (viewOnly == sketchControlsScript.IsCommandActive(GlobalCommands.ViewOnly))
                return;

            panels.SetActive(!viewOnly);
            sketchControlsScript.IssueGlobalCommand(GlobalCommands.ViewOnly);
        }

        public void ClearScene()
        {
            sketchControlsScript.IssueGlobalCommand(GlobalCommands.NewSketch);

            SketchMemoryScript.m_Instance.EndDrawingFromMemory();
        }

        public void SetOrigin(Vector3 position, Quaternion rotation)
        {
            App.ActiveCanvas.Pose = new TrTransform() { translation = position, rotation = rotation, scale = 1f };
        }

        private void OnDestroy(){
            Instance = null;
        }
    
        private void Update(){
            if(destroyInitiated){
                timeBeforeDestroy -= Time.deltaTime;
                if(timeBeforeDestroy <= 0){
                    Destroy();
                    destroyInitiated = false;
                }
            }
        }

        public void SubscribeComponent(MonoBehaviour component){
            //we don't want to have duplicates
            if(!usedBy.Contains(component))
                usedBy.Add(component);

            destroyInitiated = false;
        }

        
        public void UnsubscribeComponent(MonoBehaviour component){
            usedBy.Remove(component);
            //Destroy Tiltbrush if 0 component is using it
            if(usedBy.Count == 0){
                destroyInitiated = true;
                timeBeforeDestroy = destroyDelay;
            }
        }
    }
}
