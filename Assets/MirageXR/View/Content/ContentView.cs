using System;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using UnityEngine;

namespace MirageXR.View
{
    public class ContentView : MonoBehaviour
    {
        public Guid Id => Content.Id;

        protected Content Content;
        protected ObjectManipulator ObjectManipulator;
        protected BoundsControl BoundsControl;
        protected BoxCollider BoxCollider;

        public virtual UniTask InitializeAsync(Content content)
        {
            name = $"Content_{content.Type}_{content.Id}";
            transform.SetLocalPositionAndRotation(content.Location.Position, Quaternion.Euler(content.Location.Rotation));
            transform.localScale = content.Location.Scale;
            Content = content;

            InitializeBoxCollider();
            InitializeManipulator();
            InitializeBoundsControl();

            RootObject.Instance.LEE.StepManager.OnStepChanged += OnStepChanged;
            
            return UniTask.CompletedTask;
        }

        public virtual void Play() { }

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
            ObjectManipulator = gameObject.AddComponent<ObjectManipulator>();
            ObjectManipulator.OnManipulationStarted.AddListener(_ => OnManipulationStarted());
            ObjectManipulator.OnManipulationEnded.AddListener(_ => OnManipulationEnded());
        }

        protected virtual void InitializeBoundsControl()
        {
            BoundsControl = gameObject.AddComponent<BoundsControl>();
            BoundsControl.RotateStarted.AddListener(OnRotateStarted);
            BoundsControl.RotateStopped.AddListener(OnRotateStopped);
            BoundsControl.ScaleStarted.AddListener(OnScaleStarted);
            BoundsControl.ScaleStopped.AddListener(OnScaleStopped);
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