using i5.Toolkit.Core.ServiceCore;
using Microsoft.MixedReality.Toolkit.Experimental.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

namespace MirageXR
{
    /// <summary>
    /// WorkplaceParser. Used for parsing Arlem workplace file files 
    /// and for handling all the resources defined in the file.
    /// </summary>
    public class WorkplaceManager : MonoBehaviour
    {
        [SerializeField] private GameObject taskStationPrefab;
        [SerializeField] private GameObject anchorCubePrefab;

        // Containers for all the main types.
        private Transform _thingContainer;

        private Transform _placeContainer;
        private Transform _personContainer;
        private Transform _detectableContainer;
        private Transform _sensorContainer;

        public static WorkplaceManager Instance { get; private set; }

        // List of predicate symbols.
        private static List<Sprite> _predicates = new List<Sprite>();

        // List of ISO7010 symbols.
        private static List<Sprite> _iso7010s = new List<Sprite>();

        // Device user id.
        private static string _userID;

        // Frame / configuration pair for anchor calibration.
        [Serializable]
        public struct AnchorCalibrationPair
        {
            public GameObject AnchorFrame;
            public Detectable DetectableConfiguration;
        }

        // List of calibration pairs.
        public List<AnchorCalibrationPair> _calibrationPairs = new List<AnchorCalibrationPair>();

        [Tooltip("Scaling factor required to get content scale to match 1m = 1 unit scale.")]
        public static float ScalingFactor = 0.25f;

        [Tooltip("Instantiation of the workplace file.")]
        public Workplace Workplace;

        public GameObject MirageXRSensorManager;


        private void Awake()
        {
            Instance = this;
        }

        private Transform floorTarget;

        // On start...
        private void Start()
        {
            // At least TRY to clear out the cache!
            Caching.ClearCache();
            //CalibrationTool.Instance.SetPlayer();
        }

        private void OnEnable()
        {
            // Register to event manager events
            EventManager.OnParseWorkplace += ParseWorkplace;
            EventManager.OnPlayerReset += ClearPois;
            EventManager.OnClearAll += PlayerReset;
        }

        private void OnDisable()
        {
            // Unregister from event manager events
            EventManager.OnParseWorkplace -= ParseWorkplace;
            EventManager.OnPlayerReset -= ClearPois;
            EventManager.OnClearAll -= PlayerReset;
        }

        /// <summary>
        /// Reset workplace manager when OnPlayerReset event is triggered.
        /// </summary>
        private void PlayerReset()
        {
            //MirageXRSensorManager.SetActive(false);
            Workplace = null;
            _userID = null;
            _calibrationPairs.Clear();
            //RealTimeSensorManager.Instance.IsActive = false;
        }

        private void ClearPois()
        {
            EventManager.ClearPois();
        }

        // Called from event manager
        private void ParseWorkplace(string workplaceId)
        {
            StartCoroutine(LoadWorkspace(workplaceId));
        }

        /// <summary>
        /// Parses the workplace file JSON. Called from the event manager.
        /// </summary>
        /// <param name="workplaceId">ID of the workplace file JSON file.</param>
        private IEnumerator LoadWorkspace(string workplaceId)
        {
            var errorCount = 0;

            // Get the containers
            _thingContainer = GameObject.Find("Things").transform;
            _placeContainer = GameObject.Find("Places").transform;
            _personContainer = GameObject.Find("Persons").transform;
            _detectableContainer = GameObject.Find("Detectables").transform;
            _sensorContainer = GameObject.Find("Sensors").transform;

            // Get the _predicates
            _predicates = Resources.LoadAll<Sprite>("predicates").ToList();

            // Get the ISO7010s
            _iso7010s = Resources.LoadAll<Sprite>("iso7010").ToList();

            // empty string => create new workplace
            if (string.IsNullOrEmpty(workplaceId))
            {
                Workplace = new Workplace();
            }
            // For loading from resources
            else if (workplaceId.StartsWith("resources://"))
            {
                var asset = Resources.Load(workplaceId.Replace("resources://", "")) as TextAsset;
                yield return new WaitForSeconds(0.1f);

                // Create workplace file object from json
                try
                {
                    // For loading from resources
                    Workplace = JsonUtility.FromJson<Workplace>(asset.text);
                }
                catch (Exception e)
                {
                    EventManager.DebugLog("Error: Workplace manager: Parsing: Couldn't parse the Workplace json: " + e);
                    errorCount++;
                    throw;
                }
            }

            // For loading from application path.
            else
            {
                if (workplaceId.StartsWith("http"))
                {
                    var fullname = workplaceId.Split('/');
                    workplaceId = fullname[fullname.Length - 1];
                }

                if (!workplaceId.EndsWith(".json"))
                    workplaceId += ".json";

                var url = File.ReadAllText(Path.Combine(Application.persistentDataPath, workplaceId));

                try
                {
                    Workplace = JsonUtility.FromJson<Workplace>(url);
                }
                catch (Exception e)
                {
                    EventManager.DebugLog("Error: Workplace manager: Couldn't load the workplace file: " + e);
                    errorCount++;
                    throw;
                }
            }

            // If all good...
            if (errorCount.Equals(0))
                // Create workplace objects.
                CreateObjects();
            else
                EventManager.DebugLog("Error: Workplace manager: Couldn't load the workplace file.");
        }

        // Create all the objects defined in workplace file.
        private async void CreateObjects()
        {
            EventManager.DebugLog("Workplace manager: Starting to create the objects...");
            int errorCount = 0;

            /*
            VestService vestService = ServiceManager.GetService<VestService>();
            // Check if we should enable the vest communications.
            if (vestService.VestEnabled)
            {
                var sensor = vestService.VestConfig;

                try
                {
                    // Check for unique object name.
                    if (GameObject.Find(sensor.id))
                        throw new AmbiguousMatchException(sensor.id + " id already in use.");

                    // Create an empty sensor object by using the helper function.
                    var temp = Utilities.CreateObject(sensor.id, _sensorContainer);

                    // Create sensors based on protocol.
                    switch (sensor.uri.Split(':')[0])
                    {
                        case "mqtt":

                            var initSuccess = false;

                            switch (sensor.type)
                            {
                                case "human":
                                case "environment":
                                    RealTimeSensorManager.Instance.IsActive = true;
                                    initSuccess = await temp.AddComponent<HumEnvMqttBehaviour>().Init(sensor);
                                    break;
                                case "device":
                                    initSuccess = await temp.AddComponent<DeviceMqttBehaviour>().Init(sensor);
                                    break;
                            }

                            // If mqtt initialization fails...
                            if (!initSuccess)
                                // DIE!
                                Destroy(temp);

                            break;
                        default:
                            throw new ArgumentException(sensor.id + " uri doesn't contain a valid protocol: " +
                                                        sensor.uri.Split(':')[0] + ".");
                    }
                }
                catch (Exception e)
                {
                    errorCount++;
                    Maggie.Speak("Error while trying to connect to sensor " + sensor.id);
                    EventManager.DebugLog("Error: Workplace manager: Couldn't create sensor objects.");
                    Debug.Log(e);
                    throw;
                }
            }

            */



            // Instantiate detectables first, since they need to be there when others want to attach to them.
            foreach (var detectable in Workplace.detectables)
            {
                try
                {
                    CreateDetectableObject(detectable, false);
                }
                catch (Exception e)
                {
                    errorCount++;
                    EventManager.DebugLog("Error: Workplace manager: Couldn't create detectables.");
                    Debug.Log(e);
                    throw;
                }
            }

            EventManager.DebugLog("Workplace manager: Detectables created.");

            // Continue only if everything is ok.
            if (errorCount.Equals(0))
            {
                // Instantiate device sensors.
                foreach (var sensor in Workplace.sensors)
                {
                    try
                    {
                        // Check for unique object name.
                        if (GameObject.Find(sensor.id))
                            throw new AmbiguousMatchException(sensor.id + " id already in use.");

                        // Create an empty sensor object by using the helper function.
                        var temp = Utilities.CreateObject(sensor.id, _sensorContainer);

                        // Create sensors based on protocol.
                        switch (sensor.uri.Split(':')[0])
                        {
                            case "mqtt":

                                var initSuccess = false;

                                switch (sensor.type)
                                {
                                    case "human":
                                    case "environment":
                                        RealTimeSensorManager.Instance.IsActive = true;
                                        initSuccess = await temp.AddComponent<HumEnvMqttBehaviour>().Init(sensor);
                                        break;
                                    case "device":
                                        initSuccess = await temp.AddComponent<DeviceMqttBehaviour>().Init(sensor);
                                        break;
                                }

                                // If mqtt initialization fails...
                                if (!initSuccess)
                                    // DIE!
                                    Destroy(temp);

                                break;
                            default:
                                throw new ArgumentException(sensor.id + " uri doesn't contain a valid protocol: " +
                                                            sensor.uri.Split(':')[0] + ".");
                        }
                    }
                    catch (Exception e)
                    {
                        errorCount++;
                        Maggie.Speak("Error while trying to connect to sensor " + sensor.id);
                        EventManager.DebugLog("Error: Workplace manager: Couldn't create sensor objects.");
                        Debug.Log(e);
                        throw;
                    }
                }
            }

            // Continue only if everything is ok.
            if (errorCount.Equals(0))
            {
                // Instantiate thing objects
                foreach (var thing in Workplace.things)
                {
                    try
                    {
                        // Check for unique object name
                        if (GameObject.Find(thing.id))
                            throw new AmbiguousMatchException(thing.id + " id already in use.");

                        // Create an empty thing object by using the helper function
                        var temp = Utilities.CreateObject(thing.id, _thingContainer);

                        var counter = 0;

                        // Instantiate poi objects
                        foreach (var poi in thing.pois)
                        {
                            // Just to check if default poi is defined.
                            if (poi.id == "default")
                            {
                                counter++;
                            }

                            var poiTemp = Utilities.CreateObject(poi.id, temp.transform);

                            // If offset not defined as separate values...
                            if (poi.x_offset.Equals(0) && poi.y_offset.Equals(0) && poi.z_offset.Equals(0))
                            {
                                // Use the CSV format if available.
                                if (!string.IsNullOrEmpty(poi.offset))
                                    poiTemp.transform.localPosition = Utilities.ParseStringToVector3(poi.offset);
                            }

                            // Parse offset from separate values.
                            else
                            {
                                poiTemp.transform.localPosition =
                                    new Vector3(poi.x_offset, poi.y_offset, poi.z_offset);
                            }

                            if (!string.IsNullOrEmpty(poi.rotation))
                                poiTemp.transform.localEulerAngles = Utilities.ParseStringToVector3(poi.rotation);
                        }

                        // Create default poi if not already defined.
                        if (counter == 0)
                        {
                            var poiTemp = Utilities.CreateObject("default", temp.transform);
                            PopulateTaskStation(poiTemp);
                        }

                        // Add guide line.
                        var guide = Instantiate(Resources.Load<GameObject>("Pathway"), Vector3.zero,
                            Quaternion.identity);

                        // Make guide line a child of default poi object.
                        guide.transform.SetParent(temp.transform.FindDeepChild("default"));

                        PathSegmentsController segmentsController = guide.GetComponent<PathSegmentsController>();
                        segmentsController.startTransform = ServiceManager.GetService<ActivitySelectionSceneReferenceService>().References.floorTarget;
                        segmentsController.endTransform = guide.transform.parent;

                        guide.GetComponent<PathRoleController>().Role = PathRole.TASKSTATION;

                        // Hide by default.
                        guide.SetActive(false);

                        // Get the thing behaviour component attached to the empty game object by the helper function
                        temp.AddComponent<ThingBehaviour>();

                        // Don't go any further if the thing doesn't have any detectables attached...
                        if (string.IsNullOrEmpty(thing.detectable))
                        {
                            continue;
                        }

                        // Go through all the available detectables...
                        foreach (Transform detectable in _detectableContainer)
                        {
                            // Don't go any further if not the detectable defined for the thing...
                            if (detectable.name != thing.detectable)
                            {
                                continue;
                            }

                            // Get detectable behaviour of the detectable...
                            var detectableBehaviour = detectable.gameObject.GetComponent<DetectableBehaviour>();

                            // Attach this thing to the detectable defined in the workplace file
                            detectableBehaviour.AttachedObject = temp;

                            // Set raw tracking style.
                            detectableBehaviour.Style = DetectableBehaviour.TrackingStyle.Raw;


                            detectableBehaviour.IsDetectableReady = true;
                        }

                        // Check if there is a sensor attached.
                        if (string.IsNullOrEmpty(thing.sensor))
                            continue;

                        // If sensor attached, go through the sensor container to find a match.
                        foreach (Transform sensor in _sensorContainer)
                        {
                            // Only interested in matching ids...
                            if (sensor.name != thing.sensor)
                                continue;

                            // Check if thing has a sensor poi defined...
                            var sensorPoi = GameObject.Find(thing.id + "/sensor");
                            if (sensorPoi != null)
                            {
                                // If sensor poi found, link sensor display to sensor poi.
                                sensor.GetComponent<DeviceMqttBehaviour>().LinkDisplay(sensorPoi.transform);
                                Debug.Log("Sensor poi");
                            }

                            else
                            {
                                // If sensor poi not found, link to default poi.
                                var defaultPoi = GameObject.Find(thing.id + "/default");
                                sensor.GetComponent<DeviceMqttBehaviour>().LinkDisplay(defaultPoi.transform);
                                Debug.Log("Default poi");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        errorCount++;
                        EventManager.DebugLog("Error: Workplace manager: Couldn't create thing object: " + thing.id);
                        Debug.Log(e);
                        throw;
                    }
                }

                EventManager.DebugLog("Workplace manager: Thing objects created.");

                // Instantiate place objects
                foreach (var place in Workplace.places)
                {
                    try
                    {
                        CreatePlaceObject(place);
                    }
                    catch (Exception e)
                    {
                        errorCount++;
                        EventManager.DebugLog("Error: Workplace manager: Couldn't create place objects.");
                        Debug.Log(e);
                        throw;
                    }
                }

                EventManager.DebugLog("Workplace manager: Place objects created.");

                // Instantiate person objects
                foreach (var person in Workplace.persons)
                {
                    try
                    {
                        // Check for unique object name
                        if (GameObject.Find(person.id))
                            throw new AmbiguousMatchException(person.id + " id already in use.");

                        // Create an empty thing object by using the helper function
                        var temp = Utilities.CreateObject(person.id, _personContainer);

                        var counter = 0;

                        // Instantiate poi objects
                        foreach (var poi in person.pois)
                        {
                            // Just to check if default poi is defined.
                            if (poi.id == "default")
                            {
                                counter++;
                            }

                            var poiTemp = Utilities.CreateObject(poi.id, temp.transform);

                            // If offset not defined as separate values...
                            if (poi.x_offset.Equals(0) && poi.y_offset.Equals(0) && poi.z_offset.Equals(0))
                            {
                                // Use the CSV format if available.
                                if (!string.IsNullOrEmpty(poi.offset))
                                    poiTemp.transform.localPosition = Utilities.ParseStringToVector3(poi.offset);
                            }

                            // Parse offset from separate values.
                            else
                            {
                                poiTemp.transform.localPosition =
                                    new Vector3(poi.x_offset, poi.y_offset, poi.z_offset);
                            }

                            if (!string.IsNullOrEmpty(poi.rotation))
                                poiTemp.transform.localEulerAngles = Utilities.ParseStringToVector3(poi.rotation);
                        }

                        // Create default poi if not already defined.
                        if (counter == 0)
                        {
                            var poiTemp = Utilities.CreateObject("default", temp.transform);
                            PopulateTaskStation(poiTemp);
                        }

                        // Add guide line.
                        var guide = Instantiate(Resources.Load<GameObject>("Pathway"), Vector3.zero,
                            Quaternion.identity);

                        // Make guide line a child of default poi object.
                        guide.transform.SetParent(temp.transform.FindDeepChild("default"));

                        PathSegmentsController segmentsController = guide.GetComponent<PathSegmentsController>();
                        segmentsController.startTransform = ServiceManager.GetService<ActivitySelectionSceneReferenceService>().References.floorTarget;
                        segmentsController.endTransform = guide.transform.parent;

                        // TODO: change this to find the actual next task station
                        guide.GetComponent<PathRoleController>().Role = PathRole.TASKSTATION;

                        // Hide by default.
                        guide.SetActive(false);

                        // Get the thing behaviour component attached to the empty game object by the helper function
                        temp.AddComponent<PersonBehaviour>();

                        // Don't go any further if the thing doesn't have any detectables attached...
                        if (string.IsNullOrEmpty(person.detectable))
                        {
                            continue;
                        }

                        // Go through all the available detectables...
                        foreach (Transform detectable in _detectableContainer)
                        {
                            // Don't go any further if not the detectable defined for the thing...
                            if (detectable.name != person.detectable)
                            {
                                continue;
                            }

                            // Get detectable behaviour of the detectable...
                            var detectableBehaviour = detectable.gameObject.GetComponent<DetectableBehaviour>();

                            // Attach this thing to the detectable defined in the workplace file
                            detectableBehaviour.AttachedObject = temp;

                            // Set raw tracking style.
                            detectableBehaviour.Style = DetectableBehaviour.TrackingStyle.Raw;

                            detectableBehaviour.IsDetectableReady = true;
                        }
                    }
                    catch (Exception e)
                    {
                        errorCount++;
                        EventManager.DebugLog("Error: Workplace manager: Couldn't create person objects.");
                        Debug.Log(e);
                        throw;
                    }
                }

                // Attach user to current device.
                foreach (var device in Workplace.devices)
                {
                    try
                    {
                        // Each device should be attached to user.
                        if (string.IsNullOrEmpty(device.owner))
                            throw new ArgumentException("Device owner not set.");

                        // Handle only the devices matching current device id
                        if (SystemInfo.deviceUniqueIdentifier != device.id)
                            continue;

                        // When a match has been found, set user id.
                        _userID = device.owner;
                    }
                    catch (Exception e)
                    {
                        errorCount++;
                        EventManager.DebugLog("Error: Workplace manager: Couldn't attach user to current device.");
                        Debug.Log(e);
                        throw;
                    }
                }

                // If all good...
                if (errorCount != 0)
                {
                    EventManager.DebugLog($"Error: Workplace manager: Couldn't parse the workflow file {Workplace.id}.");
                }
                else
                {
                    // If workplace has anchors which have not been calibrated...
                    if (_calibrationPairs.Count > 0 && !UiManager.Instance.IsCalibrated)
                    {
                        Debug.Log("Workplace has uncalibrated anchors. Please re-run the calibration");
                    }

                    Debug.Log("********** EventManager.WorkplaceParsed");
                    EventManager.WorkplaceParsed();
                }
            }
        }

        /// <summary>
        /// Get predicate symbol sprite.
        /// </summary>
        /// <param name="id">Id of the predicate.</param>
        /// <returns>Returns null if sprite not found.</returns>
        public static Sprite GetPredicate(string id)
        {
            Sprite output = null;

            foreach (var predicate in _predicates)
            {
                if (id == predicate.name)
                    output = predicate;
            }

            return output;
        }

        /// <summary>
        /// Get ISO7010 symbol sprite.
        /// </summary>
        /// <param name="id">Id of the ISO7010 symbol.</param>
        /// <returns>Returns null if sprite not found.</returns>
        public static Sprite GetIso7010(string id)
        {
            foreach (var symbol in _iso7010s)
            {
                if (id == symbol.name)
                    return symbol;
            }

            return null;
        }

        /// <summary>
        /// Get current device user id.
        /// </summary>
        /// <returns>Returns the id or "anonymous" if user not defined.</returns>
        public static string GetUser()
        {
            if (string.IsNullOrEmpty(_userID))
                return "anonymous";

            return _userID;
        }

        /// <summary>
        /// Calibrate workplace anchors.
        /// </summary>
        /// <param name="origin">Origin transform from the calibration target.</param>
        public void CalibrateAnchors(Transform origin)
        {
            if (ActivityManager.Instance.EditModeActive)
            {
                StartCoroutine(PerformEditModeCalibration());
            }
            else
            {
                StartCoroutine(PerformPlayModeCalibration(origin));
            }

            ActivityManager.Instance.StartActivity();
        }


        private IEnumerator PerformEditModeCalibration()
        {
            Debug.Log("Edit Mode Calibration started.\n");

            // cache the world anchor manager since we use it a lot in this section
            //WorldAnchorManager worldAnchorManagerInstance = ServiceManager.GetService<WorldAnchorService>().Manager;
            //string[] anchorStoreIds = worldAnchorManagerInstance.AnchorStore.GetAllIds();
        

            foreach (var pair in _calibrationPairs)
            {

                /*
                // Check if anchor is already stored...
                if (anchorStoreIds.Contains(pair.DetectableConfiguration.id))
                {
                    if (!GameObject.Find(pair.AnchorFrame.name))
                        // ...if so, attach it to anchor frame...
                        worldAnchorManagerInstance.AttachAnchor(pair.AnchorFrame, pair.DetectableConfiguration.id);

                    // ...just so we can now remove it!
                    worldAnchorManagerInstance.RemoveAnchor(pair.AnchorFrame);

                    Debug.Log("added and removed world anchor");

                    // Add a small delay...
                    yield return new WaitForSeconds(0.2f);
                }
                */

                Vector3[] relativePose = GetPoseRelativeToCalibrationOrigin(pair.AnchorFrame);
                Vector3 myPos = relativePose[0];
                Vector3 myRot = relativePose[1];

                Detectable detectable = pair.DetectableConfiguration;

                // set detectable values
                detectable.origin_position = $"{myPos.x.ToString(CultureInfo.InvariantCulture)}, " +
                                            $"{myPos.y.ToString(CultureInfo.InvariantCulture)}, " +
                                            $"{myPos.z.ToString(CultureInfo.InvariantCulture)}";
                detectable.origin_rotation = $"{myRot.x.ToString(CultureInfo.InvariantCulture)}, " +
                                            $"{myRot.y.ToString(CultureInfo.InvariantCulture)}, " +
                                            $"{myRot.z.ToString(CultureInfo.InvariantCulture)}";

                //worldAnchorManagerInstance.AttachAnchor(pair.AnchorFrame, pair.DetectableConfiguration.id);
            }

            yield return new WaitForSeconds(1f);

            UiManager.Instance.IsCalibrated = true;
            Maggie.Speak("Workplace configuration saved.");

            //delete calibration animation guide
            var calibrationGuide = GameObject.Find("CalibrationGuide");
            if (calibrationGuide)
                Destroy(calibrationGuide);

            Debug.Log("Edit mode calibration completed.");

            // commented out as, on HoloLens 1, the marker is no longer shown
            //CalibrationTool.Instance.Reset();

        }

        // here we are writing anchors for calibration pairs that (must) already exist, relative to a calibration origin.
        private IEnumerator PerformPlayModeCalibration(Transform calibrationRoot)
        {
            Debug.Log("Play Mode Calibration started.\n");

            foreach (var pair in _calibrationPairs)
            {
                Debug.Log("Handling " + pair.DetectableConfiguration.id);

                /*
                // cache the world anchor manager since we use it a lot in this section
                WorldAnchorManager worldAnchorManagerInstance = ServiceManager.GetService<WorldAnchorService>().Manager;

                // Check if anchor is already stored...
                if (worldAnchorManagerInstance.AnchorStore.GetAllIds().Contains(pair.DetectableConfiguration.id))
                {
                    if (!GameObject.Find(pair.AnchorFrame.name))
                        // ...if so, attach it to anchor frame...
                        worldAnchorManagerInstance.AttachAnchor(pair.AnchorFrame, pair.DetectableConfiguration.id);

                    // ...just so we can now remove it!
                    worldAnchorManagerInstance.RemoveAnchor(pair.AnchorFrame);
                }

                // Add a small delay...
                yield return new WaitForSeconds(0.2f);

                */
                // Create a temporary empty frame.
                var dummy = new GameObject("AnchorDummy");

                // Make sure that the scale is 1:1.
                dummy.transform.localScale = Vector3.one;

                // Place the frame to calibration root position.
                dummy.transform.position = calibrationRoot.position;

                // Get calibration root rotation.
                var originRotation = calibrationRoot.rotation;

                // Apply calibration rotation to dummy frame.
                dummy.transform.rotation = originRotation;

                // Place anchor frame into the dummy frame.
                pair.AnchorFrame.transform.SetParent(dummy.transform);

                // Set anchor frame to proper position and orientation.
                pair.AnchorFrame.transform.localEulerAngles =
                    Utilities.ParseStringToVector3(pair.DetectableConfiguration.origin_rotation);

                pair.AnchorFrame.transform.localPosition =
                    Utilities.ParseStringToVector3(pair.DetectableConfiguration.origin_position);

                // Update position to also the tangible attached to this anchor.
                pair.AnchorFrame.GetComponent<DetectableBehaviour>().AttachAnchor();

                // Now move the frame back to detectable container.
                pair.AnchorFrame.transform.SetParent(_detectableContainer);

                // Now attach anchor to the detectable anchor frame.
                //worldAnchorManagerInstance.AttachAnchor(pair.AnchorFrame, pair.DetectableConfiguration.id);

                Debug.Log("Anchor " + pair.AnchorFrame.name + " created at " + pair.AnchorFrame.transform.position + " || " + pair.AnchorFrame.transform.eulerAngles + ".");

                // Destroy dummy.
                Destroy(dummy);
            }


            // Add a small delay just to make sure all the anchors are stored...
            yield return new WaitForSeconds(1f);

            EventManager.PlayerCalibration();
            //UiManager.Instance.IsCalibrated = true;
            Maggie.Speak("Workplace is now calibrated.");

            //delete calibration animation guide
            var calibrationGuide = GameObject.Find("CalibrationGuide");
            if (calibrationGuide)
                Destroy(calibrationGuide);

            Debug.Log("Play mode calibration completed.");

            // commented out as, on HoloLens 1, the marker is no longer shown
            //CalibrationTool.Instance.Reset();
        }



        /// <summary>
        /// Returns a 2-entry array, containing the position [0] and euler angles [1] of an object, relative to the calibration origin.
        /// </summary>
        /// <param name="objectOfInterest"></param>
        /// <returns></returns>
        private Vector3[] GetPoseRelativeToCalibrationOrigin(GameObject objectOfInterest)
        {
            // Setup initial values
            Transform calibrationOrigin = CalibrationTool.Instance.transform;
            Transform originalParent = objectOfInterest.transform.parent;

            // Create a temporary empty frame.
            var dummy = new GameObject("CalibrationDummy");

            // Make sure that the scale is 1:1.
            dummy.transform.localScale = Vector3.one;

            // Place the frame to calibration root position.
            dummy.transform.position = calibrationOrigin.position;

            // Temporarily move the object under the calibration marker to determine its relative position
            objectOfInterest.transform.SetParent(calibrationOrigin);

            // Store the relative pose
            Vector3 relativePosition = objectOfInterest.transform.localPosition;
            Vector3 relativeOrientation = objectOfInterest.transform.localEulerAngles;

            // Revert the object back to its original parent
            objectOfInterest.transform.SetParent(originalParent);

            // Destroy dummy object
            Destroy(dummy);

            // Return relative position
            return new Vector3[] { relativePosition, relativeOrientation };
        }

        private void PopulateTaskStation(GameObject parent)
        {
            
            GameObject instance = Instantiate(Resources.Load("Prefabs/PlayerTaskStation") as GameObject, parent.transform);
            //instance.transform.localPosition = PlatformManager.Instance.GetTSPosition();
            instance.transform.localPosition = Vector3.zero + Camera.main.transform.right * 0.8f;
            instance.transform.localRotation = Quaternion.identity;
        }

        public Place GetPlaceFromTaskStationId(string id)
        {
            return Workplace.places.Find((item) => item.id == id);
        }

        public Detectable GetDetectable(Place place)
        {
            return Workplace.detectables.Find((item) => item.id == place.detectable);
        }

        public void SaveWorkplace()
        {
            var recFilePath = Path.Combine(Application.persistentDataPath, Workplace.id);

            var json = JsonUtility.ToJson(Workplace);
            File.WriteAllText(recFilePath, json);
        }

        public void AddPlace(Action newAction, Vector3 targetPosition, bool hasMarker = false)
        {
            Place place = new Place()
            {
                id = newAction.id,
                name = "",
                detectable = "WA-" + newAction.id.Substring(3),
            };
            Detectable detectable = new Detectable()
            {
                id = place.detectable,
                sensor = "",
                url = "",
                type = "anchor"
            };

            if (hasMarker)
            {
                detectable.type = "marker";
                return;
            }

            Workplace.detectables.Add(detectable);
            Workplace.places.Add(place);

            CreateDetectableObject(detectable, true);
            CreatePlaceObject(place);
        }

        public void AddAnnotation(Action action, ToggleObject toggleObject, Vector3 position)
        {
            try 
            {
                Place place = GetPlaceFromTaskStationId(action.id);

                Poi poi = new Poi()
                {
                    id = toggleObject.poi,
                    x_offset = position.x,
                    y_offset = position.y,
                    z_offset = position.z,
                    offset = $"{position.x.ToString(CultureInfo.InvariantCulture)}, {position.y.ToString(CultureInfo.InvariantCulture)}, {position.z.ToString(CultureInfo.InvariantCulture)}",
                    rotation = "0, 0, 0"
                };

                CreatePoiObject(poi, GameObject.Find(place.id).transform);

                place.pois.Add(poi);
            }catch (Exception e)
            {
                Debug.LogError(e);
            }

        }

        public void DeleteAnnotation(Action action, ToggleObject toggleObject)
        {
            Place place = GetPlaceFromTaskStationId(action.id);


            Poi poi = place.pois.Find((item) => item.id == toggleObject.poi);
            if (poi != null)
            {
                Destroy(GameObject.Find(poi.id));
                place.pois.Remove(poi);
            }

            if (toggleObject.predicate == "imagemarker")
            {
                Detectable detectable = Instance.GetDetectable(Instance.GetPlaceFromTaskStationId(toggleObject.id));

                GameObject detectableObj = GameObject.Find(detectable.id);
                GameObject detectableParentObj = GameObject.Find("Detectables");

                //as Vuforia dosent allow image markers to be destroyed at run time the detectable is moved instead leaving the marker still in the scene but removeing its content
                detectableObj.transform.parent = detectableParentObj.transform;
            }
        }

        private void CreateDetectableObject(Detectable detectable, bool newObject)
        {
            // Don't recreate existing detectables...
            if (GameObject.Find(detectable.id))
            {
                return;
            }

            Debug.Log("Creating Detectable Object:" + detectable.id + 
                "\nPosition:" + detectable.origin_position + 
                "\nRotation:" + detectable.origin_rotation);

            switch (detectable.type)
            {
                // Hololens world anchors.
                case "anchor":
                    // Create anchor frame
                    //GameObject anchorFrameOld = Instantiate(anchorCubePrefab, Vector3.zero,
                    //    Quaternion.identity, _detectableContainer);

                    GameObject anchorFrame = Instantiate(new GameObject("anchorCubePrefab"), Vector3.zero,
                        Quaternion.identity, _detectableContainer);

                    anchorFrame.name = detectable.id;

                    if (newObject)
                    {
                        // Set transform.
                        Vector3 startingPoint = PlatformManager.Instance.GetTaskStationPosition();
                        anchorFrame.transform.localPosition = startingPoint;
                        anchorFrame.transform.localEulerAngles = Vector3.zero;
                        anchorFrame.transform.localScale = Vector3.one;


                        // retrieve pose relative to calibration origin
                        Vector3[] relPose = GetPoseRelativeToCalibrationOrigin(anchorFrame);
                        Vector3 myPos = relPose[0];
                        Vector3 myRot = relPose[1];


                        // set detectable values
                        detectable.origin_position = $"{myPos.x.ToString(CultureInfo.InvariantCulture)}, {myPos.y.ToString(CultureInfo.InvariantCulture)}, {myPos.z.ToString(CultureInfo.InvariantCulture)}";
                        detectable.origin_rotation = $"{myRot.x.ToString(CultureInfo.InvariantCulture)}, {myRot.y.ToString(CultureInfo.InvariantCulture)}, {myRot.z.ToString(CultureInfo.InvariantCulture)}";
                    }
                    else
                    {
                        anchorFrame.transform.localPosition = Utilities.ParseStringToVector3(detectable.origin_position);
                        anchorFrame.transform.localEulerAngles = Utilities.ParseStringToVector3(detectable.origin_rotation);
                        anchorFrame.transform.localScale = Vector3.one;
                    }


                    // If the calibration has been completed...
                    //if (UiManager.Instance.IsCalibrated)
                    //{
                        //ServiceManager.GetService<    >().Manager.AttachAnchor(anchorFrame, detectable.id);
                    //}


                    // And finally add and configure detectable behaviour
                    var anchorBehaviour = anchorFrame.AddComponent<DetectableBehaviour>();
                    anchorBehaviour.Type = DetectableBehaviour.TrackableType.Anchor;
                    anchorBehaviour.IsDetectableReady = true;

                    // Add to the list of calibratable objects and attach the task station
                    var pair = new AnchorCalibrationPair();
                    pair.AnchorFrame = anchorFrame;
                    pair.DetectableConfiguration = detectable;

                    _calibrationPairs.Add(pair);

                    break;

                // Vuforia image targets.
                case "image":
                    // Path to local storage.
                    var path = Application.persistentDataPath + "/" + Workplace.id + "/detectables/" +
                               detectable.id + "/" + detectable.id + ".xml";

                    Debug.Log("VUFORIA PATH: " + path);

                    // Check that we have the data set file.
                    if (!DataSet.Exists(path, VuforiaUnity.StorageType.STORAGE_ABSOLUTE))
                        throw new FileNotFoundException(detectable.id + " data set not found.");

                    var objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
                    var dataSet = objectTracker.CreateDataSet();

                    // Try to load the data set.
                    if (!dataSet.Load(path, VuforiaUnity.StorageType.STORAGE_ABSOLUTE))
                        throw new FileLoadException(detectable.id + " data set couldn't be loaded.");

                    // Activate data set
                    if (!objectTracker.ActivateDataSet(dataSet))
                        throw new FileLoadException(detectable.id + " data set couldn't be activated.");

                    // Create the actual image target object...

                    // First get all the available trackable behaviours...
                    var trackableBehaviours = TrackerManager.Instance.GetStateManager()
                        .GetTrackableBehaviours();

                    // Then loop through all the available trackable behaviours
                    foreach (var trackableBehaviour in trackableBehaviours)
                    {
                        // Handle only trackables that are a part of this data set
                        if (!dataSet.Contains(trackableBehaviour.Trackable))
                        {
                            continue;
                        }

                        // Create the image target...
                        var imageTarget = trackableBehaviour.gameObject;


                        // Name it after the detectable id...
                        imageTarget.name = detectable.id;
                        imageTarget.tag = "VuforiaTarget";

                        // Place it into the container
                        imageTarget.transform.SetParent(_detectableContainer);

                        // Set transform
                        imageTarget.transform.localPosition = Vector3.zero;
                        imageTarget.transform.localEulerAngles = Vector3.zero;
                        imageTarget.transform.localScale = Vector3.one;

                        // Add necessary Vuforia components
                        imageTarget.AddComponent<DefaultTrackableEventHandler>();
                        imageTarget.AddComponent<TurnOffBehaviour>();

                        // Add origin
                        var originObj = new GameObject("Origin");
                        originObj.transform.SetParent(imageTarget.transform);

                        // Set origin transform
                        originObj.transform.localPosition =
                            Utilities.ParseStringToVector3(detectable.origin_position);
                        originObj.transform.localEulerAngles =
                            Utilities.ParseStringToVector3(detectable.origin_rotation);
                        originObj.transform.localScale = Vector3.one;

                        // Used for a Vuforia specific hack. A renderer enabled
                        // state can be used for quickly checking if the Vuforia
                        // tracked object is found or not because by default all
                        // the renderers in children are enabled when the object is found and
                        // disabled when the object is lost. So this is why an empty game object
                        // with just a mesh renderer component is created.
                        var rendererCheck = new GameObject("RendererCheck");
                        rendererCheck.AddComponent<MeshRenderer>();

                        // Place the renderer state checker inside the image target object
                        rendererCheck.transform.SetParent(imageTarget.transform);
                        rendererCheck.transform.localPosition = Vector3.zero;
                        rendererCheck.transform.localEulerAngles = Vector3.zero;
                        rendererCheck.transform.localScale = Vector3.one;

                        // And finally add and configure detectable behaviour
                        var detectableBehaviour = imageTarget.AddComponent<DetectableBehaviour>();
                        detectableBehaviour.Type = DetectableBehaviour.TrackableType.Image;
                        detectableBehaviour.Dataset = dataSet;
                        detectableBehaviour.IsDetectableReady = true;
                    }

                    break;
                default:
                    throw new ArgumentException(detectable.id + " unknown detectable type: " +
                                                detectable.type + ".");
            }
        }

        private void CreatePlaceObject(Place place)
        {
            // Check for unique object name
            if (GameObject.Find(place.id))
                throw new AmbiguousMatchException(place.id + " id already in use.");

            // Create an empty thing object by using the helper function
            var temp = Utilities.CreateObject(place.id, _placeContainer);

            var counter = 0;

            // Instantiate poi objects
            foreach (var poi in place.pois)
            {
                // Just to check if default poi is defined.
                if (poi.id == "default")
                {
                    counter++;
                }

                CreatePoiObject(poi, temp.transform);
            }

            // Create default poi if not already defined.
            if (counter == 0)
            {
                var poiTemp = Utilities.CreateObject("default", temp.transform);
                PopulateTaskStation(poiTemp);
            }

            // Add guide line.
            var guide = Instantiate(Resources.Load<GameObject>("Pathway"), Vector3.zero,
                Quaternion.identity);

            // Make guide line a child of default poi object.
            guide.transform.SetParent(temp.transform.FindDeepChild("default"));

            PathSegmentsController segmentsController = guide.GetComponent<PathSegmentsController>();
            segmentsController.startTransform = ServiceManager.GetService<ActivitySelectionSceneReferenceService>().References.floorTarget;
            segmentsController.endTransform = guide.transform.parent.GetChild(0);

            guide.GetComponent<PathRoleController>().Role = PathRole.TASKSTATION;

            // Hide by default.
            guide.SetActive(false);

            // Get the thing behaviour component attached to the empty game object by the helper function
            PlaceBehaviour placeBehaviour = temp.AddComponent<PlaceBehaviour>();
            placeBehaviour.Place = place;

            // Don't go any further if the thing doesn't have any detectables attached...
            if (string.IsNullOrEmpty(place.detectable))
            {
                return;
            }

            // Go through all the available detectables...
            foreach (Transform detectable in _detectableContainer)
            {
                // Don't go any further if not the detectable defined for the thing...
                if (detectable.name != place.detectable)
                {
                    continue;
                }

                // Get detectable behaviour of the detectable...
                var detectableBehaviour = detectable.gameObject.GetComponent<DetectableBehaviour>();

                // Attach this thing to the detectable defined in the workplace file
                detectableBehaviour.AttachedObject = temp;

                // Set raw tracking style.
                detectableBehaviour.Style = DetectableBehaviour.TrackingStyle.Raw;

                detectableBehaviour.IsDetectableReady = true;
            }
        }

         private void CreatePoiObject(Poi poi, Transform parent)
        {
            var poiTemp = Utilities.CreateObject(poi.id, parent);
            poiTemp.AddComponent<PoiEditor>().Initialize(poi);

            // If offset not defined as separate values...
            if (poi.x_offset.Equals(0) && poi.y_offset.Equals(0) && poi.z_offset.Equals(0))
            {
                // Use the CSV format if available.
                if (!string.IsNullOrEmpty(poi.offset))
                    poiTemp.transform.localPosition = Utilities.ParseStringToVector3(poi.offset);
            }

            // Parse offset from separate values.
            else
            {
                poiTemp.transform.localPosition =
                    new Vector3(poi.x_offset, poi.y_offset, poi.z_offset);

            }

            if (!string.IsNullOrEmpty(poi.rotation))
            {
                if (Utilities.TryParseStringToVector3(poi.rotation, out Vector3 poiVal))
                {
                    poiTemp.transform.localEulerAngles = poiVal;
                }
                else
                {
                    Debug.LogError("Problem interpreting rotation value");
                }
            }
            
            if (!string.IsNullOrEmpty(poi.scale))
            {
                if (Utilities.TryParseStringToVector3(poi.scale, out Vector3 poiVal))
                {
                    poiTemp.transform.localScale = poiVal;
                }
                else
                {
                    Debug.LogError("Problem interpreting poi scale value");
                }
            }
            
        }

    }
}