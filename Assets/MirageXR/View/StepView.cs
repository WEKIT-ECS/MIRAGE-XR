using System;
using System.Collections.Generic;
using LearningExperienceEngine.DataModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

namespace MirageXR.View
{
    public class StepView : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private Transform diamond;
        [Space]
        [SerializeField] private GameObject _infoPanel;
        [SerializeField] private TMP_Text _stepNumber;
        [SerializeField] private TMP_Text _textDescription;
        [SerializeField] private StepsMediaListItemView stepsMediaListItemViewPrefab;
        [SerializeField] private Transform containerMedia;
        [SerializeField] private GameObject _stepCompletedToggle;
        [SerializeField] private GameObject _stepCompletedToggle_Collapsed;
        [SerializeField] private Button previousStepButton;
        [SerializeField] private Button nextStepButton;
        
        private readonly List<StepsMediaListItemView> _mediaListItemViews = new();

        public Guid Id => _step.Id;

        private ActivityStep _step;
        private Camera _camera;

        public void Initialize(ActivityStep step)
        {
            _camera = RootObject.Instance.BaseCamera;
            RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged += OnEditorModeChanged;
            InitializeManipulator();
            UpdateView(step);
            nextStepButton.onClick.AddListener(OnNextStepClicked);
            previousStepButton.onClick.AddListener(OnPreviousStepClicked);
        }

        private void OnPreviousStepClicked()
        {
            RootObject.Instance.LEE.StepManager.GoToPreviousStep();
        }

        private void OnNextStepClicked()
        {
            RootObject.Instance.LEE.StepManager.GoToNextStep();
        }

        private void OnEditorModeChanged(bool value)
        {
            _stepCompletedToggle.SetActive(!value);
            _stepCompletedToggle_Collapsed.SetActive(!value);
        }

        public async void UpdateView(ActivityStep step)
        {
            name = $"Step_{step.Id}";
            _step = step;
            text.text = RootObject.Instance.LEE.StepManager.GetStepNumber(_step.Id).ToString("00");
            transform.SetLocalPositionAndRotation(_step.Location.Position, Quaternion.Euler(_step.Location.Rotation));
            transform.localScale = _step.Location.Scale;

            _stepNumber.text = step.Name;
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

        private void InitializeManipulator()
        {
            var rigidBody = gameObject.AddComponent<Rigidbody>();
            rigidBody.isKinematic = true;
            rigidBody.useGravity = false;

            var generalGrabTransformer = gameObject.AddComponent<XRGeneralGrabTransformer>();
            generalGrabTransformer.allowTwoHandedScaling = true;

            var xrGrabInteractable = gameObject.AddComponent<XRGrabInteractable>();
            xrGrabInteractable.trackRotation = false;
            xrGrabInteractable.trackScale = false;
            xrGrabInteractable.selectEntered.AddListener(_ => OnManipulationStarted());
            xrGrabInteractable.selectExited.AddListener(_ => OnManipulationEnded());
        }

        private void OnManipulationStarted() { }

        private void OnManipulationEnded()
        {
            _step.Location.Position = transform.localPosition;
            RootObject.Instance.LEE.StepManager.UpdateStep(_step);
        }

        private void LateUpdate()
        {
            DoTextBillboarding();
        }

        private void DoTextBillboarding()
        {
            var newRotation = _camera.transform.eulerAngles;
            newRotation.x = 0;
            newRotation.z = 0;
            text.transform.eulerAngles = newRotation;
        }
    }
}