﻿using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using LearningExperienceEngine;
using LearningExperienceEngine.DataModel;
using MirageXR.View;
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
        public Guid ActivityId => _activity?.Id ?? Guid.Empty;

        public bool IsInitialized => _isInitialized;

        private bool _isInitialized;

        public UniTask WaitForInitialization()
        {
            return UniTask.WaitUntil(() => _isInitialized);
        }

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
        private ICalibrationManager _calibrationManager;

        private string _activityName;
        private string _activityDescription;
        private string _activityLanguage;
        private ActivityView _activityView;

        public async UniTask InitializeAsync(IContentManager contentManager, INetworkDataProvider networkDataProvider,
            IAssetsManager assetsManager, IStepManager stepManager, AuthManager authManager,
            ICalibrationManager calibrationManager)
        {
            _contentManager = contentManager;
            _networkDataProvider = networkDataProvider;
            _assetsManager = assetsManager;
            _stepManager = stepManager;
            _authManager = authManager;
            _calibrationManager = calibrationManager;

            await _calibrationManager.WaitForInitialization();
            CreateActivityView();
            _isInitialized = true;
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

        public Activity CreateNewActivity(Vector3 firstStepPosition)
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

            _stepManager.AddStep(new Location { Position = firstStepPosition, Rotation = Quaternion.identity.eulerAngles, Scale = Vector3.one });
            var step = _stepManager.AddStep(Location.Identity);

            _contentManager.AddContent(new Content<ImageContentData>
            {
                Id = Guid.NewGuid(),
                Steps = new List<Guid> { _stepManager.CurrentStep.Id },
                Location = new Location
                {
                    Position = Vector3.down,
                    Rotation = Vector3.zero,
                    Scale = new Vector3(1, 1, 0.05f),
                },
                Type = ContentType.Image,
                Version = Application.version,
                CreationDate = DateTime.UtcNow,
                IsVisible = true,
                ContentData = new ImageContentData
                {
                    Image = new FileModel(),
                    AvailableTriggers = null,
                    Text = null,
                }
            });
            _contentManager.AddContent(new Content<ImageContentData>
            {
                Id = Guid.NewGuid(),
                Steps = new List<Guid> { _stepManager.CurrentStep.Id },
                Location = new Location
                {
                    Position = Vector3.one,
                    Rotation = Vector3.zero,
                    Scale = new Vector3(1, 1, 0.05f),
                },
                Type = ContentType.Image,
                Version = Application.version,
                CreationDate = DateTime.UtcNow,
                IsVisible = true,
                ContentData = new ImageContentData
                {
                    IsBillboarded = true,
                    Image = new FileModel(),
                    AvailableTriggers = null,
                    Text = "Temp text 2",
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
                    IsBillboarded = true,
                    Image = new FileModel(),
                    AvailableTriggers = null,
                    Text = "Temp text 3",
                }
            });

            UpdateActivity();

            _onActivityLoaded.Invoke(_activity);
            _onActivityUpdated.Invoke(_activity);

            var json = JsonConvert.SerializeObject(_activity, Formatting.Indented); //temp
            _activity = JsonConvert.DeserializeObject<Activity>(json);                    //  
            Debug.Log(json);                                                              //

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

        private void CreateActivityView()
        {
            var activityView = new GameObject("ActivityView");
            activityView.transform.SetParent(_calibrationManager.Anchor, true);
            _activityView = activityView.AddComponent<ActivityView>();
        }
    }
}