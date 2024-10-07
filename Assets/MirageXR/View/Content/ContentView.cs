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
        public Guid Id => _content.Id;

        private Content _content;

        public virtual UniTask InitializeAsync(Content content)
        {
            name = $"Content_{content.Type}_{content.Id}";
            transform.SetLocalPositionAndRotation(content.Location.Position, Quaternion.Euler(content.Location.Rotation));
            transform.localScale = content.Location.Scale;
            _content = content;

            InitializeManipulator();
            InitializeBoundsControl();

            return UniTask.CompletedTask;
        }

        protected virtual void InitializeManipulator()
        {
            var objectManipulator = gameObject.AddComponent<ObjectManipulator>();
            objectManipulator.OnManipulationStarted.AddListener(OnManipulationStarted);
            objectManipulator.OnManipulationEnded.AddListener(OnManipulationEnded);
        }

        protected virtual void InitializeBoundsControl()
        {
            var objectManipulator = gameObject.AddComponent<BoundsControl>();
        }

        protected virtual void OnManipulationStarted(ManipulationEventData eventData)
        {
        }

        protected virtual void OnManipulationEnded(ManipulationEventData eventData)
        {
            var temp1= eventData.ManipulationSource.transform.localEulerAngles;
            var temp2= eventData.ManipulationSource.transform.localRotation.eulerAngles;

            UnityEngine.Debug.Log($"{temp1} / {temp2}");
            
            _content.Location.Position = eventData.ManipulationSource.transform.localPosition;
            _content.Location.Rotation = eventData.ManipulationSource.transform.localEulerAngles;
            RootObject.Instance.ContentManager.UpdateContent(_content);
        }
    }
}