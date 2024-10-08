using System.Collections.Generic;
using System.Linq;
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
            RootObject.Instance.StepManager.OnStepChanged += StepManagerOnStepChanged;
            RootObject.Instance.ContentManager.OnContentActivated += ContentManagerOnContentActivated;
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

        private void UpdateStepView()
        {
            if (_stepView == null)
            {
                _stepView = CreateStepView(_step);
            }
            _stepView.Initialize(_step);
        }

        private void UpdateContentsView()
        {
            RemoveUnusedContent();
            AddContents();
            UpdateContentsParent();
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
                    contentView.InitializeAsync(content);
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

        private ContentView CreateContentView(Content content)
        {
            var prefab = RootObject.Instance.AssetsManager.GetContentViewPrefab(content.Type);
            return Instantiate(prefab);
        }

        private StepView CreateStepView(ActivityStep step)
        {
            var prefab = RootObject.Instance.AssetsManager.GetStepViewPrefab();
            var stepView = Instantiate(prefab, transform, false);
            stepView.Initialize(step);
            return stepView;
        }
    }
}