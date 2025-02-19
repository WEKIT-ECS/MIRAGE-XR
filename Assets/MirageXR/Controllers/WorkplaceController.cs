using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

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

        private LearningExperienceEngine.WorkplaceManager workplaceManager => LearningExperienceEngine.LearningExperienceEngine.Instance.WorkplaceManager;
        private WorkplaceObjectFactory objectFactory;
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
            LearningExperienceEngine.EventManager.OnResetPlayer += PlayerReset;
            LearningExperienceEngine.EventManager.OnClearAll += ClearPois;
        }

        private void OnDisable()
        {
            // Unregister from event manager events
            LearningExperienceEngine.EventManager.OnResetPlayer -= PlayerReset;
            LearningExperienceEngine.EventManager.OnClearAll -= ClearPois;
        }

        private void Start()
        {
            Debug.Log("starting WorkplaceController - registering CreateObjects");
            LearningExperienceEngine.EventManager.OnInitializeWorkplaceView += CreateObjects;

            // At least TRY to clear out the cache!
            Caching.ClearCache();
        }

        private void OnDestroy()
        {
            LearningExperienceEngine.EventManager.OnInitializeWorkplaceView -= CreateObjects;
        }

        /// <summary>
        /// Constructor - initalize the WorkPlaceObjectFactory
        /// </summary>
        WorkplaceController()
        {
            objectFactory = new WorkplaceObjectFactory();
        }

        /// <summary>
        /// Clears the calibration pairs (= set activity to uncalibrated) and sets the workplace data model to null.
        /// Called from event manager or directly.
        /// </summary>
        private void ClearPois()
        {
            calibrationPairs.Clear(); // clear calibration
            workplaceManager.Reset(); // clear the workplace data model
        }

        /// <summary>
        /// Called from event manager, clears the WorkplaceModel
        /// </summary>
        private void PlayerReset()
        {
            LearningExperienceEngine.EventManager.ClearPois();
        }

        private async Task PerformEditModeCalibration()
        {
            foreach (var pair in calibrationPairs)
            {
                var (position, rotation) = objectFactory.GetPoseRelativeToCalibrationOrigin(pair.AnchorFrame);
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
            objectFactory.CreateDetectableObject(detectable, newObject);
        }

        // callback for LearningExperienceEngine.WorkplaceManager.AddPlace
        private async Task CreateDetectableWorkplaceObject(LearningExperienceEngine.Place place, LearningExperienceEngine.Action newAction)
        {
            await objectFactory.CreatePlaceObject(place, newAction);
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
            objectFactory.CreateDetectables(workplaceManager.workplace.detectables, "detectables");
            objectFactory.CreateSensors();
            await objectFactory.CreateThings();
            await objectFactory.CreatePlaces(workplaceManager.workplace.places, "places");
            //await objectFactory.CreatePersons(); // these are not used, commenting out
            //objectFactory.CreateDevices(); // these are not used, commenting out

            // If workplace has anchors which have not been calibrated...
            if (calibrationPairs.Count > 0 && !UiManager.Instance.IsCalibrated)
            {
                Debug.LogWarning("Workplace has uncalibrated anchors. Please re-run the calibration");
            }

            Debug.Log("********** triggering EventManager.NotifyWorkplaceControllerInitialized");
            LearningExperienceEngine.EventManager.NotifyWorkplaceControllerInitialized();
            
            // SUGGESTION Use a different event here that symbolizes the end of the view update by the model
            //LearningExperienceEngine.EventManager.WorkplaceLoaded();
            
        }

        //public void onCloneTaskStation (GameObject parent, LearningExperienceEngine.Action action, GameObject iObj)
        //{
        //    UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> opHandle = Addressables.LoadAssetAsync<GameObject>("PlayerTaskStation");
        //    yield return opHandle;
        //    if (opHandle.Status == AsyncOperationStatus.Succeeded)
        //    {
        //        GameObject prefab = opHandle.Result;
        //        // await ReferenceLoader.GetAssetReferenceAsync<GameObject>("PlayerTaskStation");
        //        iObj = UnityEngine.Object.Instantiate(prefab, parent.transform);
        //        iObj.transform.parent = parent.transform; // just to be sure
        //        // iObj.name = "PlayerTaskStation(Clone)";
        //        // iObj.SetActive(true);
        //    } else
        //    {
        //        Debug.LogError("FATAL ERROR: Could not instantiate task station prefab");
        //    }
        //}

    }
}
