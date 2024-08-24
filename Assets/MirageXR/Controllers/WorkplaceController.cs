using System.Threading.Tasks;
using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Instantiates the workplace model into the scene.
    /// </summary>
    public class WorkplaceController : MonoBehaviour
    {
        private static LearningExperienceEngine.WorkplaceManager workplaceManager => LearningExperienceEngine.LearningExperienceEngine.Instance.workplaceManager;

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
