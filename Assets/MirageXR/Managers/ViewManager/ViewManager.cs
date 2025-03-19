using System.Linq;
using Cysharp.Threading.Tasks;
using Fusion;
using LearningExperienceEngine.DataModel;
using LearningExperienceEngine.NewDataModel;
using MirageXR.View;
using UnityEngine;

namespace MirageXR
{
    public class ViewManager : IViewManager
    {
        public ActivityView ActivityView
        {
            get
            {
                if (_activityView == null)
                {
                    if (_collaborationManager.IsConnectedToServer)
                    {
                        var list = _collaborationManager.NetworkRunner.GetAllBehaviours<NetworkActivitySynchronizer>();
                        var netSync = list.FirstOrDefault();
                        if (netSync is not null)
                        {
                            _activityView = netSync.ActivityView;
                        }
                    }
                    else
                    {
                        _activityView = Object.FindObjectOfType<NetworkActivityView>();
                    }
                }
                return _activityView;
            }  
        }
        public GameObject UiView => _uiView;
        public bool IsInitialized => _isInitialized;

        private ActivityView _activityView;
        private GameObject _uiView;
        private IActivityManager _activityManager;
        private IAssetBundleManager _assetBundleManager;
        private CollaborationManager _collaborationManager;
        private bool _isInitialized;

        public UniTask WaitForInitialization()
        {
            return UniTask.WaitUntil(() => _isInitialized);
        }

        public void Initialize(IActivityManager activityManager, IAssetBundleManager assetBundleManager, CollaborationManager collaborationManager)
        {
            _activityManager = activityManager;
            _assetBundleManager = assetBundleManager;
            _collaborationManager = collaborationManager;
            
            _activityManager.OnActivityLoaded += OnActivityLoaded;
            collaborationManager.OnPlayerJoinEvent.AddListener(OnPlayerJoinEvent);

            CreateUiView();
        }

        private void OnPlayerJoinEvent(NetworkRunner runner, PlayerRef player)
        {
            CreateNetworkActivityView();
        }

        private void OnActivityLoaded(Activity activity)
        {
            if (_collaborationManager.IsConnectedToServer)
            {
                CreateNetworkActivityView();
            }
            else
            {
                CreateActivityView();
            }
        }

        private void CreateUiView()
        {
            var prefab = _assetBundleManager.GetUiView(UiType.Spatial);
            _uiView = Object.Instantiate(prefab);
        }

        private void CreateNetworkActivityView()
        {
            if (_activityView is not NetworkActivityView _)
            {
                if (_activityView != null)
                {
                    Object.Destroy(_activityView.gameObject);
                }

                var networkRunner = _collaborationManager.NetworkRunner;
                if (networkRunner != null && networkRunner.IsSharedModeMasterClient)
                {
                    var prefab = _assetBundleManager.GetActivityViewPrefab(true);
                    var networkObjectPrefab = prefab.GetComponent<NetworkObject>();
                    var networkObj = networkRunner.Spawn(networkObjectPrefab);
                    _activityView = networkObj.GetComponent<NetworkActivityView>();
                }
            }
        }

        private void CreateActivityView()
        {
            if (_activityView == null || _activityView is NetworkActivityView _)
            {
                if (_activityView != null)
                {
                    Object.Destroy(_activityView.gameObject);
                }

                var prefab = _assetBundleManager.GetActivityViewPrefab();
                _activityView = Object.Instantiate(prefab);
            }
        }
    }
}