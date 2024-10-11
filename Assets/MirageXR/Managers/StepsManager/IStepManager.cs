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

        ActivityStep CurrentStep { get; }

        UniTask InitializeAsync(IContentManager contentManager, IActivityManager activityManager);
        void LoadSteps(Activity activity, Guid parentId = default);
        void AddHierarchyItem(HierarchyItem hierarchyItem, Guid parentId = default);
        ActivityStep AddStep(Location location, string name = EmptyString, string description = EmptyString, Guid hierarchyItemId = default);
        void GoToNextStep();
        void GoToPreviousStep();
        bool TryGoToStep(Guid stepId);
        void GoToStep(Guid stepId);
        List<ActivityStep> GetSteps();
        List<HierarchyItem> GetHierarchy();
        void Reset();
        void UpdateStep(ActivityStep step);
        int GetStepNumber(Guid stepId);
        void RemoveStep(Guid stepId);
    }
}