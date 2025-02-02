using System;
using LearningExperienceEngine.DataModel;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

namespace MirageXR
{
    public class SpatialHyperlinkObjectView : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private Transform diamond;
        
        public Guid Id => _step.Id;

        private ActivityStep _step;
        private Camera _camera;

        public void Start()
        {
            _camera = RootObject.Instance.BaseCamera;
            InitializeManipulator();
            UpdateView();
        }

        public  void UpdateView()
        {
            text.text = "TEST TEXT";
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
