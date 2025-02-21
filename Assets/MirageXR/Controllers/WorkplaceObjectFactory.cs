using i5.Toolkit.Core.ServiceCore;
using i5.Toolkit.Core.VerboseLogging;
using LearningExperienceEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace MirageXR
{
    public class WorkplaceObjectFactory
    {
        private LearningExperienceEngine.ActivityManager activityManager => LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld;
        private LearningExperienceEngine.WorkplaceManager workplaceManager => LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager;
        private WorkplaceController workplaceController => RootObject.Instance.WorkplaceController;

        private GameObject instance;

        public WorkplaceObjectFactory()
        {
            LearningExperienceEngine.EventManager.OnCreateDetectableObject += CreateDetectableObject;
            LearningExperienceEngine.EventManager.OnCreatePlaceObject += CreatePlaceObject;
            LearningExperienceEngine.EventManager.OnCreatePoiObject += CreatePoiObject;
        }

        ~WorkplaceObjectFactory()
        {
            LearningExperienceEngine.EventManager.OnCreateDetectableObject -= CreateDetectableObject;
            LearningExperienceEngine.EventManager.OnCreatePlaceObject -= CreatePlaceObject;
            LearningExperienceEngine.EventManager.OnCreatePoiObject -= CreatePoiObject;
        }

        public void CreateDetectables(List<LearningExperienceEngine.Detectable> list, string debug)
        {
            if (list == null || list.Count == 0)
            {
                return;
            }

            try
            {
                //var rotation = Utilities.ParseStringToVector3(list[0].origin_rotation);
                //var quaternion = Quaternion.Euler(rotation);
                //workplaceManager.detectableContainer.rotation = Quaternion.Inverse(quaternion);

                foreach (var detectable in list)
                {
                    CreateDetectableObject(detectable, false);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: Workplace manager: Couldn't create {debug}.");
                Debug.LogException(e);
            }

            Debug.LogInfo($"Workplace manager: {debug} created.");
        }

        public async Task CreatePlaces(List<LearningExperienceEngine.Place> list, string debug)
        {
            try
            {
                foreach (var element in list)
                {
                    var action = LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.Activity.actions.FirstOrDefault(t => t.id == element.id);
                    await CreatePlaceObject(element, action);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: Workplace manager: Couldn't create {debug}.");
                Debug.LogException(e);
            }

            Debug.LogInfo($"Workplace manager: {debug} created.");
        }

        /// <summary>
        /// Helper method that creates sensor objects.
        /// </summary>
        /// <returns>Returns the number of errors that occured while creating the objects.</returns>
        public async void CreateSensors()
        {
            foreach (var sensor in workplaceManager.workplace.sensors)
            {
                try
                {
                    // Check for unique object name.
                    if (GameObject.Find(sensor.id))
                        throw new AmbiguousMatchException(sensor.id + " id already in use.");

                    // Create an empty sensor object by using the helper function.
                    var temp = LearningExperienceEngine.Utilities.CreateObject(sensor.id, workplaceManager.sensorContainer);

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
                    Debug.LogError("Error: Workplace manager: Couldn't create sensor objects.");
                    Debug.LogException(e);
                    throw;
                }
            }
            Debug.LogInfo("Workplace manager: Sensor objects created.");
        }

        /// <summary>
        /// Helper method that creates thing objects.
        /// </summary>
        /// <returns>Returns the number of errors that occured while creating the objects.</returns>
        public async Task CreateThings()
        {
            foreach (var thing in workplaceManager.workplace.things)
            {
                try
                {
                    // Check for unique object name
                    if (GameObject.Find(thing.id))
                    {
                        throw new AmbiguousMatchException(thing.id + " id already in use.");
                    }

                    // Create an empty thing object by using the helper function
                    var temp = LearningExperienceEngine.Utilities.CreateObject(thing.id, workplaceManager.thingContainer);

                    var counter = 0;

                    // Instantiate poi objects
                    foreach (var poi in thing.pois)
                    {
                        // Just to check if default poi is defined.
                        if (poi.id == "default")
                        {
                            counter++;
                        }

                        var poiTemp = LearningExperienceEngine.Utilities.CreateObject(poi.id, temp.transform);

                        // If offset not defined as separate values...
                        if (poi.x_offset.Equals(0) && poi.y_offset.Equals(0) && poi.z_offset.Equals(0))
                        {
                            // Use the CSV format if available.
                            if (!string.IsNullOrEmpty(poi.offset))
                            {
                                poiTemp.transform.localPosition = Utilities.ParseStringToVector3(poi.offset);
                            }
                        }

                        // Parse offset from separate values.
                        else
                        {
                            poiTemp.transform.localPosition = new Vector3(poi.x_offset, poi.y_offset, poi.z_offset);
                        }

                        if (!string.IsNullOrEmpty(poi.rotation))
                        {
                            poiTemp.transform.localEulerAngles = Utilities.ParseStringToVector3(poi.rotation);
                        }
                    }

                    // Create default poi if not already defined.
                    if (counter == 0)
                    {
                        var poiTemp = LearningExperienceEngine.Utilities.CreateObject("default", temp.transform);
                        var action = LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.Activity.actions.FirstOrDefault(t => t.id == thing.id);
                        await PopulateTaskStation(poiTemp, action);
                    }

                    //// Add guide line.
                    //var guide = Object.Instantiate(Resources.Load<GameObject>("Pathway"), Vector3.zero, Quaternion.identity);

                    //// Make guide line a child of default poi object.
                    //guide.transform.SetParent(temp.transform.FindDeepChild("default"));

                    //PathSegmentsController segmentsController = guide.GetComponent<PathSegmentsController>();
                    //segmentsController.startTransform = ServiceManager.GetService<ActivitySelectionSceneReferenceService>().References.floorTarget;
                    //segmentsController.endTransform = guide.transform.parent;

                    //// this should be removed once the ribbons are no longer needed.
                    //guide.GetComponent<PathRoleController>().Role = PathRole.TASKSTATION;

                    //// Hide by default.
                    //guide.SetActive(false);

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

                        var detectableBehaviour = detectable.gameObject.GetComponent<DetectableBehaviour>();
                        detectableBehaviour.AttachedObject = temp;
                        detectableBehaviour.Style = DetectableBehaviour.TrackingStyle.Raw;
                        detectableBehaviour.IsDetectableReady = true;
                    }

                    // Check if there is a sensor attached.
                    if (string.IsNullOrEmpty(thing.sensor))
                    {
                        continue;
                    }

                    // If sensor attached, go through the sensor container to find a match.
                    foreach (Transform sensor in workplaceManager.sensorContainer)
                    {
                        // Only interested in matching ids...
                        if (sensor.name != thing.sensor)
                        {
                            continue;
                        }

                        // Check if thing has a sensor poi defined...
                        var sensorPoi = GameObject.Find(thing.id + "/sensor");
                        if (sensorPoi != null)
                        {
                            // If sensor poi found, link sensor display to sensor poi.
                            //sensor.GetComponent<DeviceMqttBehaviour>().LinkDisplay(sensorPoi.transform);
                            Debug.LogDebug("Sensor poi");
                        }

                        else
                        {
                            // If sensor poi not found, link to default poi.
                            var defaultPoi = GameObject.Find(thing.id + "/default");
                            //sensor.GetComponent<DeviceMqttBehaviour>().LinkDisplay(defaultPoi.transform);
                            Debug.LogDebug("Default poi");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Error: Workplace manager: Couldn't create thing object: " + thing.id);
                    Debug.LogException(e);
                    throw;
                }
            }
            Debug.LogInfo("Workplace manager: Thing objects created.");
        }

        /// <summary>
        /// Helper method that creates person objects.
        /// </summary>
        /// <returns>Returns the number of errors that occured while creating the objects.</returns>
        public async Task CreatePersons()
        {
            foreach (var person in workplaceManager.workplace.persons)
            {
                try
                {
                    // Check for unique object name
                    if (GameObject.Find(person.id))
                        throw new AmbiguousMatchException(person.id + " id already in use.");

                    // Create an empty thing object by using the helper function
                    var temp = LearningExperienceEngine.Utilities.CreateObject(person.id, workplaceManager.personContainer);

                    var counter = 0;

                    // Instantiate poi objects
                    foreach (var poi in person.pois)
                    {
                        // Just to check if default poi is defined.
                        if (poi.id == "default")
                        {
                            counter++;
                        }

                        var poiTemp = LearningExperienceEngine.Utilities.CreateObject(poi.id, temp.transform);

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
                        var poiTemp = LearningExperienceEngine.Utilities.CreateObject("default", temp.transform);
                        var action = LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.Activity.actions.FirstOrDefault(t => t.id == person.id);
                        await PopulateTaskStation(poiTemp, action);
                    }

                    //// Add guide line.
                    //var guide = Object.Instantiate(Resources.Load<GameObject>("Pathway"), Vector3.zero, Quaternion.identity);

                    //// Make guide line a child of default poi object.
                    //guide.transform.SetParent(temp.transform.FindDeepChild("default"));

                    //PathSegmentsController segmentsController = guide.GetComponent<PathSegmentsController>();
                    //segmentsController.startTransform = ServiceManager.GetService<ActivitySelectionSceneReferenceService>().References.floorTarget;
                    //segmentsController.endTransform = guide.transform.parent;

                    //// this should be removed once the ribbons are no longer needed.
                    //guide.GetComponent<PathRoleController>().Role = PathRole.TASKSTATION;

                    //// Hide by default.
                    //guide.SetActive(false);

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

                        var detectableBehaviour = detectable.gameObject.GetComponent<DetectableBehaviour>();
                        detectableBehaviour.AttachedObject = temp;
                        detectableBehaviour.Style = DetectableBehaviour.TrackingStyle.Raw;
                        detectableBehaviour.IsDetectableReady = true;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Error: Workplace manager: Couldn't create person objects.");
                    Debug.LogException(e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Helper method that creates device objects.
        /// </summary>
        /// <returns>Returns the number of errors that occured while creating the objects.</returns>
        public void CreateDevices()
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
                    Debug.LogError("Error: Workplace manager: Couldn't attach user to current device.");
                    Debug.LogException(e);
                    throw;
                }
            }
        }

        // *** SINGLE OBJECT CREATION METHODS ***

        public void CreateDetectableObject(Detectable detectable, bool newObject)
        {
            // Don't recreate existing detectables...
            if (GameObject.Find(detectable.id))
            {
                return;
            }

            Debug.LogInfo($"Creating Detectable Object:{detectable.id}\nPosition:{detectable.origin_position}\nRotation:{detectable.origin_rotation}");

            switch (detectable.type)
            {
                // Hololens world anchors.
                case "anchor":
                    {
                        // Create anchor frame
                        var anchorFrame = new GameObject("anchorObject");
                        anchorFrame.transform.SetParent(workplaceManager.detectableContainer, true);
                        anchorFrame.transform.position = Vector3.zero;
                        anchorFrame.transform.rotation = Quaternion.identity;
                        anchorFrame.name = detectable.id;

                        if (newObject)
                        {
                            anchorFrame.transform.position = RootObject.Instance.PlatformManager.GetTaskStationPosition();
                            anchorFrame.transform.localRotation = Quaternion.identity;
                            anchorFrame.transform.localScale = Vector3.one;

                            detectable.origin_position = Utilities.Vector3ToString(anchorFrame.transform.localPosition);
                            detectable.origin_rotation = Utilities.Vector3ToString(anchorFrame.transform.localRotation.eulerAngles);
                        }
                        else
                        {
                            anchorFrame.transform.localPosition = Utilities.ParseStringToVector3(detectable.origin_position);
                            anchorFrame.transform.localEulerAngles = Utilities.ParseStringToVector3(detectable.origin_rotation);
                            anchorFrame.transform.localScale = Vector3.one;
                        }

                        var anchorBehaviour = anchorFrame.AddComponent<DetectableBehaviour>();
                        anchorBehaviour.Type = DetectableBehaviour.TrackableType.Anchor;
                        anchorBehaviour.IsDetectableReady = true;

                        // Add to the list of calibratable objects and attach the task station
                        var pair = new WorkplaceController.AnchorCalibrationPair
                        {
                            AnchorFrame = anchorFrame,
                            DetectableConfiguration = detectable,
                        };

                        workplaceController.calibrationPairs.Add(pair);

                        break;
                    }

                // Vuforia image targets.
                case "image":
                    {
                        Debug.LogError("Support for Vuforia image targets has been removed");

                        /*var path = Path.Combine(Application.persistentDataPath, workplaceManager.workplace.id, "/detectables/", detectable.id, detectable.id, ".xml");

                        Debug.LogDebug("VUFORIA PATH: " + path);

                        // Check that we have the data set file.
                        if (!DataSet.Exists(path, VuforiaUnity.StorageType.STORAGE_ABSOLUTE))
                        {
                            throw new FileNotFoundException($"{detectable.id} data set not found.");
                        }

                        var objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
                        var dataSet = objectTracker.CreateDataSet();

                        // Try to load the data set.
                        if (!dataSet.Load(path, VuforiaUnity.StorageType.STORAGE_ABSOLUTE))
                        {
                            throw new FileLoadException($"{detectable.id} data set couldn't be loaded.");
                        }

                        // Activate data set
                        if (!objectTracker.ActivateDataSet(dataSet))
                        {
                            throw new FileLoadException($"{detectable.id} data set couldn't be activated.");
                        }

                        // Create the actual image target object...

                        // First get all the available trackable behaviours...
                        var trackableBehaviours = TrackerManager.Instance.GetStateManager().GetTrackableBehaviours();

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
                            originObj.transform.localPosition = Utilities.ParseStringToVector3(detectable.origin_position);
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
                        }*/

                        break;
                    }

                default:
                    {
                        throw new ArgumentException($"{detectable.id} unknown detectable type: {detectable.type}.");
                    }
            }
            Debug.LogInfo($"Creating Detectable Object:{detectable.id} created");
        }

        public async Task CreatePlaceObject(Place place, LearningExperienceEngine.Action action)
        {
            // Check for unique object name
            if (GameObject.Find(place.id))
            {
                throw new AmbiguousMatchException(place.id + " id already in use.");
            }

            // Create an empty place object by using the helper function
            var temp = LearningExperienceEngine.Utilities.CreateObject(place.id, workplaceManager.placeContainer);

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
                GameObject poiTemp = LearningExperienceEngine.Utilities.CreateObject("default", temp.transform);
                poiTemp.transform.parent = temp.transform;
                await PopulateTaskStation(poiTemp, action);
                Debug.Log("after creation " + poiTemp.transform.GetChild(0).name + " has " + poiTemp.transform.GetChild(0).childCount + " children");
            }

            //// Add guide line.
            //var guide = Object.Instantiate(Resources.Load<GameObject>("Pathway"), Vector3.zero, Quaternion.identity);

            //// Make guide line a child of default poi object.
            //guide.transform.SetParent(temp.transform.FindDeepChild("default"));

            //var segmentsController = guide.GetComponent<PathSegmentsController>();
            //segmentsController.startTransform = ServiceManager.GetService<ActivitySelectionSceneReferenceService>().References.floorTarget;
            //segmentsController.endTransform = guide.transform.parent.GetChild(0);

            //// this should be removed once the ribbons are no longer needed.
            //guide.GetComponent<PathRoleController>().Role = PathRole.TASKSTATION;

            //// Hide by default.
            //guide.SetActive(false);

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

                var detectableBehaviour = detectable.gameObject.GetComponent<DetectableBehaviour>();
                detectableBehaviour.AttachedObject = temp;
                detectableBehaviour.Style = DetectableBehaviour.TrackingStyle.Raw;
                detectableBehaviour.IsDetectableReady = true;
            }
        }

        public async Task CreatePoiObject(Poi poi, Transform parent)
        {
            var poiTemp = LearningExperienceEngine.Utilities.CreateObject(poi.id, parent);
            poiTemp.AddComponent<PoiEditor>().Initialize(poi);

            // If offset not defined as separate values...
            if (poi.x_offset.Equals(0) && poi.y_offset.Equals(0) && poi.z_offset.Equals(0))
            {
                // Use the CSV format if available.
                if (!string.IsNullOrEmpty(poi.offset))
                {
                    poiTemp.transform.localPosition = Utilities.ParseStringToVector3(poi.offset);
                }
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
        /// <param name="source"></param>
        /// <returns></returns>
        public (Vector3, Vector3) GetPoseRelativeToCalibrationOrigin(GameObject source)
        {
            var anchor = RootObject.Instance.CalibrationManager.Anchor;

            var position = anchor.InverseTransformPoint(source.transform.position);
            var rotation = Quaternion.Inverse(anchor.rotation) * source.transform.rotation;

            return (position, rotation.eulerAngles);
        }

        private async Task PopulateTaskStation(GameObject parent, LearningExperienceEngine.Action action)
        {
            string debug = (action != null && !string.IsNullOrEmpty(action.id)) ? action.id : String.Empty;
            Debug.LogInfo("CLONING TASK STATION for action " + debug);

            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>("PlayerTaskStation");
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject prefab = handle.Result;
                instance = UnityEngine.Object.Instantiate(prefab, parent.transform);
                instance.transform.parent = parent.transform; // just to be sure
                //instance.SetActive(true); // just to be sure (may not be needed / good)
            }
            else
            {
                Debug.LogError("FATAL ERROR: Could not instantiate task station prefab");
            }

            Debug.LogInfo("Received adressable, and CLONED TASK STATION for action " + debug + "--- " + instance.name);

            // this was the old prefab loading and instantiating code
            // GameObject prefab = await ReferenceLoader.GetAssetReferenceAsync<GameObject>("PlayerTaskStation");
            // GameObject instance = Object.Instantiate(prefab, parent.transform);

            var taskStationEditor = instance.GetComponentInChildren<TaskStationEditor>();
            taskStationEditor.Init(action);

            //only for the first taskstation in this step move it to the right of the player
            var taskStationPos = LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.ActionsOfTypeAction.Count == 0 ? Camera.main.transform.right * offsetFromPlayer : Vector3.zero;

            var isFirstTaskStation = RootObject.Instance.PlatformManager.WorldSpaceUi && LearningExperienceEngine.LearningExperienceEngine.Instance.ActivityManagerOld.EditModeActive;
            instance.transform.localPosition = isFirstTaskStation ? taskStationPos : Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;

            //// test code borrowed from ActionDetailView
            //var actionId = activityManager.ActiveActionId;
            //Debug.LogInfo("CLONED TASK STATION --- actionId " + actionId);
            //var place = GameObject.Find(actionId);
            //if (!place) Debug.LogInfo("Warning: could not find place with actionID = " + actionId);
            ////var taskStation = place.transform.Find("default/PlayerTaskStation(Clone)"); // TODO: possible NRE
            ////if (!taskStation) Debug.LogInfo("Warning: could not find task station clone");

            // now release the opHandle again (from the prefab addressables loading/cloning)
            Addressables.Release(handle);
            LearningExperienceEngine.EventManager.NotifyTaskStationCloned();
        }

    }
}