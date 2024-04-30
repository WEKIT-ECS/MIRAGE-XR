using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class VisibilityManager : MonoBehaviour
    {
        public static VisibilityManager Instance;

        private List<TaskStationVisibilityController> controllers = new List<TaskStationVisibilityController>();

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);  // Optional, wenn der Manager über Szenen hinweg bestehen bleiben soll
            }
        }

        public void RegisterController(TaskStationVisibilityController controller)
        {
            if (!controllers.Contains(controller))
            {
                controllers.Add(controller);
            }
        }

        public void UnregisterController(TaskStationVisibilityController controller)
        {
            controllers.Remove(controller);
        }

        public void SetAllVisibility(bool isVisible)
        {
            foreach (var controller in controllers)
            {
                controller.SetVisibility(isVisible);
            }
        }
    }
}