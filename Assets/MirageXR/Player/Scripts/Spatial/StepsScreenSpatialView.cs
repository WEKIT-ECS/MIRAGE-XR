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
        
        [Header("Augmentations tab")]
        [Header("Buttons")]
        [SerializeField] private Button _buttonGhostTrack;
        [SerializeField] private Button _buttonAction;
        [SerializeField] private Button _button3DModel;
        [SerializeField] private Button _buttonAddAugmentation;
        [Header("Tesxts")]
        [SerializeField] private TMP_Text _textGhostTrack;
        [SerializeField] private TMP_Text _textAction;
        [SerializeField] private TMP_Text _text3DModel;
        [Space]
        [Header("Marker tab")]
        [Header("Buttons")]
        [SerializeField] private Button _buttonAddImage;
        [SerializeField] private Button _buttonAddNewImage;
        [SerializeField] private Button _buttonDelete;
        [Header("Image")]
        [SerializeField] private Image _imageMarker;
        //[Space]
        //[Header("Info tab")] TODO
        
        public void SetActionOnButtonBackClick(UnityAction action) => _buttonBack.SafeSetListener(action);
        public void SetActionOnButtonGhostTrackClick(UnityAction action) => _buttonGhostTrack.SafeSetListener(action);
        public void SetActionOnButtonActionClick(UnityAction action) => _buttonAction.SafeSetListener(action);
        public void SetActionOnButton3DModelClick(UnityAction action) => _button3DModel.SafeSetListener(action);
        
        public void SetGhostTrackText(string text) => _textGhostTrack.SafeSetText(text);
        public void SetActionText(string text) => _textAction.SafeSetText(text);
        public void Set3DModelText(string text) => _text3DModel.SafeSetText(text);
        
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
    }
}
