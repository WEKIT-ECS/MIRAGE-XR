using LearningExperienceEngine.DataModel;
using TMPro;
using UnityEngine;
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
            _defaultBackgroundColor = backgroundImage.color;
            HideDeleteConfirmation();
            InitializeDeleteLongPress();
            
            RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged += OnEditorModeChanged;
            OnEditorModeChanged(RootObject.Instance.LEE.ActivityManager.IsEditorMode);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnButtonClick);
            buttonMenu.onClick.RemoveListener(OnButtonMenuClick);
            buttonDelete.onClick.RemoveListener(OnButtonDeleteClick);
            buttonDeleteConfirmation.onClick.RemoveListener(DeleteStep);
            buttonDeleteCancel.onClick.RemoveListener(HideDeleteConfirmation);
            _stepCompletedToggle.onValueChanged.RemoveListener(OnStepCompleted);
            RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged -= OnEditorModeChanged;
            if (_deleteButtonLongPress == null)
            {
                return;
            }

            _deleteButtonLongPress.onHoldProgressChanged.RemoveListener(OnDeleteHoldProgressChanged);
        }

        private void OnEditorModeChanged(bool value)
        {
            _stepCompletedToggle.gameObject.SetActive(!value);
            buttonMenu.gameObject.SetActive(value);
            if (!value)
            {
                HideDeleteConfirmation();
            }
        }

        private void OnStepCompleted(bool arg0)
        {
            // TODO
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
