using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MirageXR
{
    public class StepItemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text textStepName;
        [SerializeField] private TMP_Text textStepDescription;
        [SerializeField] private Button button;
        [SerializeField] private Button buttonMenu;
        [SerializeField] private Button buttonDelete;
        [SerializeField] private Button _buttonLock;
        [SerializeField] private Image _imageLock;
        [SerializeField] private Sprite _spriteLock;
        [SerializeField] private Sprite _spriteUnlock;
        [SerializeField] private Toggle _stepCompletedToggle;
        [SerializeField] private GameObject _stepSelected;
        [SerializeField] private GameObject deleteConfirmation;
        [SerializeField] private Button buttonDeleteConfirmation;
        [SerializeField] private Button buttonDeleteCancel;
        [SerializeField] private Image backgroundImage;

        private ActivityStep _step;
        private UnityAction<ActivityStep> _onClick;
        private UnityAction<ActivityStep> _onMenuClick;
        private ButtonLongPress _deleteButtonLongPress;
        private Color _defaultBackgroundColor;

        public void Initialize(ActivityStep step, UnityAction<ActivityStep> onClick, UnityAction<ActivityStep> onMenuClick)
        {
            _step = step;
            _onClick = onClick;
            _onMenuClick = onMenuClick;
            var number = RootObject.Instance.LEE.StepManager.GetStepNumber(_step.Id);
            textStepName.text = $"{number} {step.Name}";
            var data = HyperlinkPositionData.SplitPositionsFromText(_step.Description);
            textStepDescription.text = data.DisplayText;
            button.onClick.AddListener(OnButtonClick);
            buttonMenu.onClick.AddListener(OnButtonMenuClick);
            buttonDelete.onClick.AddListener(OnButtonDeleteClick);
            buttonDeleteConfirmation.onClick.AddListener(DeleteStep);
            buttonDeleteCancel.onClick.AddListener(HideDeleteConfirmation);
            _stepCompletedToggle.onValueChanged.AddListener(OnStepCompleted);
            if (_step != null)
            {
                _stepCompletedToggle.SetIsOnWithoutNotify(StepsListItem_v2.IsStepCompleted(_step.Id.ToString()));
            }
            _defaultBackgroundColor = backgroundImage.color;
            HideDeleteConfirmation();
            InitializeDeleteLongPress();

            if (_buttonLock != null)
            {
                _buttonLock.onClick.RemoveAllListeners();
                _buttonLock.onClick.AddListener(OnLockClick);
            }
            UpdateLockVisual();
            
            RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged += OnEditorModeChanged;
            OnEditorModeChanged(RootObject.Instance.LEE.ActivityManager.IsEditorMode);
        }

        private void OnDestroy()
        {
            if (button != null) button.onClick.RemoveListener(OnButtonClick);
            if (buttonMenu != null) buttonMenu.onClick.RemoveListener(OnButtonMenuClick);
            if (buttonDelete != null) buttonDelete.onClick.RemoveListener(OnButtonDeleteClick);
            if (buttonDeleteConfirmation != null) buttonDeleteConfirmation.onClick.RemoveListener(DeleteStep);
            if (buttonDeleteCancel != null) buttonDeleteCancel.onClick.RemoveListener(HideDeleteConfirmation);
            if (_stepCompletedToggle != null) _stepCompletedToggle.onValueChanged.RemoveListener(OnStepCompleted);
            if (_buttonLock != null)
            {
                _buttonLock.onClick.RemoveListener(OnLockClick);
            }
            if (RootObject.Instance?.LEE?.ActivityManager != null)
            {
                RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged -= OnEditorModeChanged;
            }
            if (_deleteButtonLongPress != null)
            {
                _deleteButtonLongPress.onHoldProgressChanged.RemoveListener(OnDeleteHoldProgressChanged);
            }
        }

        private void OnEditorModeChanged(bool value)
        {
            _stepCompletedToggle.gameObject.SetActive(!value);
            buttonMenu.gameObject.SetActive(value);
            if (_buttonLock != null)
            {
                _buttonLock.gameObject.SetActive(value);
            }
            if (!value)
            {
                HideDeleteConfirmation();
            }
        }

        public void OnLockClick()
        {
            if (_step == null) return;
            if (_step.Location == null)
            {
                _step.Location = new Location();
            }

            _step.Location.IsLocked = !_step.Location.IsLocked;

            // If this is the active step, update StepView in the scene
            var currentStep = RootObject.Instance.LEE.StepManager.CurrentStep;
            if (currentStep != null && _step.Id == currentStep.Id)
            {
                var stepView = RootObject.Instance.ViewManager.ActivityView.StepView;
                if (stepView != null)
                {
                    stepView.SetLocked(_step.Location.IsLocked, cascadeToContents: true);
                }
            }

            RootObject.Instance.LEE.StepManager.UpdateStep(_step);

            // Cascade lock state to all contents belonging to this step
            var allContents = RootObject.Instance.LEE.ContentManager.GetContents();
            if (allContents != null)
            {
                var stepContents = new List<Content>();
                foreach (var content in allContents)
                {
                    if (content.Steps != null && content.Steps.Contains(_step.Id))
                    {
                        if (content.Location == null)
                        {
                            content.Location = new Location();
                        }
                        content.Location.IsLocked = _step.Location.IsLocked;
                        stepContents.Add(content);
                    }
                }

                if (stepContents.Count > 0)
                {
                    RootObject.Instance.LEE.ContentManager.UpdateContents(stepContents);
                }
            }

            UpdateLockVisual();
        }

        private void UpdateLockVisual()
        {
            if (_imageLock != null)
            {
                bool isLocked = _step?.Location?.IsLocked ?? false;
                _imageLock.sprite = isLocked ? _spriteLock : _spriteUnlock;
            }
        }

        private void OnStepCompleted(bool isToggled)
        {
            if (isToggled)
            {
                _stepCompletedToggle.SetIsOnWithoutNotify(false);
                StepVerificationController.CheckStepCompletedAsync(
                    _step,
                    isSpatial: true,
                    onResult: (success) =>
                    {
                        if (_stepCompletedToggle != null)
                        {
                            _stepCompletedToggle.SetIsOnWithoutNotify(success);
                        }
                    }
                ).Forget();
            }
            else
            {
                if (_step != null)
                {
                    StepVerificationController.SetStepCompleted(_step.Id.ToString(), false);
                }
            }
        }

        private void OnButtonClick()
        {
            _onClick?.Invoke(_step);
        }

        private void OnButtonMenuClick()
        {
            //TODO
        }

        private void OnButtonDeleteClick()
        {
            _deleteButtonLongPress?.ConsumeLongPress();
        }

        private void InitializeDeleteLongPress()
        {
            _deleteButtonLongPress = buttonDelete.GetComponent<ButtonLongPress>();
            _deleteButtonLongPress.onHoldProgressChanged.AddListener(OnDeleteHoldProgressChanged);
        }

        private void OnDeleteHoldProgressChanged(float progress)
        {
            if (deleteConfirmation != null && deleteConfirmation.activeSelf)
            {
                backgroundImage.color = GetDeleteHoldColor();
                return;
            }

            if (progress >= 1f)
            {
                ShowDeleteConfirmation();
                return;
            }

            backgroundImage.color = Color.Lerp(_defaultBackgroundColor, GetDeleteHoldColor(), progress);
        }

        private void ShowDeleteConfirmation()
        {
            if (deleteConfirmation != null)
            {
                deleteConfirmation.SetActive(true);
            }

            backgroundImage.color = GetDeleteHoldColor();
        }

        private void HideDeleteConfirmation()
        {
            if (deleteConfirmation != null)
            {
                deleteConfirmation.SetActive(false);
            }

            backgroundImage.color = _defaultBackgroundColor;
        }

        private Color GetDeleteHoldColor()
        {
            var color = _deleteButtonLongPress.holdColor;
            color.a = _defaultBackgroundColor.a;
            return color;
        }

        public void OnStepSelected(bool value)
        {
            _stepSelected.SetActive(value);
        }

        private void DeleteStep()
        {
            _onMenuClick?.Invoke(_step);
        }
    }
}
