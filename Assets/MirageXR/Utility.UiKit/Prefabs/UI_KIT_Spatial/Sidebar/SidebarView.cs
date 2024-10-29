using UnityEngine;
using UnityEngine.UI;
using Utility.UiKit.Runtime.Extensions;

namespace MirageXR
{
    public class SidebarView : MonoBehaviour
    {
        [Header ("Sidebar. Toggles")]
        [SerializeField] private Toggle _toggleActivities;
        [SerializeField] private Toggle _toggleDashboard;
        [SerializeField] private Toggle _toggleProfile;
        [SerializeField] private Toggle _toggleInfo;
        [Space]
        [SerializeField] private ToggleGroup _toggleGroup;
        
        private void Awake()
        {
            MenuManager.ScreenChanged.AddListener(OnScreenChanged);
            
            _toggleActivities.SafeAddListener(OnToggleActivities);
            _toggleDashboard.SafeAddListener(OnToggleDashboard);
            _toggleProfile.SafeAddListener(OnToggleProfile);
            _toggleInfo.SafeAddListener(OnToggleInfo);
        }

        private void OnToggleInfo(bool value)
        {
            // TODO
        }

        private void OnToggleProfile(bool value)
        {
            if (!value) return;
            MenuManager.Instance.ShowScreen(ScreenName.ProfileScreen);
        }

        private void OnToggleDashboard(bool value)
        {
            // TODO
        }

        private void OnToggleActivities(bool value)
        {
            if (!value) return;
            MenuManager.Instance.ShowScreen(ScreenName.MainScreen);
        }
        
        protected virtual void OnScreenChanged(ScreenName screenName, string args)
        {
        }
    }
}

