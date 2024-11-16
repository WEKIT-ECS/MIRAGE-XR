using System;
using LearningExperienceEngine.DataModel;
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
        [Header("Marker tab")]
        [Header("Title and description")]
        [SerializeField] private GameObject _description_EditMode;
        [SerializeField] private GameObject _description_ViewMode;
        [SerializeField] private TMP_InputField _titleInput;
        [SerializeField] private TMP_InputField _descriptionInput; 
        [SerializeField] private TMP_Text _descriptionText;
        [Space]
        [Header("Media")]
        [SerializeField] private GameObject _media_EditMode;
        [SerializeField] private GameObject _media_ViewMode;
        [SerializeField] private Image _image_EditMode;
        [SerializeField] private Image _image_ViewMode;
        [SerializeField] private Button _buttonAddNewFile;
        [SerializeField] private Button _buttonDeleteFile;
        [Header("Tools")]
        [SerializeField] private GameObject _tools_EditMode;
        [SerializeField] private GameObject _tools_ViewMode;
        [SerializeField] private GameObject _toolsTextPrefab;
        [SerializeField] private Transform _textContainer_EditMode;
        [SerializeField] private Button _buttonAddNewObject;
        [SerializeField] private Transform _textContainer_ViewMode;

        public void SetActionOnToggleEditModeValueChanged(UnityAction<bool> action) => _editModeToggle.SafeSetListener(action);
        public void SetActionOnButtonBackClick(UnityAction action) => _buttonBack.SafeSetListener(action);
        public void SetActionOnButtonAddAugmentationClick(UnityAction action) => _buttonAddAugmentation.SafeSetListener(action);
        public Transform GetStepContainer() => _stepContainer;
        public ContetItemView GetContetItemViewPrefab() => _contetItemViewPrefab;

        public void SetActionOnButtonAddImageClick(UnityAction action) => _buttonAddImage.SafeSetListener(action);
        public void SetActionOnButtonAddNewImageClick(UnityAction action) => _buttonAddNewImage.SafeSetListener(action);
        public void SetActionOnButtonDeleteClick(UnityAction action) => _buttonDelete.SafeSetListener(action);
        
        public void SetActionOnInputFieldTitleValueChanged(UnityAction<string> action) => _titleInput.SafeSetListener(action);
        public void SetActionOnInputFieldDescriptionValueChanged(UnityAction<string> action) => _descriptionInput.SafeSetListener(action);
        public void SetDescriptionText(string text) => _descriptionText.SafeSetText(text);
        public void SafeSetMediaImageEditMode(Sprite sprite)
        {
            if (_image_EditMode != null)
            {
                _image_EditMode.sprite = sprite;
            }
        }
        public void SafeSetMediaImageViewMode(Sprite sprite)
        {
            if (_image_ViewMode != null)
            {
                _image_ViewMode.sprite = sprite;
            }
        }
        
        public void SetActionOnButtonAddNewFileClick(UnityAction action) => _buttonAddNewFile.SafeSetListener(action);
        public void SetActionOnButtonDeleteFileClick(UnityAction action) => _buttonDeleteFile.SafeSetListener(action);
        public void SetActionOnButtonAddNewObjectClick(UnityAction action) => _buttonAddNewObject.SafeSetListener(action);

        public void SafeSetImage(Sprite sprite)
        {
            if (_imageMarker != null)
            {
                _imageMarker.sprite = sprite;
            }
        }
    }
}
