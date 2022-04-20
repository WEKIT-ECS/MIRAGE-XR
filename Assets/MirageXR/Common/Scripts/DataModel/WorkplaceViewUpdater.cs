using System.Threading.Tasks;
using UnityEngine;

namespace MirageXR
{
    // <summary>
    // Model script in the MVP sense.
    // Updates workplace objects in the scene upon changes to the workplace data model.
    // Utilizes WorkplaceManager's CreateObjects functionality.
    // Could include methods reacting to calibtation changes in the future
    // </summary>
    public static class WorkplaceViewUpdater
    {
        private static WorkplaceManager workplaceManager => RootObject.Instance.workplaceManager;

        /// <summary>
        /// Sets up the scene by creating all the objects defined in the workplace data model.
        /// Called from the event manager.
        /// </summary>
        /// <param name="workplaceId">ID of the workplace file JSON file.</param>
        public static async Task CreateObjects()
        {
            EventManager.DebugLog("Workplace manager: Starting to create the objects...");
            
            // Instantiate detectables first, since they need to be there when others want to attach to them.
            //TODO further detatch the model from the WorkplaceManager
            await WorkplaceObjectFactory.CreateDetectablesOrPlaces(workplaceManager.Workplace.detectables, "detectables");

            // Instantiate device sensors.
            WorkplaceObjectFactory.CreateSensors();
            // Instantiate thing objects
            await WorkplaceObjectFactory.CreateThings();
            // Instantiate place objects
            await WorkplaceObjectFactory.CreateDetectablesOrPlaces(workplaceManager.Workplace.places, "places");
            // Instantiate person objects
            await WorkplaceObjectFactory.CreatePersons();
            // Attach user to current device.
            WorkplaceObjectFactory.CreateDevices();

            // If workplace has anchors which have not been calibrated...
            if (workplaceManager.calibrationPairs.Count > 0 && !UiManager.Instance.IsCalibrated)
            {
                Debug.Log("Workplace has uncalibrated anchors. Please re-run the calibration");
            }

            Debug.Log("********** EventManager.WorkplaceLoaded");
            //SUGGESTION Use a different event here that symbolizes the end of the view update by the model
            EventManager.WorkplaceLoaded();
        }
    }
}
