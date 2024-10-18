using System;
using LearningExperienceEngine.DataModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utility.UiKit.Runtime.Extensions;

namespace MirageXR
{
    public class NewActivityScreenSpatialView : ScreenView
    {
        [Serializable]
        public class EditorListItem
        {
            public ContentType ContentType;
            public EditorSpatialView EditorView;
        }

        [Header("Buttons")]
        [SerializeField] private Button _buttonBack;
        [SerializeField] private Button _buttonSettings;
        [SerializeField] private Button _buttonCollaborativeSession;
        [SerializeField] private Button _buttonAddNewStep;
        [Header("Containers")]
        [SerializeField] private Transform _stepsContainer;
        [Header("Prefabs")]
        [SerializeField] private StepItemView _stepsItemPrefab;
        [SerializeField] private ImageEditorSpatialView _imageEditorPrefab;
        [Space]
        [Header("Calibration tab")]
        [Header("Buttons and Toggles")]
        [SerializeField] private Button _buttonWireframeVignette;
        [SerializeField] private Button _buttonAssignRoomModel;
        [SerializeField] private Toggle _toggleShowRoomScan;
        [SerializeField] private Button _buttonReposition;
        [Header("Game Objects")]
        [SerializeField] private GameObject _panelCalibrated;
        [SerializeField] private GameObject _panelNotCalibrated;
        [SerializeField] private GameObject _panelRoomScanAdded;
        [SerializeField] private GameObject _panelRoomScanNotAdded;
        [SerializeField] private GameObject _panelHeightNotCalibrated;
        [SerializeField] private EditorListItem[] _editorPrefabs;

        public void SetActionOnButtonBackClick(UnityAction action) => _buttonBack.SafeSetListener(action);
        public void SetActionOnButtonSettingsClick(UnityAction action) => _buttonSettings.SafeSetListener(action);
        public void SetActionOnButtonCollaborativeSessionClick(UnityAction action) => _buttonCollaborativeSession.SafeSetListener(action);
        public void SetActionOnButtonAddNewStepClick(UnityAction action) => _buttonAddNewStep.SafeSetListener(action);

        public Transform GetStepsContainer()
        {
            return _stepsContainer;
        }

        public StepItemView GetStepsItemPrefab()
        {
            return _stepsItemPrefab;
        }

        public EditorSpatialView GetEditorPrefab(ContentType contentType)
        {
            foreach (var editorListItem in _editorPrefabs)
            {
                if (editorListItem.ContentType == contentType)
                {
                    return editorListItem.EditorView;
                }
            }

            return null;
        }
        
        //#region Calibration tab
        public void SetActionOnButtonWireframeVignetteClick(UnityAction action) => _buttonWireframeVignette.SafeSetListener(action);
        public void SetActionOnButtonAssignRoomModelClick(UnityAction action) => _buttonAssignRoomModel.SafeSetListener(action);
        public void SetActionOnToggleShowRoomScanValueChanged(UnityAction<bool> action) => _toggleShowRoomScan.SafeSetListener(action);
        public void SetActionOnButtonRepositionClick(UnityAction action) => _buttonReposition.SafeSetListener(action);
    }
}
