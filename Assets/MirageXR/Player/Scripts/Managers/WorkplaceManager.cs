using i5.Toolkit.Core.ServiceCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using Vuforia;
using Object = UnityEngine.Object;

namespace MirageXR
{
    /// <summary>
    /// WorkplaceParser. Used for parsing Arlem workplace files
    /// and for handling all the resources defined in the file.
    /// </summary>
    public class WorkplaceManager
    {
        private static ActivityManager activityManager => RootObject.Instance.activityManager;
        // Containers for all the main types.
        public Transform thingContainer { get; private set; }
        public Transform placeContainer { get; private set; }
        public Transform personContainer { get; private set; }
        public Transform detectableContainer { get; private set; }
        public Transform sensorContainer { get; private set; }

        // List of predicate symbols.
        private static List<Sprite> _predicates = new List<Sprite>();

        // List of ISO7010 symbols.
        private static List<Sprite> _iso7010s = new List<Sprite>();

        // Device user id.
        public static string userID {get; set; }

        // Frame / configuration pair for anchor calibration.
        [Serializable]
        public struct AnchorCalibrationPair
        {
            public GameObject AnchorFrame;
            public Detectable DetectableConfiguration;
        }

        // List of calibration pairs.
        public List<AnchorCalibrationPair> calibrationPairs = new List<AnchorCalibrationPair>();

        [Tooltip("Scaling factor required to get content scale to match 1m = 1 unit scale.")]
        public static float ScalingFactor = 0.25f;

        [Tooltip("Instantiation of the workplace file.")]
        public Workplace Workplace;

        [SerializeField] private GameObject MirageXRSensorManager;

        /// <summary>
        /// Reset workplace manager when OnPlayerReset event is triggered.
        /// </summary>
        public void PlayerReset()
        {
            Workplace = null;
            userID = null;
            calibrationPairs.Clear();
        }

        /// <summary>
        /// Parses the workplace file, deserializes JSON and instantiates the workplace data model.
        /// Called from the event manager.
        /// </summary>
        /// <param name="workplaceId">ID of the workplace file JSON file.</param>
        public async Task ParseWorkplace(string workplaceId)
        {
            var errorCount = 0;

            // Get the containers
            thingContainer = GameObject.Find("Things").transform;
            placeContainer = GameObject.Find("Places").transform;
            personContainer = GameObject.Find("Persons").transform;
            detectableContainer = GameObject.Find("Detectables").transform;
            sensorContainer = GameObject.Find("Sensors").transform;

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
                }
            }

            // If all good...
            if (errorCount.Equals(0))
            {
                // Fire the event that the workplace has been successfully parsed.
                Debug.Log("********** EventManager.WorkplaceParsed");
                await WorkplaceViewUpdater.CreateObjects();
            }
            else
                EventManager.DebugLog("Error: Workplace manager: Couldn't load the workplace file.");
        }

        /// <summary>
        /// Get predicate symbol sprite.
        /// </summary>
        /// <param name="id">Id of the predicate.</param>
        /// <returns>Returns null if sprite not found.</returns>
        //SUGGESTION Shall we put this object on a separate layer to reduce the scope of the search?
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
        //SUGGESTION Shall we put this object on a separate layer to reduce the scope of the search?
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
            if (string.IsNullOrEmpty(userID))
                return "anonymous";

            return userID;
        }
        
        public async Task PerformEditModeCalibration()
        {
            Debug.Log("Edit Mode Calibration started.\n");

            foreach (var pair in calibrationPairs)
            {
                (Vector3, Vector3) relativePose = WorkplaceObjectFactory.GetPoseRelativeToCalibrationOrigin(pair.AnchorFrame);
                Vector3 myPos = relativePose.Item1;
                Vector3 myRot = relativePose.Item2;

                Detectable detectable = pair.DetectableConfiguration;

                // set detectable values
                detectable.origin_position = $"{myPos.x.ToString(CultureInfo.InvariantCulture)}, {myPos.y.ToString(CultureInfo.InvariantCulture)}, {myPos.z.ToString(CultureInfo.InvariantCulture)}";
                detectable.origin_rotation = $"{myRot.x.ToString(CultureInfo.InvariantCulture)}, {myRot.y.ToString(CultureInfo.InvariantCulture)}, {myRot.z.ToString(CultureInfo.InvariantCulture)}";
            }

            await Task.Yield();

            UiManager.Instance.IsCalibrated = true;
            Maggie.Speak("Workplace configuration saved.");

            //delete calibration animation guide
            var calibrationGuide = GameObject.Find("CalibrationGuide");
            if (calibrationGuide)
            {
                Object.Destroy(calibrationGuide);
            }

            Debug.Log("Edit mode calibration completed.");
        }

        // here we are writing anchors for calibration pairs that (must) already exist, relative to a calibration origin.
        public async Task PerformPlayModeCalibration(Transform calibrationRoot)
        {
            Debug.Log("Play Mode Calibration started.\n");

            foreach (var pair in calibrationPairs)
            {
                Debug.Log("Handling " + pair.DetectableConfiguration.id);

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
                pair.AnchorFrame.transform.localEulerAngles = Utilities.ParseStringToVector3(pair.DetectableConfiguration.origin_rotation);

                pair.AnchorFrame.transform.localPosition = Utilities.ParseStringToVector3(pair.DetectableConfiguration.origin_position);

                // Update position to also the tangible attached to this anchor.
                pair.AnchorFrame.GetComponent<DetectableBehaviour>().AttachAnchor();

                // Now move the frame back to detectable container.
                pair.AnchorFrame.transform.SetParent(detectableContainer);

                // Now attach anchor to the detectable anchor frame.
                Debug.Log($"Anchor {pair.AnchorFrame.name} created at {pair.AnchorFrame.transform.position} || {pair.AnchorFrame.transform.eulerAngles}.");

                // Destroy dummy.
                Object.Destroy(dummy);
            }


            // Add a small delay just to make sure all the anchors are stored...
            await Task.Yield();

            EventManager.WorkplaceCalibrated();
            Maggie.Speak("Workplace is now calibrated.");

            //delete calibration animation guide
            var calibrationGuide = GameObject.Find("CalibrationGuide");
            if (calibrationGuide)
                Object.Destroy(calibrationGuide);

            Debug.Log("Play mode calibration completed.");
        }

        public Place GetPlaceFromTaskStationId(string id)
        {
            return Workplace.places.Find(item => item.id == id);
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

        public async Task AddPlace(Action newAction, Vector3 targetPosition, bool hasMarker = false)
        {
            Place place = new Place
            {
                id = newAction.id,
                name = "",
                detectable = "WA-" + newAction.id.Substring(3),
            };
            Detectable detectable = new Detectable
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

            //TODO move this to the model
            WorkplaceObjectFactory.CreateDetectableObject(detectable, true);
            await WorkplaceObjectFactory.CreatePlaceObject(place);
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

                WorkplaceObjectFactory.CreatePoiObject(poi, GameObject.Find(place.id).transform);

                place.pois.Add(poi);
            }catch (Exception e)
            {
                Debug.LogError(e);
            }

        }

        public void DeleteAugmentation(Action action, ToggleObject toggleObject)
        {
            Place place = GetPlaceFromTaskStationId(action.id);


            Poi poi = place.pois.Find((item) => item.id == toggleObject.poi);
            if (poi != null)
            {
                Object.Destroy(GameObject.Find(poi.id));
                place.pois.Remove(poi);
            }

            if (toggleObject.predicate == "imagemarker")
            {
                Detectable detectable = GetDetectable(GetPlaceFromTaskStationId(toggleObject.id));

                GameObject detectableObj = GameObject.Find(detectable.id);
                GameObject detectableParentObj = GameObject.Find("Detectables");

                //as Vuforia dosent allow image markers to be destroyed at run time the detectable is moved instead leaving the marker still in the scene but removeing its content
                detectableObj.transform.parent = detectableParentObj.transform;
            }
        }

        /// <summary>
        /// Controller triggers the calibration of the workplace anchors and
        /// performs changes to the model using the WorkplaceManager's functionality.
        /// </summary>
        /// <param name="origin">Origin transform from the calibration target.</param>
        public async void CalibrateWorkplace(Transform origin)
        {
            if (activityManager.EditModeActive)
            {
                await PerformEditModeCalibration();
            }
            else
            {
                await PerformPlayModeCalibration(origin);
            }

            await activityManager.StartActivity();
        }
    }
}
