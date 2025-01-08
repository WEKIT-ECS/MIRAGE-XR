using System.Collections.Generic;
using LearningExperienceEngine.DataModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class InfoScreenSpatialView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _textTitle;
        [SerializeField] private TMP_Text _textTitle_Collapsed;
        [SerializeField] private TMP_Text _textDescription;
        [SerializeField] private StepsMediaListItemView stepsMediaListItemViewPrefab;
        [SerializeField] private Transform containerMedia;
        [SerializeField] private Toggle collapsePanelToggle;
        [SerializeField] private Toggle collapsePanelToggle_Collapsed;
        [SerializeField] private Toggle stepCompletedToggle;
        [SerializeField] private Toggle stepCompletedToggle_Collapsed;
        [SerializeField] private GameObject _mainScreen;
        [SerializeField] private GameObject _mainScreen_Collapsed;
        [SerializeField] private Button previousStepButton;
        [SerializeField] private Button nextStepButton;
        [SerializeField] private Button previousStepButton_Collapsed;
        [SerializeField] private Button nextStepButton_Collapsed;
        
        private readonly List<StepsMediaListItemView> _mediaListItemViews = new();
        
        private ActivityStep _step;
        private Camera _camera;

        public void Initialize(ActivityStep step)
        {
            RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged += OnEditorModeChanged;
            nextStepButton.onClick.AddListener(OnNextStepClicked);
            previousStepButton.onClick.AddListener(OnPreviousStepClicked);
            nextStepButton_Collapsed.onClick.AddListener(OnNextStepClicked);
            previousStepButton_Collapsed.onClick.AddListener(OnPreviousStepClicked);
            collapsePanelToggle.onValueChanged.AddListener(OnStepCompletedToggleValueChanged);
            collapsePanelToggle_Collapsed.onValueChanged.AddListener(OnStepCompletedToggleCollapsedValueChanged);
            UpdateView(step); 
        }
        
        private void OnEditorModeChanged(bool value)
        {
            stepCompletedToggle.gameObject.SetActive(!value);
            stepCompletedToggle_Collapsed.gameObject.SetActive(!value);
        }

        private void OnStepCompletedToggleValueChanged(bool value)
        {
            _mainScreen.SetActive(false);
            _mainScreen_Collapsed.SetActive(true);
            UpdateToggleStates();
        }
        
        private void OnStepCompletedToggleCollapsedValueChanged(bool value)
        {
            _mainScreen.SetActive(true);
            _mainScreen_Collapsed.SetActive(false);
            UpdateToggleStates();
        }
        
        private void UpdateToggleStates()
        {
            switch (_mainScreen.activeSelf)
            {
                case true when !_mainScreen_Collapsed.activeSelf:
                    collapsePanelToggle.isOn = false;
                    collapsePanelToggle_Collapsed.isOn = true;
                    break;
                case false when _mainScreen_Collapsed.activeSelf:
                    collapsePanelToggle.isOn = false;
                    collapsePanelToggle_Collapsed.isOn = true;
                    break;
            }
        }
        
        private void OnPreviousStepClicked()
        {
            RootObject.Instance.LEE.StepManager.GoToPreviousStep();
        }

        private void OnNextStepClicked()
        {
            RootObject.Instance.LEE.StepManager.GoToNextStep();
        } 
        
        public async void UpdateView(ActivityStep step)
        {
            if (step == null)
            {
                return; 
            }
            _step = step;  
            _textTitle.text = step.Name;
            _textTitle_Collapsed.text = step.Name;
            _textDescription.text = step.Description;
            
            foreach (var item in _mediaListItemViews)
            {
                Destroy(item.gameObject);
            }
            _mediaListItemViews.Clear();
            
            if (_step?.Attachment != null)
            {
                foreach (var file in _step.Attachment)
                {
                    var activityId = RootObject.Instance.LEE.ActivityManager.ActivityId;
                    var texture = await RootObject.Instance.LEE.MediaManager.LoadMediaFileToTexture2D(activityId, file.Id);

                    if (texture != null)
                    {
                        var item = Instantiate(stepsMediaListItemViewPrefab, containerMedia);
                        item.Initialize(file, texture, _step.Id, (stepId, fileModel) =>
                        {
                            RootObject.Instance.LEE.StepManager.RemoveAttachment(stepId, fileModel.Id);
                        });
                        item.Interactable = false;
                        _mediaListItemViews.Add(item);
                    }
                }
            }
        }
    }
}
