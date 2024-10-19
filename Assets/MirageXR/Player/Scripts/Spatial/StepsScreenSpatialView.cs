using System;
using LearningExperienceEngine.DataModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utility.UiKit.Runtime.Extensions;

namespace MirageXR
{
    public class StepsScreenSpatialView : ScreenView
    {
        [Serializable]
        public class EditorListItem
        {
            public ContentType ContentType;
            public EditorSpatialView EditorView;
        }

        [SerializeField] private Button _buttonBack;

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
    
        [SerializeField] private EditorListItem[] _editorPrefabs;
        //[Space]
        //[Header("Info tab")] TODO

        public void SetActionOnButtonBackClick(UnityAction action) => _buttonBack.SafeSetListener(action);
        public void SetActionOnButtonAddAugmentationClick(UnityAction action) => _buttonAddAugmentation.SafeSetListener(action);
        public Transform GetStepContainer() => _stepContainer;
        public ContetItemView GetContetItemViewPrefab() => _contetItemViewPrefab;

        public void SetActionOnButtonAddImageClick(UnityAction action) => _buttonAddImage.SafeSetListener(action);
        public void SetActionOnButtonAddNewImageClick(UnityAction action) => _buttonAddNewImage.SafeSetListener(action);
        public void SetActionOnButtonDeleteClick(UnityAction action) => _buttonDelete.SafeSetListener(action);

        public void SafeSetImage(Sprite sprite)
        {
            if (_imageMarker != null)
            {
                _imageMarker.sprite = sprite;
            }
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
    }
}
