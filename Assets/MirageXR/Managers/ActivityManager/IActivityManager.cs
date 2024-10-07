using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine;
using UnityEngine.Events;
using Activity = LearningExperienceEngine.DataModel.Activity;

namespace MirageXR.NewDataModel
{
    public class UnityEventActivities : UnityEvent<List<Activity>> {} 
    public class UnityEventActivity : UnityEvent<Activity> {} 
    
    public interface IActivityManager
    {
        event UnityAction<List<Activity>> OnActivitiesFetched;
        event UnityAction<Activity> OnActivityLoaded;
        event UnityAction<Activity> OnActivityUpdated;

        Guid ActivityId { get; }

        UniTask InitializeAsync(IContentManager contentManager, INetworkDataProvider networkDataProvider, IAssetsManager assetsManager, IStepManager stepManager, AuthManager authManager);
        UniTask<List<Activity>> FetchActivitiesAsync();
        Activity CreateNewActivity();
        UniTask<Activity> LoadActivityAsync(Guid activityId);
        void UpdateActivity();
        UniTask<bool> UploadActivityAsync();
    }
}