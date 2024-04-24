using UnityEngine;
using UnityEngine.UI; 

namespace MirageXR
{
    /// <summary>
    /// Class for controlling the visibility of a toggle button in Unity.
    /// </summary>
    /// <remarks>
    /// This class is used to control the visibility of a toggle button in Unity. It is designed to be used in conjunction with the Toggle component from the UnityEngine.UI namespace.
    /// </remarks>
    public class ToggleVisibilityController : MonoBehaviour
    {
        public Toggle myToggle;

        /// <summary>
        /// Controls the visibility of a game object based on a toggle.
        /// </summary>
        private TaskStationVisibilityController visibilityController;

        /// <summary>
        /// This method is called when the ToggleVisibilityController starts.
        /// </summary>
        /// <remarks>
        /// The Start method is responsible for initializing the ToggleVisibilityController by subscribing to the onValueChanged event of the Toggle component and assigning the visibilityController variable with an instance of the TaskStationVisibilityController.
        /// </remarks>
        void Start()
        {
            if (myToggle != null)
            {
                myToggle.onValueChanged.AddListener(ToggleValueChanged);
                visibilityController = FindObjectOfType<TaskStationVisibilityController>();
            }
        }

        /// <summary>
        /// Method called when the value of the toggle is changed.
        /// </summary>
        /// <param name="isOn">The new value of the toggle.</param>
        private void ToggleValueChanged(bool isOn)
        {
            if (visibilityController != null)
            {
                UnityEngine.Debug.Log(visibilityController);
                visibilityController.SetVisibility(isOn);
            }
        }
    }
}