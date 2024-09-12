using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using UnityEngine.Events;

namespace MirageXR.NewDataModel
{
    public class UnityEventActivityStep : UnityEvent<ActivityStep> {} 

    public interface IStepManager
    {
        const string EmptyString = "";  

        event UnityAction<ActivityStep> OnStepChanged;

        UniTask InitializeAsync(IContentManager contentManager);
        void AddHierarchyItem(HierarchyItem hierarchyItem, Guid parentId = default);
        void AddStep(Location location, string name = EmptyString, string description = EmptyString, Guid hierarchyItemId = default);
        void GoToNextStep();
        bool TryGoToStep(Guid stepId);
        void GoToStep(Guid stepId);
        List<ActivityStep> GetSteps();
        List<HierarchyItem> GetHierarchy();
        void Reset();
    }
}