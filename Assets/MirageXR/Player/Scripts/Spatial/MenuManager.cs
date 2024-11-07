using System;
using System.Collections.Generic;
using System.Linq;
using LearningExperienceEngine.DataModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MirageXR
{
    public class MenuManager : Manager<MenuManager>
    {
        [Serializable]
        public class EditorListItem
        {
            public ContentType ContentType;
            public EditorSpatialView EditorView;
        }

        [SerializeField] private SortingScreenSpatialView _sortingScreenSpatialViewPrefab;
        [SerializeField] private RegisterScreenSpatialView _registerScreenSpatialViewPrefab;
        //[SerializeField] private LoginView_Spatial _signInScreenSpatialViewPrefab;  // TODO SignInScreenSpatialView
        
        [SerializeField] private CollaborativeSessionPanelView _collabSessionPanel;
        [SerializeField] private CollaborativeSessionSettingsView _collabSessionSettingsPanel;
        [SerializeField] private RoomScanSettingsSpatialView _roomScanSettingsSpatialViewPrefab;
        [SerializeField] private SelectAugmentationScreenSpatialView _selectAugmentationScreenSpatialView;

        [SerializeField] private EditorListItem[] editorPrefabs;
        
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
            //PopupsViewer.Instance.Show(_signInScreenSpatialViewPrefab);
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
        
        public void ShowRoomScanSettingsPanelView()
        {  
            PopupsViewer.Instance.Show(_roomScanSettingsSpatialViewPrefab);
        }

        public void ShowSelectAugmentationScreenSpatialView()
        {
            var types = editorPrefabs.Select(t => t.ContentType).ToList();
            PopupsViewer.Instance.Show(_selectAugmentationScreenSpatialView, types);
        }

        /*public void ShowPopup(PopupName popupName, string args = "")
        {
            PopupChanged.Invoke(popupName, args);
        }*/

        public EditorSpatialView GetEditorPrefab(ContentType contentType)
        {
            foreach (var editorListItem in editorPrefabs)
            {
                if (editorListItem.ContentType == contentType)
                {
                    return editorListItem.EditorView;
                }
            }

            return null;
        }

        public EditorListItem[] GetEditorPrefabs()
        {
            return editorPrefabs;
        }
    }
}
