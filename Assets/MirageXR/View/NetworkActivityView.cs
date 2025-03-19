using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Fusion;
using LearningExperienceEngine.DataModel;
using UnityEngine;

namespace MirageXR.View
{
    public class NetworkActivityView : ActivityView
    {
        protected override async UniTask InitializeAsync()
        {
            /*if (RootObject.Instance.CollaborationManager.IsSharedModeMasterClient)
            {
                RootObject.Instance.LEE.ActivityManager.OnActivityLoaded += OnActivityLoaded;
                RootObject.Instance.LEE.StepManager.OnStepChanged += OnStepChanged;
                RootObject.Instance.LEE.ContentManager.OnContentUpdated += OnContentUpdated;
                RootObject.Instance.LEE.ContentManager.OnContentActivated += OnContentActivated;
                RootObject.Instance.LEE.ActivityManager.OnActivityUpdated += OnActivityUpdated;
                RootObject.Instance.LEE.ActivitySynchronizationManager.OnMessageReceived += OnSyncMessageReceived;
            }*/

            var calibrationManager = RootObject.Instance.CalibrationManager;
            await calibrationManager.WaitForInitialization();
            transform.SetParent(calibrationManager.Anchor, false);
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        protected override void RemoveUnusedContent()
        {
            var contentViews = GetComponentsInChildren<ContentView>();
            for (var i = contentViews.Length - 1; i >= 0; i--)
            {
                if (_contents.All(t => t.Id != contentViews[i].Id))
                {
                    RemoveContent(contentViews[i]);
                }
            }
        }

        protected override void UpdateStepView()
        {
            if (_step == null)
            {
                return;
            }
            
            if (_stepView == null)
            {
                _stepView = GetComponentInChildren<StepView>();
            }
            if (_stepView == null)
            {
                _stepView = CreateStepView(_step);
            }
            _stepView.GetComponent<NetworkStep>().UpdateView(_step);
        }

        /*protected override void UpdateContentsView()
        {
            if (RootObject.Instance.CollaborationManager.IsSharedModeMasterClient)
            {
                base.UpdateContentsView();
            }
        }*/

        public void UpdateActivityId(Guid activityId)
        {
            _activityId = activityId;
        }
 
        public void UpdateContent(List<Content> contents)
        {
            OnContentUpdated(contents);
        }

        public void UpdateActivity(Activity activity)
        {
            OnActivityUpdated(activity);
        }

        public void ActivateContent(List<Content> contents)
        {
            OnContentActivated(contents);
        }

        public void ChangeStep(ActivityStep step)
        {
            OnStepChanged(step);
        }

        protected override void OnContentUpdated(List<Content> contents)
        {
            if (_contents == null)
            {
                return;
            }

            var contentViews = GetComponentsInChildren<ContentView>();
            foreach (var content in contents)
            {
                var view = contentViews.FirstOrDefault(t => t.Id == content.Id);
                if (view != null)
                {
                    //view.UpdateContent(content);
                    view.GetComponent<NetworkContent>().UpdateView(content);
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

        protected override void AddContents()
        {
            var contentViews = GetComponentsInChildren<ContentView>();

            foreach (var content in _contents)
            {
                if (contentViews.Any(t => t.Id == content.Id))
                {
                    continue;
                }
                CreateContentView(content);
            }
        }

        /*protected override void UpdateContentsParent()
        {
            _stepView ??= GetComponentInChildren<StepView>();
            if (_stepView == null)
            {
                _stepView = CreateStepView(_step);
            }
            var contentViews = GetComponentsInChildren<ContentView>();
            foreach (var contentView in contentViews)
            {
                contentView.transform.SetParent(_stepView.transform, false);
            }
        }*/

        /*protected override void PlayContents()
        {
            RpcPlayContents();
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        protected void RpcPlayContents()
        {
            foreach (var contentView in _contentViews)
            {
                contentView.PlayAsync().Forget();
            }
        }
*/
        protected override void RemoveContent(ContentView contentView)
        {
            var netObj = contentView.GetComponent<NetworkObject>();
            //var networkContent = contentView.GetComponent<NetworkContent>();
            //networkContent.Despawn();
            RootObject.Instance.CollaborationManager.NetworkRunner.Despawn(netObj);
        }

        protected override ContentView CreateContentView(Content content)
        {
            if (RootObject.Instance.CollaborationManager.IsSharedModeMasterClient)
            {
                if (_stepView == null)
                {
                    _stepView = GetComponentInChildren<StepView>();
                }
                if (_stepView == null)
                {
                    _stepView = CreateStepView(_step);
                }

                var prefab = RootObject.Instance.AssetBundleManager.GetContentViewPrefab(content.Type, true);
                var networkObjectPrefab = prefab.GetComponent<NetworkObject>();
                var networkRunner = RootObject.Instance.CollaborationManager.NetworkRunner;
                var networkObject = networkRunner.Spawn(networkObjectPrefab, Vector3.zero, Quaternion.identity, networkRunner.LocalPlayer);
                networkObject.transform.SetParent(_stepView.transform);
                var contentView = networkObject.GetComponent<ContentView>();
                var networkContent = networkObject.GetComponent<NetworkContent>();
                networkContent.Initialize(content);
                return contentView;
            }

            return null;
        }

        protected override StepView CreateStepView(ActivityStep step)
        {
            if (RootObject.Instance.CollaborationManager.IsSharedModeMasterClient)
            {
                var prefab = RootObject.Instance.AssetBundleManager.GetStepViewPrefab(true);
                var networkObjectPrefab = prefab.GetComponent<NetworkObject>();
                var networkRunner = RootObject.Instance.CollaborationManager.NetworkRunner;
                var networkObject = networkRunner.Spawn(networkObjectPrefab, Vector3.zero, Quaternion.identity);
                networkObject.transform.SetParent(transform);
                var stepView = networkObject.GetComponent<StepView>();
                var networkStep = networkObject.GetComponent<NetworkStep>();
                networkStep.Initialize(step);
                return stepView;
            }

            return null;
        }
    }
}