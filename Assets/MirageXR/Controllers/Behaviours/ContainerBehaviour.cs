using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Class for all the containers that should remain in the scene on scene clear.
    /// </summary>
    public class ContainerBehaviour : MonoBehaviour
    {
        private void OnEnable()
        {
            // Register to events.
            LearningExperienceEngine.EventManager.OnClearAll += ClearContainer;
        }

        private void OnDisable()
        {
            // Unregister from events.
            LearningExperienceEngine.EventManager.OnClearAll -= ClearContainer;
        }

        /// <summary>
        /// Resets the container to empty state.
        /// </summary>
        private void ClearContainer()
        {
            // Destroy all the child objects inside the container.
            foreach (Transform obj in transform)
            {
                obj.SendMessage("Delete", SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}