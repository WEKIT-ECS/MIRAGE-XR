using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine;
using UnityEngine;
using UnityEngine.Events;
using Activity = LearningExperienceEngine.DataModel.Activity;

namespace MirageXR.NewDataModel
{
    public class UnityEventActivities : UnityEvent<List<Activity>> {} 
    public class UnityEventActivity : UnityEvent<Activity> {} 
    
    public interface IActivityManager : IManager
    {
        event UnityAction<List<Activity>> OnActivitiesFetched;
        event UnityAction<Activity> OnActivityLoaded;
        event UnityAction<Activity> OnActivityUpdated;

        Guid ActivityId { get; }

        UniTask InitializeAsync(IContentManager contentManager, INetworkDataProvider networkDataProvider,
            IAssetsManager assetsManager, IStepManager stepManager, AuthManager authManager,
            ICalibrationManager calibrationManager);
        UniTask<List<Activity>> FetchActivitiesAsync();
        Activity CreateNewActivity(Vector3 firstStepPosition);
        UniTask<Activity> LoadActivityAsync(Guid activityId);
        UniTask DeleteActivityAsync(Guid activityId);
        void UpdateActivity();
        UniTask<bool> UploadActivityAsync();
    }
}