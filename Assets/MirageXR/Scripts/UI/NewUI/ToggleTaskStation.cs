using UnityEngine;
using UnityEngine.UI; // Zugriff auf die UI-Komponenten


namespace MirageXR
{

// Steuert die Sichtbarkeit basierend auf einem Toggle
    public class ToggleVisibilityController : MonoBehaviour
    {
        public Toggle myToggle; // Der Toggle, der die Sichtbarkeit steuert
        private TaskStationVisibilityController visibilityController; // Referenz zum VisibilityController

        void Start()
        {
            // Überprüfe, ob der Toggle zugewiesen wurde
            if (myToggle != null)
            {
                // Füge den Listener hinzu, der aufgerufen wird, wenn der Toggle seinen Wert ändert
                myToggle.onValueChanged.AddListener(ToggleValueChanged);
                // Suche den VisibilityController im Scene
                visibilityController = FindObjectOfType<TaskStationVisibilityController>();
            }
        }

        private void ToggleValueChanged(bool isOn)
        {
            UnityEngine.Debug.Log("Toggle............");
            // Setze die Sichtbarkeit des Ziel-GameObjects
            if (visibilityController != null)
            {
                UnityEngine.Debug.Log(visibilityController);
                visibilityController.SetVisibility(isOn);
            }
            else
            {
                UnityEngine.Debug.Log("visibilityController = null ............");
            }
        }
    }
}