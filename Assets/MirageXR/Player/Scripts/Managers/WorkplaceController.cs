using System.Threading.Tasks;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Creates and updates the workplace model.
    /// Utilizes WorkplaceManager for parsing Arlem workplace files
    /// </summary>
    public class WorkplaceController : MonoBehaviour
    {
        private static WorkplaceManager workplaceManager => RootObject.Instance.workplaceManager;

        void OnEnable()
        {
            // Register to event manager events
            EventManager.OnPlayerReset += ClearPois;
            EventManager.OnClearAll += PlayerReset;
            EventManager.OnCalibrateWorkplace += CalibrateWorkplace;
        }

        void OnDisable()
        {
            // Unregister from event manager events
            EventManager.OnPlayerReset -= ClearPois;
            EventManager.OnClearAll -= PlayerReset;
            EventManager.OnCalibrateWorkplace -= CalibrateWorkplace;
        }

        void Start()
        {
            // At least TRY to clear out the cache!
            Caching.ClearCache();
        }

        // Called from event manager
        private async Task ParseWorkplace(string workplaceId)
        {
            await workplaceManager.ParseWorkplace(workplaceId);
        }

        // Called from event manager
        private void ClearPois()
        {
            EventManager.ClearPois();
        }

        // Called from event manager
        // Clears the WorkplaceModel
        private void PlayerReset()
        {
            workplaceManager.PlayerReset();
        }

        /// <summary>
        /// Controller triggers the calibration of the workplace anchors and
        /// performs changes to the model using the WorkplaceManager's functionality.
        /// </summary>
        /// <param name="origin">Origin transform from the calibration target.</param>
        private void CalibrateWorkplace(Transform origin)
        {
            var activityManager = RootObject.Instance.activityManager;
            if (activityManager.EditModeActive)
            {
                workplaceManager.PerformEditModeCalibration(origin);
            }
            else
            {
                workplaceManager.PerformPlayModeCalibration(origin);
            }

            activityManager.StartActivity();
        }
    }
}
