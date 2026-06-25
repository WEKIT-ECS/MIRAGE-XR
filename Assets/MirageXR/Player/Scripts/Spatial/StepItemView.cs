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
        [SerializeField] private Button deleteButton;
        [SerializeField] private Toggle _stepCompletedToggle;
        [SerializeField] private GameObject _stepSelected;

        private ActivityStep _step;
        private UnityAction<ActivityStep> _onClick;
        private UnityAction<ActivityStep> _onMenuClick;
        private ButtonLongPress _deleteButtonLongPress;
        private Graphic _deleteButtonGraphic;
        private Color _deleteButtonDefaultColor;

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
            deleteButton.onClick.AddListener(OnDeleteButtonClick);
            _stepCompletedToggle.onValueChanged.AddListener(OnStepCompleted);
            
            InitializeDeleteLongPress();
            RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged += OnEditorModeChanged;
            OnEditorModeChanged(RootObject.Instance.LEE.ActivityManager.IsEditorMode);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnButtonClick);
            buttonMenu.onClick.RemoveListener(OnButtonMenuClick);
            deleteButton.onClick.RemoveListener(OnDeleteButtonClick);
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
            deleteButton.gameObject.SetActive(value);
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
            _onMenuClick?.Invoke(_step); //TODO this just deletes the step at the moment
        }

        public void OnStepSelected(bool value)
        {
            _stepSelected.SetActive(value);
        }

        public void OnDeleteButtonClick()
        {
            if (_deleteButtonLongPress != null && _deleteButtonLongPress.ConsumeLongPress())
            {
                DeleteStep();
            }
        }

        private void InitializeDeleteLongPress()
        {
            _deleteButtonLongPress = deleteButton.GetComponent<ButtonLongPress>();
            _deleteButtonLongPress.onHoldProgressChanged.RemoveListener(OnDeleteHoldProgressChanged);
            _deleteButtonLongPress.onHoldProgressChanged.AddListener(OnDeleteHoldProgressChanged);
            _deleteButtonGraphic = deleteButton.targetGraphic != null ? deleteButton.targetGraphic : deleteButton.GetComponent<Graphic>();
            if (_deleteButtonGraphic != null)
            {
                _deleteButtonDefaultColor = _deleteButtonGraphic.color;
            }
        }

        private void OnDeleteHoldProgressChanged(float progress)
        {
            if (_deleteButtonLongPress == null || _deleteButtonGraphic == null)
            {
                return;
            }

            _deleteButtonLongPress.holdColor.a = _deleteButtonDefaultColor.a;
            _deleteButtonGraphic.color = progress >= 1f ? _deleteButtonLongPress.holdColor : Color.Lerp(_deleteButtonDefaultColor, _deleteButtonLongPress.holdColor, progress);
        }

        private void DeleteStep()
        {
            _onMenuClick?.Invoke(_step);
        }
    }
}
