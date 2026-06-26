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
        [SerializeField] private GameObject _menu;

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
            deleteButton.onClick.AddListener(OnDeleteButtonClick);
            _stepCompletedToggle.onValueChanged.AddListener(OnStepCompleted);
            SetMenuActive(false);
            
            RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged += OnEditorModeChanged;
            OnEditorModeChanged(RootObject.Instance.LEE.ActivityManager.IsEditorMode);
        }

        private void OnDestroy()
        {
            if (_menu != null)
            {
                Destroy(_menu);
            }

            button.onClick.RemoveListener(OnButtonClick);
            buttonMenu.onClick.RemoveListener(OnButtonMenuClick);
            deleteButton.onClick.RemoveListener(OnDeleteButtonClick);
            _stepCompletedToggle.onValueChanged.RemoveListener(OnStepCompleted);
            RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged -= OnEditorModeChanged;
        }

        private void OnEditorModeChanged(bool value)
        {
            _stepCompletedToggle.gameObject.SetActive(!value);
            buttonMenu.gameObject.SetActive(value);
            if (!value)
            {
                SetMenuActive(false);
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
            SetMenuActive(_menu != null && !_menu.activeSelf);
        }

        public void OnStepSelected(bool value)
        {
            _stepSelected.SetActive(value);
        }

        public void OnDeleteButtonClick()
        {
            SetMenuActive(false);
            DeleteStep();
        }

        private void DeleteStep()
        {
            _onMenuClick?.Invoke(_step);
        }

        private void SetMenuActive(bool value)
        {
            if (_menu == null)
            {
                return;
            }

            if (value)
            {
                DetachMenuFromViewport();
                _menu.transform.SetAsLastSibling();
                _menu.SetActive(true);
            }
            else
            {
                _menu.SetActive(false);
            }
        }

        private void DetachMenuFromViewport() //Detaching the menu to avoid it being hidden from viewport
        {
            if (_menu == null)
            {
                return;
            }

            var screenView = GetComponentInParent<NewActivityScreenSpatialView>();
            if (screenView != null)
            {
                SetMenuParent(screenView.transform);
                return;
            }

            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                SetMenuParent(canvas.rootCanvas.transform);
            }
        }

        private void SetMenuParent(Transform parent)
        {
            if (_menu.transform.parent != parent)
            {
                _menu.transform.SetParent(parent, true);
            }
        }
    }
}
