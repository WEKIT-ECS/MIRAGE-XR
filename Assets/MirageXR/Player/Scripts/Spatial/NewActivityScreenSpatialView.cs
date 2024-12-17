using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utility.UiKit.Runtime.Extensions;

namespace MirageXR
{
    public class NewActivityScreenSpatialView : ScreenView
    {
        [Header("Buttons")]
        [SerializeField] private Button _buttonBack;
        [SerializeField] private Button _buttonSettings;
        [SerializeField] private Button _buttonCollaborativeSession;
        [SerializeField] private Button _buttonAddNewStep;
        [SerializeField] private Button _buttonThumbnail;
        [SerializeField] private Button _nextStep;
        [SerializeField] private Button _previousStep;
        [Header("Images")]
        [SerializeField] private RawImage _imageThumbnail;
        [SerializeField] private Image _imageThumbnailTemp;
        [Header("Text")]
        [SerializeField] private TMP_Text textTitle;
        [Header("Input Fields")]
        [SerializeField] private TMP_InputField inputFieldActivityName;
        [SerializeField] private TMP_InputField inputFieldActivityDescription;
        [Header("Toggles")]
        [SerializeField] private Toggle _toggleEditorMode;
        [Header("Containers")]
        [SerializeField] private RectTransform _thumbnaiContainer;
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
        [SerializeField] private GameObject _panelAddNewStep;

        public void SetActionOnInputFieldActivityNameEditEnd(UnityAction<string> action) => inputFieldActivityName.onEndEdit.AddListener(action);
        public void SetActionOnInputFieldActivityDescriptionEditEnd(UnityAction<string> action) => inputFieldActivityDescription.onEndEdit.AddListener(action);
        public void SetInputFieldActivityNameText(string text) => inputFieldActivityName.text = text;
        public void SetInputFieldActivityDescriptionText(string text) => inputFieldActivityDescription.text = text;
        public void SetInputFieldActivityNameTextInteractable(bool value) => inputFieldActivityName.interactable = value;
        public void SetInputFieldActivityDescriptionInteractable(bool value) => inputFieldActivityDescription.interactable = value;
        public void SetActionOnButtonBackClick(UnityAction action) => _buttonBack.SafeSetListener(action);
        public void SetActionOnButtonSettingsClick(UnityAction action) => _buttonSettings.SafeSetListener(action);
        public void SetActionOnButtonCollaborativeSessionClick(UnityAction action) => _buttonCollaborativeSession.SafeSetListener(action);
        public void SetActionOnButtonAddNewStepClick(UnityAction action) => _buttonAddNewStep.SafeSetListener(action);
        public void SetActionOnButtonThumbnailClick(UnityAction action) => _buttonThumbnail.SafeSetListener(action);
        public void SetActionOnButtonNextStepClick(UnityAction action) => _nextStep.SafeSetListener(action);
        public void SetActionOnButtonPreviousStepClick(UnityAction action) => _previousStep.SafeSetListener(action);
        public void RemoveActionOnToggleEditorValueChanged(UnityAction<bool> action) => _toggleEditorMode.SafeRemoveListener(action);
        public void SetActionOnToggleEditorValueChanged(UnityAction<bool> action) => _toggleEditorMode.SafeAddListener(action);
        public void SetIsToggleEditorOn(bool value) => _toggleEditorMode.SafeSetIsOn(value);
        public void SetPanelAddNewStepActive(bool value) => _panelAddNewStep.SetActive(value);
        public Vector2 GetImageThumbnailContainerSize() => _thumbnaiContainer.rect.size;
        public void SetImageThumbnailActive(bool value) => _imageThumbnailTemp.gameObject.SetActive(value);
        //public void SetImageThumbnailTexture(Texture2D texture2D) => _imageThumbnailTemp.texture = texture2D;
        public void SetImageThumbnailTexture(Sprite sprite) => _imageThumbnailTemp.sprite = sprite;
        public void SetTitleText(string text) => textTitle.text = text;
        public void SetImageThumbnailSize(Vector2 size)
        {
            _imageThumbnail.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            _imageThumbnail.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
        }

        public Transform GetStepsContainer()
        {
            return _stepsContainer;
        }

        public StepItemView GetStepsItemPrefab()
        {
            return _stepsItemPrefab;
        }
        
        //#region Calibration tab
        public void SetActionOnButtonWireframeVignetteClick(UnityAction action) => _buttonWireframeVignette.SafeSetListener(action);
        public void SetActionOnButtonAssignRoomModelClick(UnityAction action) => _buttonAssignRoomModel.SafeSetListener(action);
        public void SetActionOnToggleShowRoomScanValueChanged(UnityAction<bool> action) => _toggleShowRoomScan.SafeSetListener(action);
        public void SetActionOnButtonRepositionClick(UnityAction action) => _buttonReposition.SafeSetListener(action);
    }
}
