using System;
using LearningExperienceEngine.DataModel;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MirageXR.View
{
    public class StepView : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private Transform diamond;

        public Guid Id => _step.Id;

        private ActivityStep _step;
        private Camera _camera;
        private ObjectManipulator _objectManipulator;
        public XRGrabInteractable _grabInteractible;

        public void Initialize(ActivityStep step)
        {
            _camera = RootObject.Instance.BaseCamera;
            name = $"Step_{step.Id}";
            _step = step;
            text.text = RootObject.Instance.LEE.StepManager.GetStepNumber(_step.Id).ToString("00");
            transform.SetLocalPositionAndRotation(_step.Location.Position, Quaternion.Euler(_step.Location.Rotation));
            transform.localScale = _step.Location.Scale;
            InitializeManipulator();
            InitializeXRGrabInteractible();
        }

        private void InitializeManipulator()
        {
            if (_objectManipulator is null)
            {
                _objectManipulator = gameObject.AddComponent<ObjectManipulator>();
                _objectManipulator.OnManipulationStarted.AddListener(_ => OnManipulationStarted());
                _objectManipulator.OnManipulationEnded.AddListener(_ => OnManipulationEnded());
            }
        }
        
        private void InitializeXRGrabInteractible()
        {
            if (_grabInteractible is null)
            {
                _grabInteractible = transform.GetChild(0).gameObject.GetComponent<XRGrabInteractable>();
                _grabInteractible.selectExited.AddListener(_ => OnDiamondMoved());
            }
        }

        private void OnManipulationStarted() { }

        private void OnManipulationEnded()
        {
            _step.Location.Position = transform.localPosition;
            RootObject.Instance.LEE.StepManager.UpdateStep(_step);
        }

        private void OnDiamondMoved()
        {
            Transform diamondPos = _grabInteractible.gameObject.transform;
            gameObject.transform.position = diamondPos.position;
            _grabInteractible.gameObject.transform.position -= diamondPos.localPosition;
            _step.Location.Position = _grabInteractible.gameObject.transform.localPosition;
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