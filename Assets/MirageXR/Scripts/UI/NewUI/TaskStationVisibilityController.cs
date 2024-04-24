using UnityEngine;
using UnityEngine.UI; 

namespace MirageXR
{
    /// <summary>
    /// Controls the visibility of a game object based on a toggle.
    /// </summary>
    public class TaskStationVisibilityController : MonoBehaviour
    {
        private Renderer myRenderer;

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        /// <remarks>
        /// This method is used to initialize the component by finding and assigning the Renderer component from the first child object.
        /// It also logs an error if no Renderer component is found.
        /// </remarks>
        void Awake()
        {
            myRenderer = GetComponentInChildren<Renderer>();
            if (myRenderer == null)
            {
                UnityEngine.Debug.LogError("No renderer found in game object (TaskStationVisibilityController.cs)");
            }
        }

        /// <summary>
        /// Sets the visibility of the game object.
        /// </summary>
        /// <param name="isVisible">The visibility state of the game object.</param>
        public void SetVisibility(bool isVisible)
        {
            if (myRenderer != null)
            {
                myRenderer.enabled = isVisible;
            }
            else
            {
                UnityEngine.Debug.LogError("No renderer assignt in game object (TaskStationVisibilityController.cs)");
            }
        }
    }
}