using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
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

        // Device user id.
        public static string userID { get; set; }

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
        public Workplace workplace;

        /// <summary>
        /// Reset workplace manager when OnPlayerReset event is triggered.
        /// </summary>
        public void PlayerReset()
        {
            workplace = null;
            userID = null;
            calibrationPairs.Clear();
        }

        /// <summary>
        /// Parses the workplace file, deserializes JSON and instantiates the workplace data model.
        /// Called from the event manager.
        /// </summary>
        /// <param name="workplaceId">ID of the workplace file JSON file.</param>
        public async Task LoadWorkplace(string workplaceId)
        {
            InitContainers();

            // empty string => create new workplace
            if (string.IsNullOrEmpty(workplaceId))
            {
                workplace = new Workplace();
            }

            // For loading from resources
            else
            {
                try
                {
                    workplace = WorkplaceParser.Parse(workplaceId);
                }
                catch (Exception e)
                {
                    EventManager.DebugLog("Error: Workplace manager: Parsing: Couldn't parse the Workplace: " + e);
                }
            }

            await WorkplaceViewUpdater.CreateObjects();
        }

        private void InitContainers()
        {
            thingContainer = GameObject.Find("Things").transform;
            placeContainer = GameObject.Find("Places").transform;
            personContainer = GameObject.Find("Persons").transform;
            detectableContainer = GameObject.Find("Detectables").transform;
            sensorContainer = GameObject.Find("Sensors").transform;

            if (!detectableContainer.parent || detectableContainer.parent == placeContainer.parent)
            {
                var holder = new GameObject("DetectablesHolder");
                if (detectableContainer.parent)
                {
                    holder.transform.SetParent(detectableContainer.parent, true);
                }

                detectableContainer.SetParent(holder.transform, true);
            }

            var poseСopier = detectableContainer.parent.gameObject.AddComponent<PoseСopier>();
            poseСopier.target = RootObject.Instance.calibrationManager.anchor;
            poseСopier.localSpace = false;
        }

        /// <summary>
        /// Get current device user id.
        /// </summary>
        /// <returns>Returns the id or "anonymous" if user not defined.</returns>
        public static string GetUser()
        {
            if (string.IsNullOrEmpty(userID))
            {
                return "anonymous";
            }

            return userID;
        }

        private async Task PerformEditModeCalibration()
        {
            foreach (var pair in calibrationPairs)
            {
                var (position, rotation) = WorkplaceObjectFactory.GetPoseRelativeToCalibrationOrigin(pair.AnchorFrame);
                var detectable = pair.DetectableConfiguration;

                detectable.origin_position = Utilities.Vector3ToString(position);
                detectable.origin_rotation = Utilities.Vector3ToString(rotation);
            }

            await Task.Yield();

            UiManager.Instance.IsCalibrated = true;
        }

        private async Task PerformPlayModeCalibration()
        {
            if (calibrationPairs.Count == 0)
            {
                return;
            }

            foreach (var pair in calibrationPairs)
            {
                var localPosition = Utilities.ParseStringToVector3(pair.DetectableConfiguration.origin_position);
                var localRotation = Utilities.ParseStringToVector3(pair.DetectableConfiguration.origin_rotation);

                pair.AnchorFrame.transform.localPosition = localPosition;
                pair.AnchorFrame.transform.localRotation = Quaternion.Euler(localRotation);
                pair.AnchorFrame.GetComponent<DetectableBehaviour>().AttachAnchor();
            }

            await Task.Yield();
        }

        public Place GetPlaceFromTaskStationId(string id)
        {
            return workplace.places.Find(item => item.id == id);
        }

        public Detectable GetDetectable(Place place)
        {
            return workplace.detectables.Find((item) => item.id == place.detectable);
        }

        public void SaveWorkplace()
        {
            var recFilePath = Path.Combine(Application.persistentDataPath, workplace.id);

            var json = WorkplaceParser.Serialize(workplace);
            File.WriteAllText(recFilePath, json);
        }

        public async Task AddPlace(Action newAction, Vector3 targetPosition, bool hasMarker = false)
        {
            var place = new Place
            {
                id = newAction.id,
                name = string.Empty,
                detectable = "WA-" + newAction.id.Substring(3),
            };

            var detectable = new Detectable
            {
                id = place.detectable,
                sensor = string.Empty,
                url = string.Empty,
                type = "anchor",
            };

            if (hasMarker)
            {
                detectable.type = "marker";
                return;
            }

            workplace.detectables.Add(detectable);
            workplace.places.Add(place);

            // TODO move this to the model
            WorkplaceObjectFactory.CreateDetectableObject(detectable, true);
            await WorkplaceObjectFactory.CreatePlaceObject(place);
        }

        public void AddAnnotation(Action action, ToggleObject toggleObject, Vector3 position)
        {
            try
            {
                var place = GetPlaceFromTaskStationId(action.id);

                var poi = new Poi
                {
                    id = toggleObject.poi,
                    x_offset = position.x,
                    y_offset = position.y,
                    z_offset = position.z,
                    offset = $"{position.x.ToString(CultureInfo.InvariantCulture)}, {position.y.ToString(CultureInfo.InvariantCulture)}, {position.z.ToString(CultureInfo.InvariantCulture)}",
                    rotation = "0, 0, 0",
                };

                WorkplaceObjectFactory.CreatePoiObject(poi, GameObject.Find(place.id).transform);

                place.pois.Add(poi);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void DeleteAugmentation(Action action, ToggleObject toggleObject)
        {
            var place = GetPlaceFromTaskStationId(action.id);

            var poi = place.pois.Find((item) => item.id == toggleObject.poi);
            if (poi != null)
            {
                var temp = GameObject.Find(poi.id);

                Object.Destroy(temp);
                place.pois.Remove(poi);
            }
        }

        public void DeletePlace(string id)
        {
            var place = GetPlaceFromTaskStationId(id);

            workplace.places.Remove(place);

            workplace.detectables.Remove(GetDetectable(place));
        }

        /// <summary>
        /// Controller triggers the calibration of the workplace anchors and
        /// performs changes to the model using the WorkplaceManager's functionality.
        /// </summary>
        /// <param name="origin">Origin transform from the calibration target.</param>
        public async Task CalibrateWorkplace(bool isNewPosition = false)
        {
            await activityManager.ActivateFirstAction();

            if (isNewPosition)
            {
                await PerformEditModeCalibration();
            }
            else
            {
                await PerformPlayModeCalibration();
            }

            //await activityManager.StartActivity();

            EventManager.WorkplaceCalibrated();
            Maggie.Speak("Workplace is now calibrated.");
        }
    }
}
