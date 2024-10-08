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

        public virtual UniTask InitializeAsync(Content content)
        {
            name = $"Content_{content.Type}_{content.Id}";
            transform.SetLocalPositionAndRotation(content.Location.Position, Quaternion.Euler(content.Location.Rotation));
            transform.localScale = content.Location.Scale;
            Content = content;

            InitializeManipulator();
            InitializeBoundsControl();

            return UniTask.CompletedTask;
        }

        protected virtual void InitializeManipulator()
        {
            ObjectManipulator = gameObject.AddComponent<ObjectManipulator>();
            ObjectManipulator.OnManipulationStarted.AddListener(OnManipulationStarted);
            ObjectManipulator.OnManipulationEnded.AddListener(OnManipulationEnded);
        }

        protected virtual void InitializeBoundsControl()
        {
            BoundsControl = gameObject.AddComponent<BoundsControl>();
            BoundsControl.RotateStarted.AddListener(OnRotateStarted);
            BoundsControl.RotateStopped.AddListener(OnRotateStopped);
            BoundsControl.ScaleStarted.AddListener(OnScaleStarted);
            BoundsControl.ScaleStopped.AddListener(OnScaleStopped);
        }

        protected virtual void OnRotateStarted()
        {
        }

        protected virtual void OnRotateStopped()
        {
            Content.Location.Rotation = transform.localEulerAngles;
            RootObject.Instance.ContentManager.UpdateContent(Content);
        }

        protected virtual void OnScaleStarted()
        {
        }

        protected virtual void OnScaleStopped()
        {
            Content.Location.Scale = transform.localScale;
            RootObject.Instance.ContentManager.UpdateContent(Content);
        }

        protected virtual void OnManipulationStarted(ManipulationEventData eventData)
        {
        }

        protected virtual void OnManipulationEnded(ManipulationEventData eventData)
        {
            Content.Location.Position = eventData.ManipulationSource.transform.localPosition;
            Content.Location.Rotation = eventData.ManipulationSource.transform.localEulerAngles;
            RootObject.Instance.ContentManager.UpdateContent(Content);
        }
    }
}