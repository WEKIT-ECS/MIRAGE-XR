using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class ToggleVisibilityController : MonoBehaviour
    {
        public Toggle myToggle;

        void Start()
        {
            if (myToggle == null)
            {
                Debug.LogError("Toggle is not assigned.", this);
                this.enabled = false;
                return;
            }

            myToggle.isOn = true;

            
            myToggle.onValueChanged.AddListener(isOn => VisibilityManager.Instance.SetAllVisibility(isOn));
        }
    }
}