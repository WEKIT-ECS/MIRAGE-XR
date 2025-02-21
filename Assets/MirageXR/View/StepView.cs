using System;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;


#if FUSION2
using Fusion;
using Fusion.Sockets;
#endif

namespace MirageXR.View
{
    public class StepView : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private Transform diamond;
        [Space]
        [SerializeField] private InfoScreenSpatialView _infoPanelPrefab;
        
        public Guid Id => _step.Id;

        private ActivityStep _step;
        private Camera _camera;
        private InfoScreenSpatialView _infoScreenView;
        private NetworkObjectSynchronizer _networkObjectSynchronizer;

        public void Initialize(ActivityStep step)
        {
#if FUSION2
            RootObject.Instance.CollaborationManager.OnConnectedToServer.AddListener(OnConnectedToServer);
            RootObject.Instance.CollaborationManager.OnDisconnectedFromServer.AddListener(OnDisconnectedFromServer);
#endif
            _camera = RootObject.Instance.BaseCamera;
            InitializeManipulator();
            _infoScreenView = Instantiate(_infoPanelPrefab, transform, false);
            _infoScreenView.Initialize(step);
            UpdateView(step);
        }

#if FUSION2
        private void OnConnectedToServer(NetworkRunner runner)
        {
            OnConnectedToServerAsync(runner).Forget();
        }

        private async UniTask OnConnectedToServerAsync(NetworkRunner runner)
        {
            if (runner.LocalPlayer.PlayerId == 1)
            {
                var prefab = RootObject.Instance.AssetBundleManager.GetNetworkObjectPrefab();
                var networkObject = await runner.SpawnAsync(prefab);
                _networkObjectSynchronizer = networkObject.GetComponent<NetworkObjectSynchronizer>();
                _networkObjectSynchronizer.Initialization(this);
            }
        }

        private void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            if (_networkObjectSynchronizer != null && _networkObjectSynchronizer.NetworkObject != null)
            {
                runner.Despawn(_networkObjectSynchronizer.NetworkObject);
            }
        }
#endif

        private void OnDestroy()
        {
            if (RootObject.Instance is null)
            {
                return;
            }

#if FUSION2
            RootObject.Instance.CollaborationManager.OnConnectedToServer.RemoveListener(OnConnectedToServer);
            RootObject.Instance.CollaborationManager.OnDisconnectedFromServer.RemoveListener(OnDisconnectedFromServer);
            var networkRunner = RootObject.Instance.CollaborationManager.NetworkRunner;
            if (networkRunner.IsConnectedToServer)
            {
                if (networkRunner.LocalPlayer.PlayerId == 1)
                {
                    networkRunner.Despawn(_networkObjectSynchronizer.NetworkObject);
                }
            }
#endif
        }

        public  void UpdateView(ActivityStep step)
        {
            if (step == null)
            {
                return; 
            }
            name = $"Step_{step.Id}";
            _step = step;
            text.text = RootObject.Instance.LEE.StepManager.GetStepNumber(_step.Id).ToString("00");
            transform.SetLocalPositionAndRotation(_step.Location.Position, Quaternion.Euler(_step.Location.Rotation));
            transform.localScale = _step.Location.Scale;
            _infoScreenView.UpdateView(step);
        }

        private void InitializeManipulator()
        {
            var rigidBody = gameObject.AddComponent<Rigidbody>();
            rigidBody.isKinematic = true;
            rigidBody.useGravity = false;

            var generalGrabTransformer = gameObject.AddComponent<XRGeneralGrabTransformer>();
            generalGrabTransformer.allowTwoHandedScaling = true;

            var xrGrabInteractable = gameObject.AddComponent<XRGrabInteractable>();
            xrGrabInteractable.trackRotation = false;
            xrGrabInteractable.trackScale = false;
            xrGrabInteractable.selectEntered.AddListener(_ => OnManipulationStarted());
            xrGrabInteractable.selectExited.AddListener(_ => OnManipulationEnded());
        }

        private void OnManipulationStarted() { }

        private void OnManipulationEnded()
        {
            _step.Location.Position = transform.localPosition;
            RootObject.Instance.LEE.StepManager.UpdateStep(_step);
        }

        private void LateUpdate()
        {
            DoTextBillboarding();
        }

        private void DoTextBillboarding()
        {
            var newRotation = _camera.transform.eulerAngles;
            newRotation.x = 0;
            newRotation.z = 0;
            text.transform.eulerAngles = newRotation;
        }
    }
}