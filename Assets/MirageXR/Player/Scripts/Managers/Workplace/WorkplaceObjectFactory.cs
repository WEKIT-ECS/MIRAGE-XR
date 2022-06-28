using i5.Toolkit.Core.ServiceCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using Vuforia;
using Object = UnityEngine.Object;

namespace MirageXR
{
    public static class WorkplaceObjectFactory
    {
        private static WorkplaceManager workplaceManager => RootObject.Instance.workplaceManager;
        // *** OBJECT FACTORIES ***

        /// <summary>
        /// Helper method that creates detectable or place objects.
        /// </summary>
        /// <returns>Returns the number of errors that occured while creating the objects.</returns>
        public static async Task CreateDetectablesOrPlaces<T>(List<T> list, string debug)
        {
            foreach (var element in list)
            {
                try
                {
                    if (element is Detectable)
                    {
                        CreateDetectableObject((Detectable)(object)element, false);
                    }
                    else if (element is Place)
                    {
                        await CreatePlaceObject((Place)(object)element);
                    }
                }
                catch (Exception e)
                {
                    EventManager.DebugLog($"Error: Workplace manager: Couldn't create {debug}.");
                    Debug.Log(e);
                    throw;
                }
            }
            EventManager.DebugLog($"Workplace manager: {debug} created.");
        }

        /// <summary>
        /// Helper method that creates sensor objects.
        /// </summary>
        /// <returns>Returns the number of errors that occured while creating the objects.</returns>
        public static async void CreateSensors()
        {
            foreach (var sensor in workplaceManager.workplace.sensors)
            {
                try
                {
                    // Check for unique object name.
                    if (GameObject.Find(sensor.id))
                        throw new AmbiguousMatchException(sensor.id + " id already in use.");

                    // Create an empty sensor object by using the helper function.
                    var temp = Utilities.CreateObject(sensor.id, workplaceManager.sensorContainer);

                    if (!sensor.uri.Contains(':'))
                    {
                        throw new FormatException(sensor.uri + " is not in the correct format.");
                    }

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

                            if (!initSuccess)
                                Object.Destroy(temp);

                            break;
                        default:
                            throw new ArgumentException(sensor.id + " uri doesn't contain a valid protocol: " +
                                                        sensor.uri.Split(':')[0] + ".");
                    }
                }
                catch (Exception e)
                {
                    Maggie.Speak("Error while trying to connect to sensor " + sensor.id);
                    EventManager.DebugLog("Error: Workplace manager: Couldn't create sensor objects.");
                    Debug.Log(e);
                    throw;
                }
            }
            EventManager.DebugLog("Workplace manager: Sensor objects created.");
        }

        /// <summary>
        /// Helper method that creates thing objects.
        /// </summary>
        /// <returns>Returns the number of errors that occured while creating the objects.</returns>
        public static async Task CreateThings()
        {
            foreach (var thing in workplaceManager.workplace.things)
            {
                try
                {
                    // Check for unique object name
                    if (GameObject.Find(thing.id))
                        throw new AmbiguousMatchException(thing.id + " id already in use.");

                    // Create an empty thing object by using the helper function
                    var temp = Utilities.CreateObject(thing.id, workplaceManager.thingContainer);

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
                        await PopulateTaskStation(poiTemp);
                    }

                    // Add guide line.
                    var guide = Object.Instantiate(Resources.Load<GameObject>("Pathway"), Vector3.zero, Quaternion.identity);

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
                    foreach (Transform detectable in workplaceManager.detectableContainer)
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
                    foreach (Transform sensor in workplaceManager.sensorContainer)
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
                    EventManager.DebugLog("Error: Workplace manager: Couldn't create thing object: " + thing.id);
                    Debug.Log(e);
                    throw;
                }
            }
            EventManager.DebugLog("Workplace manager: Thing objects created.");
        }

        /// <summary>
        /// Helper method that creates person objects.
        /// </summary>
        /// <returns>Returns the number of errors that occured while creating the objects.</returns>
        public static async Task CreatePersons()
        {
            foreach (var person in workplaceManager.workplace.persons)
            {
                try
                {
                    // Check for unique object name
                    if (GameObject.Find(person.id))
                        throw new AmbiguousMatchException(person.id + " id already in use.");

                    // Create an empty thing object by using the helper function
                    var temp = Utilities.CreateObject(person.id, workplaceManager.personContainer);

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
                        await PopulateTaskStation(poiTemp);
                    }

                    // Add guide line.
                    var guide = Object.Instantiate(Resources.Load<GameObject>("Pathway"), Vector3.zero, Quaternion.identity);

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
                    foreach (Transform detectable in workplaceManager.detectableContainer)
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
                    EventManager.DebugLog("Error: Workplace manager: Couldn't create person objects.");
                    Debug.Log(e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Helper method that creates device objects.
        /// </summary>
        /// <returns>Returns the number of errors that occured while creating the objects.</returns>
        public static void CreateDevices()
        {
            foreach (var device in workplaceManager.workplace.devices)
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
                    WorkplaceManager.userID = device.owner;
                }
                catch (Exception e)
                {
                    EventManager.DebugLog("Error: Workplace manager: Couldn't attach user to current device.");
                    Debug.Log(e);
                    throw;
                }
            }
        }

        // *** SINGLE OBJECT CREATION METHODS ***

        public static void CreateDetectableObject(Detectable detectable, bool newObject)
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
                    GameObject anchorFrame = Object.Instantiate(new GameObject("anchorCubePrefab"), Vector3.zero,
                        Quaternion.identity, workplaceManager.detectableContainer);

                    anchorFrame.name = detectable.id;

                    if (newObject)
                    {
                        // Set transform.
                        Vector3 startingPoint = PlatformManager.Instance.GetTaskStationPosition();
                        anchorFrame.transform.localPosition = startingPoint;
                        anchorFrame.transform.localEulerAngles = Vector3.zero;
                        anchorFrame.transform.localScale = Vector3.one;


                        // retrieve pose relative to calibration origin
                        (Vector3, Vector3) relPose = GetPoseRelativeToCalibrationOrigin(anchorFrame);
                        Vector3 myPos = relPose.Item1;
                        Vector3 myRot = relPose.Item2;


                        // set detectable values
                        detectable.origin_position = Utilities.Vector3ToString(myPos);
                        detectable.origin_rotation = Utilities.Vector3ToString(myRot);
                    }
                    else
                    {
                        anchorFrame.transform.localPosition = Utilities.ParseStringToVector3(detectable.origin_position);
                        anchorFrame.transform.localEulerAngles = Utilities.ParseStringToVector3(detectable.origin_rotation);
                        anchorFrame.transform.localScale = Vector3.one;
                    }

                    // And finally add and configure detectable behaviour
                    var anchorBehaviour = anchorFrame.AddComponent<DetectableBehaviour>();
                    anchorBehaviour.Type = DetectableBehaviour.TrackableType.Anchor;
                    anchorBehaviour.IsDetectableReady = true;

                    // Add to the list of calibratable objects and attach the task station
                    var pair = new WorkplaceManager.AnchorCalibrationPair();
                    pair.AnchorFrame = anchorFrame;
                    pair.DetectableConfiguration = detectable;

                    workplaceManager.calibrationPairs.Add(pair);

                    break;

                // Vuforia image targets.
                case "image":
                    // Path to local storage.
                    string[] paths = { Application.persistentDataPath, workplaceManager.workplace.id, "/detectables/",
                                      detectable.id, detectable.id, ".xml" };
                    var path = Path.Combine(paths);

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
                        imageTarget.transform.SetParent(workplaceManager.detectableContainer);

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
                    throw new ArgumentException($"{detectable.id} unknown detectable type: {detectable.type}.");
            }
        }

        public static async Task CreatePlaceObject(Place place)
        {
            // Check for unique object name
            if (GameObject.Find(place.id))
            {
                throw new AmbiguousMatchException(place.id + " id already in use.");
            }

            // Create an empty thing object by using the helper function
            var temp = Utilities.CreateObject(place.id, workplaceManager.placeContainer);

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
                await PopulateTaskStation(poiTemp);
            }

            // Add guide line.
            var guide = Object.Instantiate(Resources.Load<GameObject>("Pathway"), Vector3.zero, Quaternion.identity);

            // Make guide line a child of default poi object.
            guide.transform.SetParent(temp.transform.FindDeepChild("default"));

            var segmentsController = guide.GetComponent<PathSegmentsController>();
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
            foreach (Transform detectable in workplaceManager.detectableContainer)
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

        public static void CreatePoiObject(Poi poi, Transform parent)
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
                poiTemp.transform.localPosition = new Vector3(poi.x_offset, poi.y_offset, poi.z_offset);
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

        // *** HELPER METHODS ***

        private const float offsetFromPlayer = 0.8f;

        /// <summary>
        /// Returns a 2-entry array, containing the position [0] and euler angles [1] of an object, relative to the calibration origin.
        /// </summary>
        /// <param name="objectOfInterest"></param>
        /// <returns></returns>
        public static (Vector3, Vector3) GetPoseRelativeToCalibrationOrigin(GameObject objectOfInterest)
        {
            // Setup initial values
            Transform calibrationOrigin = CalibrationTool.Instance.transform;
            Transform originalParent = objectOfInterest.transform.parent;

            // Create a temporary empty frame.
            var dummy = new GameObject("CalibrationDummy");     //TODO: it seems to be useless

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
            Object.Destroy(dummy);
            return (relativePosition, relativeOrientation);
        }

        private static async Task PopulateTaskStation(GameObject parent)
        {
            var prefab = await ReferenceLoader.GetAssetReferenceAsync<GameObject>("PlayerTaskStation");
            var instance = Object.Instantiate(prefab, parent.transform);

            //only for the first taskstation in this step move it to the right of the player
            var taskStationPos = RootObject.Instance.activityManager.ActionsOfTypeAction.Count == 0 ? Camera.main.transform.right * offsetFromPlayer : Vector3.zero;

            var isFirstTaskStation = PlatformManager.Instance.WorldSpaceUi && RootObject.Instance.activityManager.EditModeActive;
            instance.transform.localPosition = isFirstTaskStation ? taskStationPos : Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
        }
    }
}
