using UnityEngine;
using UnityEngine.UI; // Zugriff auf die UI-Komponenten


namespace MirageXR
{


// Steuert die Sichtbarkeit eines Game Objects
    public class TaskStationVisibilityController : MonoBehaviour
    {
        private Renderer myRenderer; // Renderer des Game Objects

        void Awake()
        {
            // Hole den Renderer der Komponente
            myRenderer = GetComponentInChildren<Renderer>();
            if (myRenderer == null)
            {
                UnityEngine.Debug.LogError("No Ernerder found in Gamen obj");
            }
        }

        public void SetVisibility(bool isVisible)
        {
            // Aktiviere oder deaktiviere den Renderer
            if (myRenderer != null)
            {
                myRenderer.enabled = isVisible;

            }
            else
            {
                UnityEngine.Debug.LogError("Renderer ist nicht zugewiesen!");
            }
        }
    }
}