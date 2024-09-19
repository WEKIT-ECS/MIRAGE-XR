using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine;
using LearningExperienceEngine.DTOs;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using Activity = LearningExperienceEngine.DataModel.Activity;
using Location = LearningExperienceEngine.DataModel.Location;

namespace MirageXR.NewDataModel
{
    public class ActivityManager : IActivityManager
    {
        public List<ActivityResponse> Activities => _activities;
        public Activity Activity => _activity;

        public event UnityAction<List<ActivityResponse>> OnActivitiesFetched
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

        public event UnityAction<Activity> OnActivityUpdated
        {
            add
            {
                _onActivityUpdated.AddListener(value);
                if (_activities != null)
                {
                    value(_activity);
                }
            }
            remove => _onActivityUpdated.RemoveListener(value);
        }

        private readonly UnityEventActivities _onActivitiesFetched = new();
        private readonly UnityEventActivity _onActivityLoaded = new();
        private readonly UnityEventActivity _onActivityUpdated = new();

        private List<ActivityResponse> _activities;
        private Activity _activity;
        private IContentManager _contentManager;
        private IAssetsManager _assetsManager;
        private INetworkDataProvider _networkDataProvider;
        private IStepManager _stepManager;
        private AuthManager _authManager;

        public async UniTask InitializeAsync(IContentManager contentManager, INetworkDataProvider networkDataProvider, IAssetsManager assetsManager, IStepManager stepManager, AuthManager authManager)
        {
            _contentManager = contentManager;
            _networkDataProvider = networkDataProvider;
            _assetsManager = assetsManager;
            _stepManager = stepManager;
            _authManager = authManager;
            CreateNewActivity();
            await FetchActivitiesAsync();
        }

        public async UniTask<List<ActivityResponse>> FetchActivitiesAsync()
        {
            await UniTask.WaitUntil(_authManager.LoggedIn);
            _activities = await _networkDataProvider.GetActivitiesAsync();
            _onActivitiesFetched.Invoke(_activities);
            await LoadActivityAsync(_activities.First().Id);
            return _activities;
        }

        public async UniTask<Activity> LoadActivityAsync(Guid activityId)
        {
            var activity = await _networkDataProvider.GetActivityAsync(activityId);
            if (activity.Id == Guid.Empty)
            {
                throw new Exception($"Activity with id {activityId} not found");
            }
            _contentManager.Reset();
            _stepManager.Reset();
            await _contentManager.LoadContentAsync(activity);
            _activity = activity;
            _onActivityLoaded.Invoke(_activity);
            return _activity;
        }

        public Activity CreateNewActivity()
        {
            _contentManager.Reset();
            _stepManager.Reset();
            _stepManager.AddStep(Location.Identity);

            _activity = new Activity
            {
                Id = Guid.NewGuid(),
                Content = _contentManager.GetContents(),
                Contributors = null,
                Creator = _authManager.GetCurrentUser(),
                Name = $"Activity {DateTime.Now.ToShortDateString()}",
                Description = string.Empty,
                Hierarchy = _stepManager.GetHierarchy(),
                Steps = _stepManager.GetSteps(),
                Thumbnail = _assetsManager.GetDefaultThumbnail(),
                Version = Application.version,
                CreationDate = DateTime.UtcNow,
                Language = "en-US",
            };

            _onActivityLoaded.Invoke(_activity);
            _onActivityUpdated.Invoke(_activity);

            var json = JsonConvert.SerializeObject(_activity, Formatting.Indented);
            _activity = JsonConvert.DeserializeObject<Activity>(json);
            Debug.Log(json);

            return _activity;
        }
    }
}