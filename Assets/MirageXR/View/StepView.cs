using System;
using LearningExperienceEngine.DataModel;
using UnityEngine;

namespace MirageXR.View
{
    public class StepView : MonoBehaviour
    {
        public Guid Id => _step.Id;

        private ActivityStep _step;

        public void Initialize(ActivityStep step)
        {
            name = $"Step_{step.Id}";
            _step = step;
            transform.SetLocalPositionAndRotation(_step.Location.Position, Quaternion.Euler(_step.Location.Rotation));
            transform.localScale = _step.Location.Scale;
        }
    }
}