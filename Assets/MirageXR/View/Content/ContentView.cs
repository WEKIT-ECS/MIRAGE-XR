using System;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

namespace MirageXR.View
{
    public class ContentView : MonoBehaviour
    {
        public Guid Id => Content?.Id ?? Guid.Empty;
        public UnityEvent<Transform> OnManipulationStartedEvent => _onManipulationStarted;
        public UnityEvent<Transform> OnManipulationEvent => _onManipulationEvent;
        public UnityEvent<Transform> OnManipulationEndedEvent => _onManipulationEnded;

        protected Content Content;
        protected BoxCollider BoxCollider;
        protected bool Initialized;
        private bool _isSelected;

        private readonly UnityEvent<Transform> _onManipulationStarted = new();
        private readonly UnityEvent<Transform> _onManipulationEvent = new();
        private readonly UnityEvent<Transform> _onManipulationEnded = new();
        
        public virtual async UniTask InitializeAsync(Content content)
        {
            await UniTask.WaitUntil(() => RootObject.Instance.ViewManager.ActivityView.ActivityId != Guid.Empty);

            name = $"Content_{content.Type}_{content.Id}";
            transform.SetLocalPositionAndRotation(content.Location.Position, Quaternion.Euler(content.Location.Rotation));
            transform.localScale = content.Location.Scale;
            Content = content;

            await InitializeContentAsync(content);

            InitializeBoxCollider();
            InitializeManipulator();
        }

        public Content GetContent() => Content;

        public virtual async UniTask PlayAsync()
        {
            await UniTask.WaitUntil(() => Initialized);
        }

        public void UpdateContent(Content content)
        {
            OnContentUpdatedAsync(content).Forget();
        }

        protected virtual UniTask InitializeContentAsync(Content content)
        {
            Initialized = false;
            return UniTask.CompletedTask;
        }

        protected virtual void InitializeBoxCollider()
        {
            BoxCollider = gameObject.GetComponent<BoxCollider>();
            if (BoxCollider == null)
            {
                BoxCollider = gameObject.AddComponent<BoxCollider>();
                BoxCollider.size = Vector3.one;
            }
        }

        protected virtual void InitializeManipulator()
        {
            var rigidBody = gameObject.AddComponent<Rigidbody>();
            rigidBody.isKinematic = true;
            rigidBody.useGravity = false;

            var generalGrabTransformer = gameObject.AddComponent<XRGeneralGrabTransformer>();
            generalGrabTransformer.allowTwoHandedScaling = true;

            var xrGrabInteractable = gameObject.AddComponent<XRGrabInteractable>();
            xrGrabInteractable.movementType = XRBaseInteractable.MovementType.Instantaneous;
            xrGrabInteractable.useDynamicAttach = true;
            xrGrabInteractable.matchAttachPosition = true;
            xrGrabInteractable.matchAttachRotation = true;
            xrGrabInteractable.snapToColliderVolume = false;
            xrGrabInteractable.throwOnDetach = false;
            xrGrabInteractable.reinitializeDynamicAttachEverySingleGrab = false;
            xrGrabInteractable.selectMode = InteractableSelectMode.Multiple;
            xrGrabInteractable.selectEntered.AddListener(_ => OnManipulationStarted());
            xrGrabInteractable.selectExited.AddListener(_ => OnManipulationEnded());
        }

        /*protected virtual void OnStepChanged(ActivityStep step)
        {
        }*/

        /*protected virtual void OnRotateStarted()
        {
        }

        protected virtual void OnRotateStopped()
        {
            Content.Location.Rotation = transform.localEulerAngles;
            RootObject.Instance.LEE.ContentManager.UpdateContent(Content);
        }

        protected virtual void OnScaleStarted()
        {
        }

        protected virtual void OnScaleStopped()
        {
            Content.Location.Scale = transform.localScale;
            RootObject.Instance.LEE.ContentManager.UpdateContent(Content);
        }*/

        protected void Update()
        {
            if (_isSelected)
            {
                OnManipulation();
            }
        }

        protected virtual void OnManipulationStarted()
        {
            _isSelected = true;
            _onManipulationStarted.Invoke(transform);   
        }

        protected virtual void OnManipulation()
        {
            _onManipulationStarted.Invoke(transform);   
        }

        protected virtual void OnManipulationEnded()
        {
            _isSelected = false;
            Content.Location.Position = transform.localPosition;
            Content.Location.Rotation = transform.localEulerAngles;
            Content.Location.Scale = transform.localScale;
            _onManipulationEnded.Invoke(transform);
        }

        protected virtual UniTask OnContentUpdatedAsync(Content content)
        {
            transform.SetLocalPositionAndRotation(content.Location.Position, Quaternion.Euler(content.Location.Rotation));
            transform.localScale = content.Location.Scale;
            Content = content;

            return UniTask.CompletedTask;
        }
    }
}