using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine;
using UnityEngine.Events;
using LearningExperienceEngine.DTOs;
using Activity = LearningExperienceEngine.DataModel.Activity;

namespace MirageXR.NewDataModel
{
    public class UnityEventActivities : UnityEvent<List<ActivityResponse>> {} 
    public class UnityEventActivity : UnityEvent<Activity> {} 
    
    public interface IActivityManager
    {
        event UnityAction<List<ActivityResponse>> OnActivitiesFetched;
        event UnityAction<Activity> OnActivityLoaded;
        event UnityAction<Activity> OnActivityUpdated;

        UniTask InitializeAsync(IContentManager contentManager, INetworkDataProvider networkDataProvider, IAssetsManager assetsManager, IStepManager stepManager, AuthManager authManager);
        UniTask<List<ActivityResponse>> FetchActivitiesAsync();
        UniTask<Activity> LoadActivityAsync(Guid activityId);
        public Activity CreateNewActivity();
    }
}