using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Instantiates the workplace model into the scene.
    /// Updates workplace objects in the scene upon changes to the workplace data model.
    /// Utilizes WorkplaceManager's CreateObjects functionality.
    /// Could include methods reacting to calibration changes in the future
    /// </summary>
    public class WorkplaceController : MonoBehaviour
    {
        private static LearningExperienceEngine.WorkplaceManager workplaceManager => LearningExperienceEngine.LearningExperienceEngine.Instance.workplaceManager;

        #region Calibration pair data

        /// <summary>
        /// Frame / configuration pair for anchor calibration.
        /// </summary>
        [Serializable]
        public struct AnchorCalibrationPair
        {
            public GameObject AnchorFrame;
            public LearningExperienceEngine.Detectable DetectableConfiguration;
        }

        /// <summary>
        /// List of calibration pairs.
        /// </summary>
        public List<AnchorCalibrationPair> calibrationPairs = new List<AnchorCalibrationPair>();

        #endregion

        private void OnEnable()
        {
            // Register to event manager events
            EventManager.OnPlayerReset += ClearPois;
            EventManager.OnClearAll += PlayerReset;
            LearningExperienceEngine.EventManager.OnInitializeWorkplaceView += CreateObjects;
        }

        private void OnDisable()
        {
            // Unregister from event manager events
            EventManager.OnPlayerReset -= ClearPois;
            EventManager.OnClearAll -= PlayerReset;
            LearningExperienceEngine.EventManager.OnInitializeWorkplaceView -= CreateObjects;
        }

        private void Start()
        {
            // At least TRY to clear out the cache!
            Caching.ClearCache();
        }

        // Called from event manager
        private void ClearPois()
        {
            EventManager.ClearPois();
        }

        /// <summary>
        /// Called from event manager, clears the WorkplaceModel
        /// </summary>
        private void PlayerReset()
        {
            calibrationPairs.Clear();
            workplaceManager.PlayerReset();
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
        }

        private async Task PerformPlayModeCalibration()
        {
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

        // callback for LearningExperienceEngine.WorkplaceManager.AddPlace
        private async Task CreateDetectableWorkplaceObject(LearningExperienceEngine.Detectable detectable, bool newObject)
        {
            WorkplaceObjectFactory.CreateDetectableObject(detectable, newObject);
        }

        // callback for LearningExperienceEngine.WorkplaceManager.AddPlace
        private async Task CreateDetectableWorkplaceObject(LearningExperienceEngine.Place place, LearningExperienceEngine.Action newAction)
        {
            await WorkplaceObjectFactory.CreatePlaceObject(place, newAction);
        }

        /// <summary>
        /// Sets up the scene by creating all the objects defined in the workplace data model.
        /// Called from the event manager.
        /// </summary>
        /// <param name="workplaceId">ID of the workplace file JSON file.</param>
        public async void CreateObjects()
        {
            Debug.LogInfo("Workplace controller: Starting to create the objects...");

            // Instantiate detectables first, since they need to be there when others want to attach to them.
            // TODO: further detatch the model from the WorkplaceManager
            WorkplaceObjectFactory.CreateDetectables(workplaceManager.workplace.detectables, "detectables");
            WorkplaceObjectFactory.CreateSensors();
            await WorkplaceObjectFactory.CreateThings();
            await WorkplaceObjectFactory.CreatePlaces(workplaceManager.workplace.places, "places");
            await WorkplaceObjectFactory.CreatePersons();
            WorkplaceObjectFactory.CreateDevices();

            // If workplace has anchors which have not been calibrated...
            if (calibrationPairs.Count > 0 && !UiManager.Instance.IsCalibrated)
            {
                Debug.LogWarning("Workplace has uncalibrated anchors. Please re-run the calibration");
            }

            Debug.Log("********** EventManager.WorkplaceLoaded");
            // SUGGESTION Use a different event here that symbolizes the end of the view update by the model
            LearningExperienceEngine.EventManager.WorkplaceLoaded();
        }

    }
}
