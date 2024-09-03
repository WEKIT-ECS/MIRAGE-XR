using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;
using LearningExperienceEngine.DataModel;

namespace MirageXR.NewDataModel
{
    public class UnityEventActivities : UnityEvent<List<Activity>> {} 
    public class UnityEventActivity : UnityEvent<Activity> {} 
    
    public interface IActivityManager
    {
        UniTask InitializeAsync(IContentManager contentManager, INetworkDataProvider networkDataProvider);
        UniTask<List<Activity>> FetchActivitiesAsync();
        UniTask<Activity> LoadActivityAsync(Guid activityId);
    }
}