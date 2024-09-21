using UnityEngine;
using UnityEngine.UI;
using Utility.UiKit.Runtime.Extensions;

namespace MirageXR
{
    public class SidebarView : MonoBehaviour
    {
        [SerializeField] private Button _buttonSidebarCollapse;
        [SerializeField] private Button _buttonSidebarExpand;
        [SerializeField] private GameObject _sidebarOpened;
        [SerializeField] private GameObject _sidebarClosed;
        
        [Header ("Sidebar open. Toggles")]
        [SerializeField] private Toggle _toggleActivities;
        [SerializeField] private Toggle _toggleDashboard;
        [SerializeField] private Toggle _toggleProfile;
        [SerializeField] private Toggle _toggleInfo;
        
        [Header("Sidebar closed. Toggles")]
        [SerializeField] private Toggle _toggleSidebarClosedActivities;
        [SerializeField] private Toggle _toggleSidebarClosedDashboard;
        [SerializeField] private Toggle _toggleSidebarClosedProfile;
        [SerializeField] private Toggle _toggleSidebarClosedInfo;

        
        private void Awake()
        {
            SetActiveState(_sidebarOpened, true);
            SetActiveState(_sidebarClosed, false);
            _buttonSidebarCollapse.SafeSetListener(OnSidebarCollapse);
            _buttonSidebarExpand.SafeSetListener(OnSidebarExpand);
            
            _toggleActivities.SafeAddListener(OnToggleActivities);
            _toggleDashboard.SafeAddListener(OnToggleDashboard);
            _toggleProfile.SafeAddListener(OnToggleProfile);
            _toggleInfo.SafeAddListener(OnToggleInfo);
            
            _toggleSidebarClosedActivities.SafeAddListener(OnToggleSidebarClosedActivities);
            _toggleSidebarClosedDashboard.SafeAddListener(OnToggleSidebarClosedDashboard);
            _toggleSidebarClosedProfile.SafeAddListener(OnToggleSidebarClosedProfile);
            _toggleSidebarClosedInfo.SafeAddListener(OnToggleSidebarClosedInfo);
        }
        
        private void SetActiveState(GameObject obj, bool state)
        {
            if (obj != null) obj.SetActive(state);
        }

        private void OnToggleSidebarClosedInfo(bool value)
        {
            if (!value) return;
            // TODO
        }

        private void OnToggleSidebarClosedProfile(bool value)
        {
            ColorBlock cb = _toggleSidebarClosedProfile.colors;
            if (value)
            {
                cb.normalColor = Color.gray;
                cb.highlightedColor = Color.gray;
            }
            else
            {
                cb.normalColor = Color.white;
                cb.highlightedColor = Color.white;
            }
            _toggleSidebarClosedProfile.colors = cb;
            
            if (!value) return;
            MenuManager.Instance.ShowScreen(ScreenName.ProfileScreen);
        }

        private void OnToggleSidebarClosedDashboard(bool value)
        {
            // TODO
        }

        private void OnToggleSidebarClosedActivities(bool value)
        {
            ColorBlock cb = _toggleSidebarClosedActivities.colors;
            if (value)
            {
                cb.normalColor = Color.gray;
                cb.highlightedColor = Color.gray;
            }
            else
            {
                cb.normalColor = Color.white;
                cb.highlightedColor = Color.white;
            }
            _toggleSidebarClosedActivities.colors = cb;
            
            if (!value) return;
            MenuManager.Instance.ShowScreen(ScreenName.MainScreen);
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

        private void OnSidebarExpand()
        {
            _sidebarOpened.SetActive(true);
            _sidebarClosed.SetActive(false);
        }

        private void OnSidebarCollapse()
        {
            _sidebarOpened.SetActive(false);
            _sidebarClosed.SetActive(true);
        }
    }
}

