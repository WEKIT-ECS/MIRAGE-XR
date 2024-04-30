using UnityEngine;

namespace MirageXR
{
    public class TaskStationVisibilityController : MonoBehaviour
    {
        private Renderer myRenderer;

        void Awake()
        {
            myRenderer = GetComponentInChildren<Renderer>();
            if (myRenderer == null)
            {
                Debug.LogError("No Renderer found in Game Object", this);
                this.enabled = false;
                return;
            }
            VisibilityManager.Instance.RegisterController(this);
        }

        void OnDestroy()
        {
            if (VisibilityManager.Instance != null)
            {
                VisibilityManager.Instance.UnregisterController(this);
            }
        }

        public void SetVisibility(bool isVisible)
        {
            if (myRenderer != null)
            {
                myRenderer.enabled = isVisible;
            }
        }
    }
}