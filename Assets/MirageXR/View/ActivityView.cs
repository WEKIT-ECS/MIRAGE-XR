using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using LearningExperienceEngine.NewDataModel;
using UnityEngine;

namespace MirageXR.View
{
    public class ActivityView : MonoBehaviour
    {
        //public static ActivityView Instance { get; private set; }

        public Guid ActivityId => _activityId;
        public ActivityStep Step => _step;
        public List<Content> Contents => _contents;

        protected Guid _activityId;
        protected ActivityStep _step;
        protected List<Content> _contents;

        protected StepView _stepView;
        protected readonly List<ContentView> _contentViews = new();
        protected Activity _activity;

        /*private void Awake()
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
            }
            Instance = this;
        }*/

        private void Start()
        {
            InitializeAsync().Forget();
        }

        protected virtual async UniTask InitializeAsync()
        {
            RootObject.Instance.LEE.ActivityManager.OnActivityLoaded += OnActivityLoaded;
            RootObject.Instance.LEE.ActivityManager.OnActivityUpdated += OnActivityUpdated;
            RootObject.Instance.LEE.StepManager.OnStepChanged += OnStepChanged;
            RootObject.Instance.LEE.ContentManager.OnContentUpdated += OnContentUpdated;
            RootObject.Instance.LEE.ContentManager.OnContentActivated += OnContentActivated;
            RootObject.Instance.LEE.ActivitySynchronizationManager.OnMessageReceived += OnSyncMessageReceived;

            var calibrationManager = RootObject.Instance.CalibrationManager;
            await calibrationManager.WaitForInitialization();
            transform.SetParent(calibrationManager.Anchor, false);
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

#if UNITY_EDITOR
            var tempObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tempObject.transform.SetParent(transform);
            tempObject.transform.SetLocalPose(Pose.identity);
            var tempCollider = tempObject.GetComponent<Collider>();
            if (tempCollider)
            {
                Destroy(tempCollider);
            }
            tempObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
#endif
        }

        protected virtual void OnSyncMessageReceived(SynchronizationDataModel data)
        {
            /*if (data.MessageID == MessageType.ActivityUpdated && data is SynchronizationDataModel<ActivityUpdatedDataModel> updatedData)
            {
                if (updatedData.Data.Activity != null)
                {
                    ContentManagerOnContentUpdated(updatedData.Data.Activity.Content);
                }
            }*/
        }

        protected virtual void OnContentUpdated(List<Content> contents)
        {
            if (_contents == null)
            {
                return;
            }
            
            foreach (var content in contents)
            {
                var view = _contentViews.FirstOrDefault(t => t.Id == content.Id);
                if (view != null)
                {
                    view.UpdateContent(content);
                    view.PlayAsync().Forget();
                }
            }

            for (var i = 0; i < _contents.Count; i++)
            {
                var content = contents.FirstOrDefault(t => t.Id == _contents[i].Id);
                if (content != null)
                {
                    _contents[i] = content;
                }
            }
        }

        protected virtual void OnActivityLoaded(Activity activity)
        {
            _activity = activity;
            _activityId = activity.Id;
            UnityEngine.Debug.Log("---OnActivityLoaded");
            UpdateLocation(activity);
            UpdateStepView();
        }

        protected virtual void OnActivityUpdated(Activity activity)
        {
            _activity = activity;
            _activityId = activity.Id;
            UnityEngine.Debug.Log("---OnActivityUpdated");
            UpdateLocation(activity);
            UpdateStepView();
        }

        protected virtual void OnContentActivated(List<Content> contents)
        {
            UnityEngine.Debug.Log("---ContentManagerOnContentActivated");
            _contents = contents;
            UpdateContentsView();
        }

        protected virtual void UpdateLocation(Activity activity)
        {
            if (activity.Location != null)
            {
                transform.localPosition = activity.Location.Position;
                transform.localEulerAngles = activity.Location.Rotation;
                transform.localScale = activity.Location.Scale;   
            }
        }

        protected virtual void OnStepChanged(ActivityStep step)
        {
            UnityEngine.Debug.Log("---StepManagerOnStepChanged");
            _step = step;
            UpdateStepView();
        }

        protected virtual void UpdateStepView()
        {
            if (_stepView == null)
            {
                _stepView = CreateStepView(_step);
            }
            _stepView.UpdateView(_step);
        }

        protected virtual void UpdateContentsView()
        {
            RemoveUnusedContent();
            AddContents();
            //UpdateContentsParent();
            PlayContents();
        }

        protected virtual void RemoveUnusedContent()
        {
            for (var i = _contentViews.Count - 1; i >= 0; i--)
            {
                if (_contents.All(t => t.Id != _contentViews[i].Id))
                {
                    RemoveContent(_contentViews[i]);
                    _contentViews.RemoveAt(i);
                }
            }
        }

        protected virtual void AddContents()
        {
            foreach (var content in _contents)
            {
                if (_contentViews.All(t => t.Id != content.Id))
                {
                    var contentView = CreateContentView(content);
                    _contentViews.Add(contentView);
                }
            }
        }

        /*protected virtual void UpdateContentsParent()
        {
            if (_stepView == null)
            {
                _stepView = CreateStepView(_step);
            }
            foreach (var contentView in _contentViews)
            {
                contentView.transform.SetParent(_stepView.transform, false);
            }
        }*/

        protected virtual void PlayContents()
        {
            foreach (var contentView in _contentViews)
            {
                contentView.PlayAsync().Forget();
            }
        }

        protected virtual void RemoveContent(ContentView contentView)
        {
            Destroy(contentView.gameObject);
        }

        protected virtual ContentView CreateContentView(Content content)
        {
            if (_stepView == null)
            {
                _stepView = CreateStepView(_step);
            }

            var prefab = RootObject.Instance.AssetBundleManager.GetContentViewPrefab(content.Type);
            var contentView = Instantiate(prefab, _stepView.transform, false);
            contentView.gameObject.AddComponent<ContentManipulationSynchronizer>();
            contentView.InitializeAsync(content).Forget();
            return contentView;
        }

        protected virtual StepView CreateStepView(ActivityStep step)
        {
            var prefab = RootObject.Instance.AssetBundleManager.GetStepViewPrefab();
            var stepView = Instantiate(prefab, transform, false);
            stepView.gameObject.AddComponent<StepViewManipulationSynchronizer>();
            stepView.Initialize(step);
            return stepView;
        }

        public void ResetPosition()
        {
            UpdateLocation(_activity);
        }
    }
}