using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using LearningExperienceEngine;
using LearningExperienceEngine.DataModel;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using Activity = LearningExperienceEngine.DataModel.Activity;
using ContentType = LearningExperienceEngine.DataModel.ContentType;
using Location = LearningExperienceEngine.DataModel.Location;

namespace MirageXR.NewDataModel
{
    public class ActivityManager : IActivityManager
    {
        public List<Activity> Activities => _activities;
        public Activity Activity => _activity;

        public Guid ActivityId => _activity.Id;
        
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

        private List<Activity> _activities;
        private Activity _activity;
        private IContentManager _contentManager;
        private IAssetsManager _assetsManager;
        private INetworkDataProvider _networkDataProvider;
        private IStepManager _stepManager;
        private AuthManager _authManager;
        
        private string _activityName;
        private string _activityDescription;
        private string _activityLanguage;

        public UniTask InitializeAsync(IContentManager contentManager, INetworkDataProvider networkDataProvider, IAssetsManager assetsManager, IStepManager stepManager, AuthManager authManager)
        {
            _contentManager = contentManager;
            _networkDataProvider = networkDataProvider;
            _assetsManager = assetsManager;
            _stepManager = stepManager;
            _authManager = authManager;
            
            return UniTask.CompletedTask;
        }

        public async UniTask<List<Activity>> FetchActivitiesAsync()
        {
            await UniTask.WaitUntil(_authManager.LoggedIn);
            var response = await _networkDataProvider.GetActivitiesAsync();
            if (!response.IsSuccess)
            {
                AppLog.LogError(response.Error == null ? "Failed to fetch activities" : response.Error.Message);
                return null;
            }
            _activities = response.Data;
            if (_activities.Count == 0)
            {
                AppLog.LogDebug("No activities found");
                return _activities;
            }
            _onActivitiesFetched.Invoke(_activities);
            await LoadActivityAsync(_activities.First().Id);  //temp
            return _activities;
        }

        public async UniTask<Activity> LoadActivityAsync(Guid activityId)
        {
            var response = await _networkDataProvider.GetActivityAsync(activityId);
            if (!response.IsSuccess || response.Data.Id == Guid.Empty)
            {
                throw new Exception($"Activity with id {activityId} not found");
            }
            _contentManager.Reset();
            _stepManager.Reset();
            await _contentManager.LoadContentAsync(response.Data);
            _activity = response.Data;
            _stepManager.LoadSteps(_activity);
            _onActivityLoaded.Invoke(_activity);
            return _activity;
        }

        public Activity CreateNewActivity()
        {
            _contentManager.Reset();
            _stepManager.Reset();

            _activity = new Activity
            {
                Id = Guid.NewGuid(),
                Content = new List<Content>(),
                Contributors = null,
                Creator = _authManager.GetCurrentUser(),
                Name = $"Activity {DateTime.Now.ToShortDateString()}",
                Description = string.Empty,
                Hierarchy = new List<HierarchyItem>(),
                Steps = new List<ActivityStep>(),
                Thumbnail = _assetsManager.GetDefaultThumbnail(),
                Version = Application.version,
                CreationDate = DateTime.UtcNow,
                Language = "en-US",
            };

            _stepManager.AddStep(Location.Identity);
            var step = _stepManager.AddStep(Location.Identity);

            _contentManager.AddContent(new Content<ImageContentData>
            {
                Id = Guid.NewGuid(),
                Steps = new List<Guid> { _stepManager.CurrentStep.Id },
                Location = Location.Identity,
                Type = ContentType.Image,
                Version = Application.version,
                CreationDate = DateTime.UtcNow,
                IsVisible = true,
                ContentData = new ImageContentData
                {
                    Image = new FileModel(),
                    AvailableTriggers = null,
                    Text = "Temp text",
                }
            });
            _contentManager.AddContent(new Content<ImageContentData>
            {
                Id = Guid.NewGuid(),
                Steps = new List<Guid> { _stepManager.CurrentStep.Id },
                Location = Location.Identity,
                Type = ContentType.Image,
                Version = Application.version,
                CreationDate = DateTime.UtcNow,
                IsVisible = true,
                ContentData = new ImageContentData
                {
                    Image = new FileModel(),
                    AvailableTriggers = null,
                    Text = "Temp text",
                }
            });
            _contentManager.AddContent(new Content<ImageContentData>
            {
                Id = Guid.NewGuid(),
                Steps = new List<Guid> { step.Id },
                Location = Location.Identity,
                Type = ContentType.Image,
                Version = Application.version,
                CreationDate = DateTime.UtcNow,
                IsVisible = true,
                ContentData = new ImageContentData
                {
                    Image = new FileModel(),
                    AvailableTriggers = null,
                    Text = "Temp text",
                }
            });

            UpdateActivity();

            _onActivityLoaded.Invoke(_activity);
            _onActivityUpdated.Invoke(_activity);

            var json = JsonConvert.SerializeObject(_activity, Formatting.Indented); //temp
            _activity = JsonConvert.DeserializeObject<Activity>(json);
            Debug.Log(json);

            return _activity;
        }

        public void UpdateActivity()
        {
            if (_activity == null)
            {
                return;
            }

            _activity.Content = _contentManager.GetContents();
            _activity.Steps = _stepManager.GetSteps();
            _activity.Hierarchy = _stepManager.GetHierarchy();
        }

        public async UniTask<bool> UploadActivityAsync()
        {
            UpdateActivity();
            var response = await _networkDataProvider.UploadActivityAsync(_activity);
            return response.IsSuccess;
        }
    }
}