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
        [SerializeField] private Toggle _stepCompletedToggle;
        [SerializeField] private GameObject _stepSelected;

        private ActivityStep _step;
        private UnityAction<ActivityStep> _onClick;
        private UnityAction<ActivityStep> _onMenuClick;

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
            _stepCompletedToggle.onValueChanged.AddListener(OnStepCompleted);
            
            RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged += OnEditorModeChanged;
        }

        private void OnEditorModeChanged(bool value)
        {
            _stepCompletedToggle.gameObject.SetActive(!value);
            buttonMenu.gameObject.SetActive(value);
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
            _onMenuClick?.Invoke(_step);
        }

        public void OnStepSelected(bool value)
        {
            _stepSelected.SetActive(value);
        }
    }
}
