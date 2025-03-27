using System;
using LearningExperienceEngine.DataModel;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

namespace MirageXR.View
{
    public class StepView : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private Transform diamond;
        [Space]
        [SerializeField] private InfoScreenSpatialView _infoPanelPrefab;

        public Guid Id => _step?.Id ?? Guid.Empty;
        public ActivityStep Step => _step;
        public UnityEvent<Transform> OnManipulationStartedEvent => _onManipulationStartedEvent;
        public UnityEvent<Transform> OnManipulationEvent => _onManipulationEvent; 
        public UnityEvent<Transform> OnManipulationEndedEvent => _onManipulationEndedEvent; 
        
        public bool Interactable
        {
            get => GetInteractable();
            set => SetInteractable(value);
        }

        private bool _isInteractable;
        private bool _isInitialized;
        private bool _isSelected;
        private ActivityStep _step;
        private Camera _camera;
        private InfoScreenSpatialView _infoScreenView;
        private NetworkObjectSynchronizer _networkObjectSynchronizer;
        private readonly UnityEvent<Transform> _onManipulationStartedEvent = new();
        private readonly UnityEvent<Transform> _onManipulationEvent = new();
        private readonly UnityEvent<Transform> _onManipulationEndedEvent = new();

        public void Initialize(ActivityStep step)
        {
            _camera = RootObject.Instance.BaseCamera;
            InitializeManipulator();
            _infoScreenView = Instantiate(_infoPanelPrefab, transform, false);
            _infoScreenView.Initialize(step);
            _isInitialized = true;
            UpdateView(step);
            
            RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged += OnEditorModeChanged;
        }

        public void UpdateView(ActivityStep step)
        {
            if (!_isInitialized || step is null)
            {
                return;
            }

            name = $"Step_{step.Id}";
            _step = step;
            text.text = RootObject.Instance.LEE.StepManager.GetStepNumber(_step.Id).ToString("00");
            transform.SetLocalPositionAndRotation(_step.Location.Position, Quaternion.Euler(_step.Location.Rotation));
            transform.localScale = _step.Location.Scale;
            _infoScreenView.UpdateView(step);
        }

        private void OnEditorModeChanged(bool value)
        {
            Interactable = value;
        }

        private void SetInteractable(bool value)
        {
            _isInteractable = value;
            var generalGrabTransformer = gameObject.GetComponent<XRGeneralGrabTransformer>();
            if (generalGrabTransformer != null)
            {
                generalGrabTransformer.enabled = value;
            }

            var xrGrabInteractable = gameObject.GetComponent<XRGrabInteractable>();
            if (xrGrabInteractable)
            {
                xrGrabInteractable.enabled = value;
            }
        }

        private bool GetInteractable()
        {
            return _isInteractable;
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
            xrGrabInteractable.throwOnDetach = false;
            xrGrabInteractable.selectEntered.AddListener(_ => OnManipulationStarted());
            xrGrabInteractable.selectExited.AddListener(_ => OnManipulationEnded());

            SetInteractable(RootObject.Instance.LEE.ActivityManager.IsEditorMode);
        }

        private void OnManipulationStarted()
        {
            _isSelected = true;
            _onManipulationStartedEvent.Invoke(transform);
        }

        private void OnManipulation()
        {
            _onManipulationEvent.Invoke(transform);
        }

        private void OnManipulationEnded()
        {
            _isSelected = false;
            _step.Location.Position = transform.localPosition;
            _onManipulationEndedEvent.Invoke(transform);
        }

        private void Update()
        {
            if (_isSelected)
            {
                OnManipulation();
            }
        }

        private void LateUpdate()
        {
            DoTextBillboarding();
        }

        private void DoTextBillboarding()
        {
            if (_camera is null)
            {
                return;
            }
            var newRotation = _camera.transform.eulerAngles;
            newRotation.x = 0;
            newRotation.z = 0;
            text.transform.eulerAngles = newRotation;
        }
    }
}