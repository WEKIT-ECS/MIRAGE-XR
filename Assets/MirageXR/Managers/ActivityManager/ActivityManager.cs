using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using Newtonsoft.Json;
using UnityEngine;
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
            CreateActivity();
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

        public Activity CreateActivity()
        {
            var user = LearningExperienceEngine.LearningExperienceEngine.Instance.authManager.GetUser();

            var content = new Content<ImageContentData>
            {
                Id = Guid.NewGuid(),
                Location = new Location(),
                Name = "Image example",
                Type = ContentType.Image,
                IsVisible = true,
                ContentData = new ImageContentData
                {
                    Image = new File
                    {
                        Id = Guid.NewGuid(),
                        CreationDate = DateTime.UtcNow,
                        Version = "2.7.160",// Application.version,
                        Name = "Image example",
                        FileHash = HashCode.Combine(Guid.NewGuid(), Guid.NewGuid()).ToString()  //temp
                    } 
                },
                CreationDate = DateTime.UtcNow,
                Version = "2.7.160",// Application.version,
            };

            var step = new ActivityStep
            {
                Id = Guid.NewGuid(),
                Location = new Location
                {
                    Position = Vector3.zero,
                    Rotation = Quaternion.identity.eulerAngles,
                    Scale = Vector3.one,
                    TargetMarker = null
                },
                Contents = new List<Guid>
                {
                    content.Id
                },
                CreationDate = DateTime.UtcNow,
                Version = "2.7.160",// Application.version,
                Name = "Step 1",
                Description = "Example step",
                Attachment = null,
                Comments = null,
                Triggers = null,
                PrivateNotes = null,
                RequiredToolsPartsMaterials = null,
            };

            _activity = new Activity
            {
                Id = Guid.NewGuid(),
                Content = new List<Content>
                {
                    content.ToBaseContent()
                },
                Contributors = null,
                Creator = user,
                Name = $"Activity {DateTime.Now.ToShortDateString()}",
                Description = string.Empty,
                Hierarchy = new StepHierarchy
                {
                    Item = new List<HierarchyItem>
                    {
                        new()
                        {
                            Item = null,
                            StepIds = new List<Guid> { step.Id },
                            Description = string.Empty,
                            Title = "Title",
                        }
                    }
                },
                Steps = new List<ActivityStep>
                {
                    step
                },
                Thumbnail = new File
                {
                    Id = Guid.NewGuid(),
                    Name = "Thumbnail",
                    FileHash = "Default Thumbnail Hash", //temp
                    Version = "2.7.160",// Application.version,
                    CreationDate = DateTime.UtcNow
                },
                Version = "2.7.160",// Application.version,
                CreationDate = DateTime.UtcNow,
                Language = "en-US",
            };
            _onActivityLoaded.Invoke(_activity);

            var json = JsonConvert.SerializeObject(_activity, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            Debug.Log(json);
            
            return _activity;
        }
    }
}