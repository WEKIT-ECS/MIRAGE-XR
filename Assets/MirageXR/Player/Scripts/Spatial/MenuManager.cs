using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MirageXR
{
    public class MenuManager : Manager<MenuManager>
    {
        [SerializeField] private SortingScreenSpatialView _sortingScreenSpatialViewPrefab;
        [SerializeField] private RegisterScreenSpatialView _registerScreenSpatialViewPrefab;
        [SerializeField] private SignInScreenSpatialView _signInScreenSpatialViewPrefab;

        public static UnityEvent<ScreenName, string> ScreenChanged = new();
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
        
        public void ShowScreen(ScreenName screenName, string args = "")
        {
            ScreenChanged.Invoke(screenName, args);
            
            /*var dictionary = new Dictionary<string, object>
            {
                { "screen_name", screenName.ToString() }
            };*/
        }

        /*public void ShowPopup(PopupName popupName, string args = "")
        {
            PopupChanged.Invoke(popupName, args);
        }*/
    }
}
