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
        [Header("Containers")]
        [SerializeField] private Transform _stepsContainer;
        [Header("Prefabs")]
        [SerializeField] private StepItemView _stepsItemPrefab;
        [SerializeField] private ImageEditorSpatialView _imageEditorPrefab;

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

        public ImageEditorSpatialView GetImageEditorPrefab()
        {
            return _imageEditorPrefab;
        }
    }
}
