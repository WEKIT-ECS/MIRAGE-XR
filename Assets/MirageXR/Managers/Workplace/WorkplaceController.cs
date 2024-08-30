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
        private static WorkplaceManager workplaceManager => RootObject.Instance.WorkplaceManager;

        private void OnEnable()
        {
            // Register to event manager events
            EventManager.OnPlayerReset += ClearPois;
            EventManager.OnClearAll += PlayerReset;
        }

        private void OnDisable()
        {
            // Unregister from event manager events
            EventManager.OnPlayerReset -= ClearPois;
            EventManager.OnClearAll -= PlayerReset;
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

        // Called from event manager
        // Clears the WorkplaceModel
        private void PlayerReset()
        {
            workplaceManager.PlayerReset();
        }
    }
}
