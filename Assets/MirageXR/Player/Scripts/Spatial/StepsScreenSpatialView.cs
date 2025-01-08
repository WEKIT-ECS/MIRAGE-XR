using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utility.UiKit.Runtime.Extensions;

namespace MirageXR
{
    public class StepsScreenSpatialView : ScreenView
    {
        [SerializeField] private Button _buttonBack;
        [SerializeField] private GameObject _augmentationToggle; // hide this in view mode
        [SerializeField] private Toggle _editModeToggle;
        [SerializeField] private Button _nextStep;
        [SerializeField] private Button _previousStep;
        
        [Header("Augmentations tab")]
        [SerializeField] private Button _buttonAddAugmentation;
        [SerializeField] private Transform _stepContainer;
        [SerializeField] private ContetItemView _contetItemViewPrefab;
        [Space]
        [Header("Marker tab")]
        [SerializeField] private Button _buttonAddImage;
        [SerializeField] private Button _buttonAddNewImage;
        [SerializeField] private Button _buttonDelete;
        [SerializeField] private Image _imageMarker;
        [Space]
        [Header("Info tab")]
        [Header("Title and description")]
        [SerializeField] private TMP_InputField titleInput;
        [SerializeField] private TMP_InputField descriptionInput; 
        [Header("Media")]
        [SerializeField] private GameObject containerAddNewFile;
        [SerializeField] private Button buttonAddNewFile;
        [SerializeField] private Transform containerMedia;
        [SerializeField] private StepsMediaListItemView stepsMediaListItemViewPrefab;
        [Header("Tools")]
        [SerializeField] private GameObject containerAddNewTool;
        [SerializeField] private Button buttonAddNewTool;
        [SerializeField] private Transform containerTools;
        [SerializeField] private StepsToolsListItemView stepsToolsListItemViewPrefab;
        [Header("Game Objects")]
        [SerializeField] private GameObject _panelAddNewStep;
        [Header("Toggles")]
        [SerializeField] private Toggle _toggleEditorMode;

        public void SetActionOnToggleEditModeValueChanged(UnityAction<bool> action) => _editModeToggle.SafeSetListener(action);
        public void SetActionOnButtonBackClick(UnityAction action) => _buttonBack.SafeSetListener(action);
        public void SetActionOnButtonAddAugmentationClick(UnityAction action) => _buttonAddAugmentation.SafeSetListener(action);
        public void SetActionOnButtonNextStepClick(UnityAction action) => _nextStep.SafeSetListener(action);
        public void SetActionOnButtonPreviousStepClick(UnityAction action) => _previousStep.SafeSetListener(action);
        public Transform GetStepContainer() => _stepContainer;
        public ContetItemView GetContetItemViewPrefab() => _contetItemViewPrefab;

        public void SetActionOnButtonAddImageClick(UnityAction action) => _buttonAddImage.SafeSetListener(action);
        public void SetActionOnButtonAddNewImageClick(UnityAction action) => _buttonAddNewImage.SafeSetListener(action);
        public void SetActionOnButtonDeleteClick(UnityAction action) => _buttonDelete.SafeSetListener(action);

        public void SetActionOnToggleEditorValueChanged(UnityAction<bool> action) => _toggleEditorMode.SafeAddListener(action);
        public void RemoveActionOnToggleEditorValueChanged(UnityAction<bool> action) => _toggleEditorMode.SafeRemoveListener(action);
        public void SetIsToggleEditorOn(bool value) => _toggleEditorMode.isOn = value;
        public void SetPanelAddNewStepActive(bool value) => _panelAddNewStep.SetActive(value);

        public void SetActionOnTitleInputEndEdit(UnityAction<string> onEndEdit) => titleInput.onEndEdit.AddListener(onEndEdit);
        public void SetTitleInputText(string text) => titleInput.text = text;
        public void SetTitleInputInteractable(bool value) => titleInput.interactable  = value;
        public void SetActionOnDescriptionInputEndEdit(UnityAction<string> onEndEdit) => descriptionInput.onEndEdit.AddListener(onEndEdit);
        public void SetDescriptionInputText(string text) => descriptionInput.text = text;
        public void SetDescriptionInputInteractable(bool value) => descriptionInput.interactable  = value;

        public void SetActiveContainerMediaAddNewFile(bool value) => containerAddNewFile.SetActive(value);
        public void SetActionOnButtonMediaAddNewFileClick(UnityAction action) => buttonAddNewFile.SafeSetListener(action);
        public Transform GetContainerMedia() => containerMedia;
        public StepsMediaListItemView GetStepsMediaListItemViewPrefab() => stepsMediaListItemViewPrefab;

        public void SetActiveContainerToolsAddNewToolClick(bool value) => containerAddNewTool.SetActive(value);
        public void SetActionOnButtonToolsAddNewToolClick(UnityAction action) => buttonAddNewTool.SafeSetListener(action);
        public Transform GetContainerTools() => containerTools;
        public StepsToolsListItemView GetStepsToolsListItemViewPrefab() => stepsToolsListItemViewPrefab;
    }
}
