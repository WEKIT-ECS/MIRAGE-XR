using LearningExperienceEngine;
using i5.Toolkit.Core.ServiceCore;
using i5.Toolkit.Core.VerboseLogging;
using MirageXR;
using NUnit.Framework;
using System.Collections;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Tests
{
    /// <summary>
    /// Calibration tests are performed following the Arrange-Act-Assert pattern.
    /// </summary>
    [TestFixture]
    public class CalibrationTests
    {

        #region Variables

        // ARLEM models to test against
        LearningExperienceEngine.Activity testActivity;
        LearningExperienceEngine.Workplace testWorkplace;

        // World origin for dummy marker
        Transform WorldTestOrigin;

        // i5 Service Core objects
        ActivitySelectionSceneReferenceServiceConfiguration referenceServiceConfiguration;
        ActivitySelectionSceneReferenceService referenceService;

        // useful (required) scene objects
        GameObject dummyCamera;
        GameObject dummyViewPort;

        GameObject thingContainer;
        GameObject placeContainer;
        GameObject personContainer;
        GameObject detectableContainer;
        GameObject sensorContainer;
        CalibrationTool dummyCalibrationTool;

        // dummy manager objects
        UiManager dummyUiManager;
        PlatformManager dummyPlatformManager;
        TaskStationDetailMenu dummyTSDM;

        // setup progress flags
        private bool sceneLoaded;
        private bool resourcesArranged;
        private bool workplaceReady;
        private bool readyToTest;
        private bool isCalibrated;


        // number of seconds to wait for setup or tests
        private float testTimeOut = 60.0f;
        private RootObject rootObject;

        #endregion Variables


        #region SetUp and TearDown


        [OneTimeSetUp]
        public void Init()
        {
            SceneManager.LoadScene("TestScene");
        }


        [OneTimeTearDown]
        public void Outit()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            LearningExperienceEngine.EventManager.OnWorkplaceLoaded -= OnWorkplaceLoaded;

            if (ServiceManager.ServiceExists<ActivitySelectionSceneReferenceService>())
            {
                ServiceManager.RemoveService<ActivitySelectionSceneReferenceService>();
            }

            SceneManager.UnloadSceneAsync("TestScene");
        }

        [UnitySetUp]
        public IEnumerator PlayModeInit()
        {
            // just do this once (OneTimeSetUp cannot feature yielding instructions, but it is necessary to wait for certain flags)
            if (!workplaceReady)
            {
                // wait for scene to load
                float timeoutStart = Time.time;
                yield return new WaitWhile(() => sceneLoaded == false && Time.time - timeoutStart < testTimeOut);

                // arrange objects
                var task = SetupReferences();
                yield return new WaitUntil(() => task.IsCompleted);

                // wait for resources to be arranged
                timeoutStart = Time.time;
                yield return new WaitWhile(() => resourcesArranged == false && Time.time - timeoutStart < testTimeOut);

                // initialise activity parsing (first thing that needs to happen)
                task = StartDummyActivity("resources://calibrationTest-activity");
                yield return new WaitUntil(() => task.IsCompleted);

                // wait for workplace parsing (last thing that happens)
                timeoutStart = Time.time;
                yield return new WaitWhile(() => workplaceReady == false && (Time.time - timeoutStart) < testTimeOut);
            }
            else
            {
                // not the first test, do nothing
                yield return null;
            }

            readyToTest = true;
        }


        [TearDown]
        public void TearDown()
        {
            readyToTest = false;

        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            sceneLoaded = true;
        }

        private void LoadTestArlemModels()
        {
            testActivity = ActivityParser.Parse("resources://calibrationTest-activity");
            testWorkplace = WorkplaceParser.Parse("resources://calibrationTest-workplace");
        }

        #endregion SetUp and TearDown

        private async Task SetupReferences()
        {
            // set world origin
            WorldTestOrigin = new GameObject("World Origin").transform;
            WorldTestOrigin.position = new Vector3(10f, 20f, 30f);
            WorldTestOrigin.eulerAngles = new Vector3(10f, 20f, 30f);

            // create scene objects with required tags
            dummyCamera = new GameObject("Main Camera");
            dummyCamera.AddComponent<Camera>();
            dummyCamera.tag = "MainCamera";

            dummyViewPort = new GameObject("User Viewport");
            dummyViewPort.tag = "UserViewport";

            // create containers for the workplace model
            thingContainer = new GameObject("Things");
            placeContainer = new GameObject("Places");
            personContainer = new GameObject("Persons");
            detectableContainer = new GameObject("Detectables");
            sensorContainer = new GameObject("Sensors");

            // register scene selection service
            referenceServiceConfiguration = new ActivitySelectionSceneReferenceServiceConfiguration
            {
                floorTarget = dummyCamera.transform
            };
            referenceService = new ActivitySelectionSceneReferenceService(referenceServiceConfiguration);

            var prefab = await ReferenceLoader.GetAssetReferenceAsync<GameObject>("WEKITCalibrationMarker");
            dummyCalibrationTool = Object.Instantiate(prefab).GetComponent<CalibrationTool>();
            //dummyCalibrationTool.SetCalibrationModel(new GameObject("Calibration Model"));

            if (!ServiceManager.ServiceExists<ActivitySelectionSceneReferenceService>())
            {
                ServiceManager.RegisterService(referenceService);
            }

            if (!RootObject.Instance)
            {
                rootObject = GenerateGameObjectWithComponent<RootObject>("root");
                CallPrivateMethod(rootObject, "Awake");
                CallPrivateMethod(rootObject, "Initialization");
                await rootObject.WaitForInitialization();
            }
            else
            {
                rootObject = RootObject.Instance;
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
            LearningExperienceEngine.EventManager.OnWorkplaceLoaded += OnWorkplaceLoaded;

            LoadTestArlemModels();

            // audio listener cannot be added with helper function
            // FW (2024-03-27) added a check to avoid having several audio listeners in the scene
            if (!GameObject.Find("Audio Listener"))
            {
                var audioListener = new GameObject("Audio Listener"); 
                audioListener.AddComponent<AudioListener>();
            } else
            {
                var audioListener = GameObject.Find("Audio Listener");
            }

            // create and configure content managers or UI elements needed to start an activity
            GenerateGameObjectWithComponent<Maggie>("Maggie");
            GenerateGameObjectWithComponent<BrandManager>("Brand Manager");

            CreateDummyUiManager();

            CreateDummyPlatformManager();

            dummyTSDM = GenerateGameObjectWithComponent<TaskStationDetailMenu>("Taskstation Detail Menu");
            dummyTSDM.SetTaskStationMenuPanel(new GameObject("Taskstation Menu Panel"));

            // copy test activity resources to the persistent data path
            //CopyResourcesToPersistentDataPath();

            resourcesArranged = true;
        }

        private void CopyResourcesToPersistentDataPath()
        {
            string sourcePath = Path.Combine(Application.streamingAssetsPath, "UnitTesting", "calibrationTest");
            string targetPath = Path.Combine(Application.persistentDataPath, "calibrationTest");

            if (!Directory.Exists(sourcePath)) {
                Debug.LogError("Calibration testing files not found");
                return;
            }

            if (!Directory.Exists(targetPath)){
                Directory.CreateDirectory(targetPath);
            }

            DirectoryInfo sourceDir = new DirectoryInfo(sourcePath);
            DirectoryInfo targetDir = new DirectoryInfo(targetPath);

            CopyWithoutMetaFiles(sourceDir, targetDir);
        }

        private void CopyWithoutMetaFiles(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                string targetFilePath = Path.Combine(target.FullName, fi.Name);

                // skip meta and existing files
                if (!File.Exists(targetFilePath) && fi.Extension != ".meta")
                {
                    fi.CopyTo(targetFilePath, false);
                }
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyWithoutMetaFiles(diSourceSubDir, nextTargetSubDir);
            }
        }

        private void CreateDummyUiManager()
        {
            // set up simple UI manager - must set calibration to true to avoid creation of calibration object
            dummyUiManager = GenerateGameObjectWithComponent<UiManager>("UiManager");
            dummyUiManager.gameObject.AddComponent<AudioSource>();
            //dummyUiManager.DebugConsole = new GameObject();
            dummyUiManager.ActionList = new GameObject();
            dummyUiManager.IsCalibrated = true;
        }

        private void CreateDummyPlatformManager()
        {
            // configure platform manager
            dummyPlatformManager = GenerateGameObjectWithComponent<PlatformManager>("Platform Manager");
        }

        private T GenerateGameObjectWithComponent<T>(string name, bool activated = true) where T : MonoBehaviour
        {
            GameObject go = new GameObject(name);
            go.SetActive(activated);
            return go.AddComponent<T>();
        }

        private Task StartDummyActivity(string activityId)
        {
            // parse activity model to begin startup process
            return LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.LoadActivity(activityId);
        }

        private void OnWorkplaceLoaded()
        {
            workplaceReady = true;
        }

        private async Task PerformCalibration(bool isEditMode)
        {
            LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.EditModeActive = isEditMode;
            await LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager.CalibrateWorkplace();
            isCalibrated = true;
        }

        /// <summary>
        /// A helper functions that checks that object have been initialised. It should be yielded at the start of each test.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private IEnumerator EnsureTestReadiness(float timeout = 60.0f)
        {
            float timeoutStart = Time.time;
            yield return new WaitWhile(() => readyToTest == false && Time.time - timeoutStart < timeout);
        }

        [UnityTest, Order(0)]
        public IEnumerator Activity_LoadResources()
        {
            yield return EnsureTestReadiness();
            Assert.IsTrue(resourcesArranged);
        }

        [UnityTest, Order(0)]
        public IEnumerator ARLEM_ParseAndStartActivity()
        {
            yield return EnsureTestReadiness();
            Assert.IsTrue(workplaceReady);
        }

        [UnityTest, Order(1)]
        public IEnumerator CalibrateAnchors_PlayerMode()
        {
            yield return EnsureTestReadiness();

            // Run the player-mode calibration routine from the Workplace Manager
            var task = PerformCalibration(false);
            yield return new WaitUntil(() => task.IsCompleted);

            // wait for calibration to finish
            float timeoutStart = Time.time;
            yield return new WaitWhile(() => isCalibrated == false && Time.time - timeoutStart < testTimeOut);

            // check calibration result
            Assert.IsTrue(isCalibrated);
        }

        //[UnityTest, Order(1)] - this will be added later, needs more work
        public IEnumerator Calibration_EditMode()
        {
            yield return EnsureTestReadiness();

            var task = PerformCalibration(true);
            yield return new WaitUntil(() => task.IsCompleted);

            float timeoutStart = Time.time;
            yield return new WaitWhile(() => isCalibrated == false && Time.time - timeoutStart < testTimeOut);

            Assert.IsTrue(isCalibrated);
        }

        [UnityTest, Order(2)]
        public IEnumerator CheckArlemFileLength_Activity()
        {
            yield return EnsureTestReadiness();

            var task = LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.LoadActivity("resources://calibrationTest-activity");
            yield return new WaitUntil(() => task.IsCompleted);

            int testFileLengthActivity = ActivityParser.Serialize(testActivity).Length;
            int actlFileLengthActivity = ActivityParser.Serialize(LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.Activity).Length;

            Assert.AreEqual(testFileLengthActivity, actlFileLengthActivity);
        }

        [UnityTest, Order(2)]
        public IEnumerator CheckArlemFileLength_Workplace()
        {
            yield return EnsureTestReadiness();

            var task = LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.LoadActivity("resources://calibrationTest-activity");
            yield return new WaitUntil(() => task.IsCompleted);

            int testFileLengthWorkplace = WorkplaceParser.Serialize(testWorkplace).Length;
            int actlFileLengthWorkplace = WorkplaceParser.Serialize(LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager.workplace).Length;

            Assert.AreEqual(testFileLengthWorkplace, actlFileLengthWorkplace);
        }

        [UnityTest, Order(3)]
        public IEnumerator CountWorkplaceObjects_Things()
        {
            yield return EnsureTestReadiness();

            int testThingCount = testWorkplace.things.Count;
            int actlThingCount = thingContainer.transform.childCount;

            Assert.AreEqual(testThingCount, actlThingCount);
        }

        [UnityTest, Order(3)]
        public IEnumerator CountWorkplaceObjects_Places()
        {
            yield return EnsureTestReadiness();

            int testPlaceCount = testWorkplace.places.Count;
            int actlPlaceCount = placeContainer.transform.childCount;

            Assert.AreEqual(testPlaceCount, actlPlaceCount);
        }

        [UnityTest, Order(3)]
        public IEnumerator CountWorkplaceObjects_Detectables()
        {
            yield return EnsureTestReadiness();

            int testDetectCount = testWorkplace.detectables.Count;
            int actlDetectCount = detectableContainer.transform.childCount;

            Assert.AreEqual(testDetectCount, actlDetectCount);
        }

        [UnityTest, Order(4)]
        public IEnumerator FocusOnDetectables_CheckLocalPositions_x()
        {
            yield return EnsureTestReadiness();

            for (int d = 0; d < LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager.workplace.detectables.Count; d++)
            {
                var testDetectablePosition = LearningExperienceEngine.Utilities.ParseStringToVector3(testWorkplace.detectables[d].origin_position).x;
                var position = detectableContainer.transform.GetChild(d).localPosition;
                var actualLocalPositionComponent = position.x;

                Assert.AreEqual(testDetectablePosition, actualLocalPositionComponent, 0.001f);
            }
        }

        [UnityTest, Order(4)]
        public IEnumerator FocusOnDetectables_CheckLocalPositions_y()
        {
            yield return EnsureTestReadiness();

            for (int d = 0; d < LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager.workplace.detectables.Count; d++)
            {
                var testDetectablePosition = LearningExperienceEngine.Utilities.ParseStringToVector3(testWorkplace.detectables[d].origin_position).y;
                var position = detectableContainer.transform.GetChild(d).localPosition;
                var actualLocalPositionComponent = position.y;

                Assert.AreEqual(testDetectablePosition, actualLocalPositionComponent, 0.001f);
            }
        }

        [UnityTest, Order(4)]
        public IEnumerator FocusOnDetectables_CheckLocalPositions_z()
        {
            yield return EnsureTestReadiness();

            for (int d = 0; d < LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager.workplace.detectables.Count; d++)
            {
                float testDetectablePosition = LearningExperienceEngine.Utilities.ParseStringToVector3(testWorkplace.detectables[d].origin_position).z;
                var position = detectableContainer.transform.GetChild(d).localPosition;
                var actualLocalPositionComponent = position.z;

                Assert.AreEqual(testDetectablePosition, actualLocalPositionComponent, 0.001f);
            }
        }

        [UnityTest, Order(5)]
        public IEnumerator FocusOnDetectables_CheckLocalRotations()
        {
            yield return EnsureTestReadiness();

            for (int d = 0; d < LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager.workplace.detectables.Count; d++)
            {
                var testDetectableRotation = LearningExperienceEngine.Utilities.ParseStringToVector3(testWorkplace.detectables[d].origin_rotation);
                var localRotation = detectableContainer.transform.GetChild(d).localRotation;
                var actualDetectableRotation = localRotation.eulerAngles;
                Assert.IsTrue(LearningExperienceEngine.Utilities.EulerAnglesAreTheSame(testDetectableRotation, actualDetectableRotation, 0.01f));
            }
        }

        [UnityTest, Order(6)]
        public IEnumerator FocusOnPois_CheckLocalPositions_x()
        {
            yield return EnsureTestReadiness();

            for (int place = 0; place < LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager.workplace.places.Count; place++)
            {
                // get next place
                Transform placeObject = placeContainer.transform.GetChild(place);

                for (int poi = 0; poi < LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager.workplace.places[place].pois.Count; poi++)
                {
                    // find relevant value in test arlem script
                    float testPoiPosition = testWorkplace.places[place].pois[poi].x_offset;

                    // calculate actual local position
                    float actualLocalPoiComponent = placeObject.GetChild(poi).localPosition.x;

                    // check equality
                    Assert.AreEqual(testPoiPosition, actualLocalPoiComponent, 0.001f);
                }
            }
        }


        [UnityTest, Order(6)]
        public IEnumerator FocusOnPois_CheckLocalPositions_y()
        {
            yield return EnsureTestReadiness();

            for (int place = 0; place < LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager.workplace.places.Count; place++)
            {
                // get next place
                Transform placeObject = placeContainer.transform.GetChild(place);

                for (int poi = 0; poi < LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager.workplace.places[place].pois.Count; poi++)
                {
                    // find relevant value in test arlem script
                    float testPoiPosition = testWorkplace.places[place].pois[poi].y_offset;

                    // calculate actual local position
                    float actualLocalPoiComponent = placeObject.GetChild(poi).localPosition.y;

                    // check equality with tolerance
                    Assert.AreEqual(testPoiPosition, actualLocalPoiComponent, 0.001f);
                }
            }
        }

        [UnityTest, Order(6)]
        public IEnumerator FocusOnPois_CheckLocalPositions_z()
        {
            yield return EnsureTestReadiness();

            for (int place = 0; place < LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager.workplace.places.Count; place++)
            {
                // get next place
                Transform placeObject = placeContainer.transform.GetChild(place);

                for (int poi = 0; poi < LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager.workplace.places[place].pois.Count; poi++)
                {
                    // find relevant value in test arlem script
                    float testPoiPosition = testWorkplace.places[place].pois[poi].z_offset;

                    // calculate actual local position
                    float actualLocalPoiComponent = placeObject.GetChild(poi).localPosition.z;

                    // check equality with tolerance
                    Assert.AreEqual(testPoiPosition, actualLocalPoiComponent, 0.001f);
                }
            }
        }


        [UnityTest, Order(7)]
        public IEnumerator FocusOnPois_CheckLocalRotations()
        {
            yield return EnsureTestReadiness();

            for (int place = 0; place < LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager.workplace.places.Count; place++)
            {
                Transform placeTransform = placeContainer.transform.GetChild(place);

                for (int poi = 0; poi < LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager.workplace.places[place].pois.Count; poi++)
                {
                    // set expected value
                    Poi poiObject = testWorkplace.places[place].pois[poi];
                    var expectedPoiRotation = LearningExperienceEngine.Utilities.ParseStringToVector3(poiObject.rotation);

                    // check that poi gameobject exists
                    Transform poiTransform = placeTransform.Find(poiObject.id);
                    Assert.IsNotNull(poiTransform);

                    // discover actual rotation component value
                    var actualPoiRotation = poiTransform.localEulerAngles;

                    // check the euler angular separation (cannot just compare numbers!)
                    Assert.IsTrue(LearningExperienceEngine.Utilities.EulerAnglesAreTheSame(expectedPoiRotation, actualPoiRotation, 0.1f));
                }
            }
        }


        [UnityTest, Order(8)]
        public IEnumerator FocusOnPois_CheckLocalScales_x()
        {
            yield return EnsureTestReadiness();

            for (int place = 0; place < LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager.workplace.places.Count; place++)
            {
                // get next place
                Transform placeObject = placeContainer.transform.GetChild(place);

                for (int poi = 0; poi < LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager.workplace.places[place].pois.Count; poi++)
                {
                    // find relevant value in test arlem script
                    float testPoiScale = LearningExperienceEngine.Utilities.ParseStringToVector3(testWorkplace.places[place].pois[poi].scale).x;

                    // calculate actual local position
                    float actualPoiScale = placeObject.GetChild(poi).localScale.x;

                    // check equality with tolerance
                    Assert.AreEqual(testPoiScale, actualPoiScale, 0.001f);
                }
            }
        }

        [UnityTest, Order(8)]
        public IEnumerator FocusOnPois_CheckLocalScales_y()
        {
            yield return EnsureTestReadiness();

            for (int place = 0; place < LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager.workplace.places.Count; place++)
            {
                // get next place
                Transform placeObject = placeContainer.transform.GetChild(place);

                for (int poi = 0; poi < LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager.workplace.places[place].pois.Count; poi++)
                {
                    // find relevant value in test arlem script
                    float testPoiScale = LearningExperienceEngine.Utilities.ParseStringToVector3(testWorkplace.places[place].pois[poi].scale).y;

                    // calculate actual local position
                    float actualPoiScale = placeObject.GetChild(poi).localScale.y;

                    // check equality with tolerance
                    Assert.AreEqual(testPoiScale, actualPoiScale, 0.001f);
                }
            }
        }

        [UnityTest, Order(8)]
        public IEnumerator FocusOnPois_CheckLocalScales_z()
        {
            yield return EnsureTestReadiness();

            for (int place = 0; place < LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager.workplace.places.Count; place++)
            {
                // get next place
                Transform placeObject = placeContainer.transform.GetChild(place);

                for (int poi = 0; poi < LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager.workplace.places[place].pois.Count; poi++)
                {
                    // find relevant value in test arlem script
                    float testPoiScale = LearningExperienceEngine.Utilities.ParseStringToVector3(testWorkplace.places[place].pois[poi].scale).z;

                    // calculate actual local position
                    float actualPoiScale = placeObject.GetChild(poi).localScale.z;

                    // check equality with tolerance
                    Assert.AreEqual(testPoiScale, actualPoiScale, 0.001f);
                }
            }
        }

        //[UnityTest, Order(99)]
        public IEnumerator PauseForDebug()
        {
            // make sure test is runnable
            yield return EnsureTestReadiness();

            Debug.LogInfo("press space bar to end testing");

            // pause for shutdown or debugging
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        }

        private static void CallPrivateMethod(object obj, string methodName, params object[] parameters)
        {
            var method = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(obj, parameters);
        }
    }

}
