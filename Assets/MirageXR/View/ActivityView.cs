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

            var calibrationManager = RootObject.Instance.CalibrationManager;
            await calibrationManager.WaitForInitialization();
            transform.SetParent(calibrationManager.Anchor, false);
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
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

        private void PlayContents()
        {
            foreach (var contentView in _contentViews)
            {
                contentView.Play();
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