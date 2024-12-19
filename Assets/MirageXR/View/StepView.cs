using System;
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
        [SerializeField] private GameObject _infoPanel;
        [SerializeField] private TMP_Text _stepNumber;
        [SerializeField] private TMP_Text _textDescription;
        [SerializeField] private Image _image;

        public Guid Id => _step.Id;

        private ActivityStep _step;
        private Camera _camera;

        public void Initialize(ActivityStep step)
        {
            _camera = RootObject.Instance.BaseCamera;
            InitializeManipulator();
            UpdateView(step);
        }

        public void UpdateView(ActivityStep step)
        {
            name = $"Step_{step.Id}";
            _step = step;
            text.text = RootObject.Instance.LEE.StepManager.GetStepNumber(_step.Id).ToString("00");
            transform.SetLocalPositionAndRotation(_step.Location.Position, Quaternion.Euler(_step.Location.Rotation));
            transform.localScale = _step.Location.Scale;

            _stepNumber.text = name;
            _textDescription.text = step.Description;
            //_image.sprite = 
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