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
        public static ActivityView Instance { get; private set; }

        private ActivityStep _step;
        private List<Content> _contents;

        private StepView _stepView;
        private readonly List<ContentView> _contentViews = new();

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
            }
            Instance = this;
        }

        private void Start()
        {
            InitializeAsync().Forget();
        }

        private async UniTask InitializeAsync()
        {
            RootObject.Instance.LEE.StepManager.OnStepChanged += StepManagerOnStepChanged;
            RootObject.Instance.LEE.ContentManager.OnContentUpdated += ContentManagerOnContentUpdated;
            RootObject.Instance.LEE.ContentManager.OnContentActivated += ContentManagerOnContentActivated;
            RootObject.Instance.LEE.ActivityManager.OnActivityUpdated += OnOnActivityUpdated;
            RootObject.Instance.LEE.ActivitySynchronizationManager.OnMessageReceived += OnSyncMessageReceived;

            var calibrationManager = RootObject.Instance.CalibrationManager;
            await calibrationManager.WaitForInitialization();
            transform.SetParent(calibrationManager.Anchor, false);
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        private void OnSyncMessageReceived(SynchronizationDataModel data)
        {
            if (data.MessageID == MessageType.ActivityUpdated && data is SynchronizationDataModel<ActivityUpdatedDataModel> updatedData)
            {
                if (updatedData.Data.Activity != null)
                {
                    ContentManagerOnContentUpdated(updatedData.Data.Activity.Content);
                }
            }
        }
        
        public StepView GetStep(Guid stepId)
        {
            return _stepView.Id == stepId ? _stepView : null;
        }

        public ContentView GetContent(Guid contentId)
        {
            return _contentViews.FirstOrDefault(t => t.Id == contentId);
        }

        private void ContentManagerOnContentUpdated(List<Content> contents)
        {
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

        private void OnOnActivityUpdated(Activity activity)
        {
            UnityEngine.Debug.Log("---OnActivityUpdated");
            UpdateStepView();
        }

        private void ContentManagerOnContentActivated(List<Content> contents)
        {
            UnityEngine.Debug.Log("---ContentManagerOnContentActivated");
            _contents = contents;
            UpdateContentsView();
        }

        private void StepManagerOnStepChanged(ActivityStep step)
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
            UpdateContentsParent();
            PlayContents();
        }

        private void UpdateContentsParent()
        {
            if (_stepView == null)
            {
                _stepView = CreateStepView(_step);
            }
            foreach (var contentView in _contentViews)
            {
                contentView.transform.SetParent(_stepView.transform, false);
            }
        }

        private void AddContents()
        {
            foreach (var content in _contents)
            {
                if (_contentViews.All(t => t.Id != content.Id))
                {
                    var contentView = CreateContentView(content);
                    contentView.InitializeAsync(content).Forget();
                    _contentViews.Add(contentView);
                }
            }
        }

        private void RemoveUnusedContent()
        {
            for (var i = _contentViews.Count - 1; i >= 0; i--)
            {
                if (_contents.All(t => t.Id != _contentViews[i].Id))
                {
                    Destroy(_contentViews[i].gameObject);
                    _contentViews.RemoveAt(i);
                }
            }
        }

        private void PlayContents()
        {
            foreach (var contentView in _contentViews)
            {
                contentView.PlayAsync().Forget();
            }
        }

        protected virtual ContentView CreateContentView(Content content)
        {
            var prefab = RootObject.Instance.AssetBundleManager.GetContentViewPrefab(content.Type);
            return Instantiate(prefab);
        }

        protected virtual StepView CreateStepView(ActivityStep step)
        {
            var prefab = RootObject.Instance.AssetBundleManager.GetStepViewPrefab();
            var stepView = Instantiate(prefab, transform, false);
            stepView.Initialize(step);
            return stepView;
        }
    }
}