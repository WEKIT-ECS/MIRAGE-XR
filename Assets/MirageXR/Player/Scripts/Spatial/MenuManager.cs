using UnityEngine;
using UnityEngine.Events;

namespace MirageXR
{
    public class MenuManager : Manager<MenuManager>
    {
        [SerializeField] private SortingScreenSpatialView _sortingScreenSpatialViewPrefab;
        [SerializeField] private RegisterScreenSpatialView _registerScreenSpatialViewPrefab;
        [SerializeField] private SignInScreenSpatialView _signInScreenSpatialViewPrefab;
        
        [SerializeField] private CollaborativeSessionPanelView _collabSessionPanel;
        [SerializeField] private CollaborativeSessionSettingsView _collabSessionSettingsPanel;

        public static UnityEvent<ScreenName, string> ScreenChanged = new();

        private ScreenName _currentScreenName;
        public ScreenName CurrentScreenName
        {
            get => _currentScreenName;
            private set => _currentScreenName = value;
        }

        public void ShowSortingView()
        {
            PopupsViewer.Instance.Show(_sortingScreenSpatialViewPrefab);
        }
        
        public void ShowRegisterView()
        {  
            PopupsViewer.Instance.Show(_registerScreenSpatialViewPrefab);
        }
        
        public void ShowSignInView()
        {  
            PopupsViewer.Instance.Show(_signInScreenSpatialViewPrefab);
        }
        
        public void ShowCollaborativeSessionPanelView()
        {  
            PopupsViewer.Instance.Show(_collabSessionPanel);
        }
        
        public void ShowCollaborativeSessionSettingsPanelView()
        {  
            PopupsViewer.Instance.Show(_collabSessionSettingsPanel);
        }
        public void ShowScreen(ScreenName screenName, string args = "")
        {
            ScreenChanged.Invoke(screenName, args);
            CurrentScreenName = screenName;
        }

        /*public void ShowPopup(PopupName popupName, string args = "")
        {
            PopupChanged.Invoke(popupName, args);
        }*/
    }
}
