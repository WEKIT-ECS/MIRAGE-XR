using System;
using LearningExperienceEngine.DataModel;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;

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

        public void Initialize(ActivityStep step)
        {
            _camera = RootObject.Instance.BaseCamera;
            name = $"Step_{step.Id}";
            _step = step;
            text.text = RootObject.Instance.LEE.StepManager.GetStepNumber(_step.Id).ToString("00");
            transform.SetLocalPositionAndRotation(_step.Location.Position, Quaternion.Euler(_step.Location.Rotation));
            transform.localScale = _step.Location.Scale;
            InitializeManipulator();
        }

        private void InitializeManipulator()
        {
            if (_objectManipulator is null)
            {
                _objectManipulator = gameObject.AddComponent<ObjectManipulator>();
                _objectManipulator.OnManipulationEnded.AddListener(OnManipulationEnded);
            }
        }

        private void OnManipulationEnded(ManipulationEventData eventData)
        {
            _step.Location.Position = eventData.ManipulationSource.transform.localPosition;
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