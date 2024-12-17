using System;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

namespace MirageXR.View
{
    public class ContentView : MonoBehaviour
    {
        public Guid Id => Content.Id;

        protected Content Content;
        protected BoundsControl BoundsControl;
        protected BoxCollider BoxCollider;

        public virtual async UniTask InitializeAsync(Content content)
        {
            name = $"Content_{content.Type}_{content.Id}";
            transform.SetLocalPositionAndRotation(content.Location.Position, Quaternion.Euler(content.Location.Rotation));
            transform.localScale = content.Location.Scale;
            Content = content;

            await InitializeContentAsync(content);

            InitializeBoxCollider();
            InitializeManipulator();
            //InitializeBoundsControl();

            RootObject.Instance.LEE.StepManager.OnStepChanged += OnStepChanged;
        }

        public virtual void Play() { }

        protected virtual UniTask InitializeContentAsync(Content content)
        {
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
            xrGrabInteractable.reinitializeDynamicAttachEverySingleGrab = false;
            xrGrabInteractable.selectMode = InteractableSelectMode.Multiple;
            xrGrabInteractable.selectEntered.AddListener(_ => OnManipulationStarted());
            xrGrabInteractable.selectExited.AddListener(_ => OnManipulationEnded());
        }

        protected virtual void InitializeBoundsControl()
        {
            /*BoundsControl = gameObject.AddComponent<BoundsControl>();
            BoundsControl.RotateStarted.AddListener(OnRotateStarted);
            BoundsControl.RotateStopped.AddListener(OnRotateStopped);
            BoundsControl.ScaleStarted.AddListener(OnScaleStarted);
            BoundsControl.ScaleStopped.AddListener(OnScaleStopped);*/
        }

        protected virtual void OnStepChanged(ActivityStep step)
        {
        }

        protected virtual void OnRotateStarted()
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
        }

        protected virtual void OnManipulationStarted()
        {
        }

        protected virtual void OnManipulationEnded()
        {
            Content.Location.Position = transform.localPosition;
            Content.Location.Rotation = transform.localEulerAngles;
            RootObject.Instance.LEE.ContentManager.UpdateContent(Content);
        }
    }
}