using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using UnityEngine;

namespace MirageXR.View
{
    public class ActivityView : MonoBehaviour
    {
        private ActivityStep _step;
        private List<Content> _contents;

        private StepView _stepView;
        private readonly List<ContentView> _contentViews = new();

        private void Start()
        {
            InitializeAsync().Forget();
        }

        private async UniTask InitializeAsync()
        {
            RootObject.Instance.LEE.StepManager.OnStepChanged += StepManagerOnStepChanged;
            RootObject.Instance.LEE.ContentManager.OnContentActivated += ContentManagerOnContentActivated;
            RootObject.Instance.LEE.ContentManager.OnContentUpdated += ContentManagerOnContentUpdated;

            var calibrationManager = RootObject.Instance.CalibrationManager;
            await calibrationManager.WaitForInitialization();
            transform.SetParent(calibrationManager.Anchor, false);
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        private void ContentManagerOnContentActivated(List<Content> contents)
        {
            _contents = contents;
            UpdateContentsView();
        }

        private void StepManagerOnStepChanged(ActivityStep step)
        {
            _step = step;
            UpdateStepView();
        }

        private void ContentManagerOnContentUpdated(List<Content> contents)
        {
            foreach (var content in contents)
            {
                var view = _contentViews.FirstOrDefault(t => t.Id == content.Id);
                if (view != null)
                {
                    view.UpdateContent(content);
                    view.PlayAsync();
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

        private void UpdateStepView()
        {
            if (_stepView == null)
            {
                _stepView = CreateStepView(_step);
            }
            _stepView.UpdateView(_step);
        }

        private void UpdateContentsView()
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

        private ContentView CreateContentView(Content content)
        {
            var prefab = RootObject.Instance.AssetBundleManager.GetContentViewPrefab(content.Type);
            return Instantiate(prefab);
        }

        private StepView CreateStepView(ActivityStep step)
        {
            var prefab = RootObject.Instance.AssetBundleManager.GetStepViewPrefab();
            var stepView = Instantiate(prefab, transform, false);
            stepView.Initialize(step);
            return stepView;
        }
    }
}