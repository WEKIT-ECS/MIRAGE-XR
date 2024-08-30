using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;

namespace MirageXR.NewDataModel
{
    public class ActivityManager : IActivityManager
    {
        public List<Activity> Activities => _activities;
        public Activity Activity => _activity;

        public event UnityAction<List<Activity>> OnActivitiesFetched
        {
            add
            {
                _onActivitiesFetched.AddListener(value);
                if (_activities != null)
                {
                    value(_activities);
                }
            }
            remove => _onActivitiesFetched.RemoveListener(value);
        }

        public event UnityAction<Activity> OnActivityLoaded
        {
            add
            {
                _onActivityLoaded.AddListener(value);
                if (_activities != null)
                {
                    value(_activity);
                }
            }
            remove => _onActivityLoaded.RemoveListener(value);
        }

        private readonly UnityEventActivities _onActivitiesFetched = new();
        private readonly UnityEventActivity _onActivityLoaded = new();

        private List<Activity> _activities;
        private Activity _activity;
        private IContentManager _contentManager;
        private INetworkDataProvider _networkDataProvider;

        public UniTask InitializeAsync(IContentManager contentManager, INetworkDataProvider networkDataProvider)
        {
            _contentManager = contentManager;
            _networkDataProvider = networkDataProvider;
            return UniTask.CompletedTask;
        }

        public async UniTask<List<Activity>> FetchActivitiesAsync()
        {
            _activities = await _networkDataProvider.GetActivitiesAsync("token");
            _onActivitiesFetched.Invoke(_activities);
            return _activities;
        }

        public async UniTask<Activity> LoadActivityAsync(Guid activityId)
        {
            var activity = _activities?.FirstOrDefault(t => t.Id == activityId);
            if (activity == null)
            {
                throw new ArgumentException($"Activity with id {activityId} not found");
            }

            await _contentManager.LoadContentAsync(activity);
            _activity = activity;
            _onActivityLoaded.Invoke(_activity);
            return _activity;
        }
    }
}