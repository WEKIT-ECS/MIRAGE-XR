using UnityEngine;

namespace MirageXR
{
    /// <summary>
    /// Toggles the follow me behavior of a RadialView. Native replacement for MRTK FollowMeToggle.
    /// </summary>
    public class FollowMeToggle : MonoBehaviour
    {
        [SerializeField] private RadialView _radialView;

        private void Awake()
        {
            if (_radialView == null)
            {
                _radialView = GetComponent<RadialView>();
            }
        }

        public void SetFollowMeBehavior(bool follow)
        {
            if (_radialView == null)
            {
                _radialView = GetComponent<RadialView>();
            }

            if (_radialView != null)
            {
                _radialView.enabled = follow;
            }
        }
    }
}
