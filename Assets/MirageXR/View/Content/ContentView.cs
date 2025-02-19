using System;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;


#if FUSION2
using Fusion;
using Fusion.Sockets;
#endif

namespace MirageXR.View
{
    public class ContentView : MonoBehaviour
    {
        public Guid Id => Content.Id;

        protected Content Content;
        protected BoundsControl BoundsControl;
        protected BoxCollider BoxCollider;
        protected bool Initialized;
        private NetworkObjectSynchronizer _networkObjectSynchronizer;

        public virtual async UniTask InitializeAsync(Content content)
        {
            name = $"Content_{content.Type}_{content.Id}";
            transform.SetLocalPositionAndRotation(content.Location.Position, Quaternion.Euler(content.Location.Rotation));
            transform.localScale = content.Location.Scale;
            Content = content;

            await InitializeContentAsync(content);

            InitializeBoxCollider();
            InitializeManipulator();
            //InitializeBoundsControl();

            RootObject.Instance.LEE.StepManager.OnStepChanged += OnStepChanged;
#if FUSION2
            RootObject.Instance.CollaborationManager.OnConnectedToServer.AddListener(OnConnectedToServer);
            RootObject.Instance.CollaborationManager.OnDisconnectedFromServer.AddListener(OnDisconnectedFromServer);
            //RootObject.Instance.LEE.ActivitySynchronizationManager.OnMessageReceived += OnSyncMessageReceived;
#endif
        }

#if FUSION2
        private void OnConnectedToServer(NetworkRunner runner)
        {
            OnConnectedToServerAsync(runner).Forget();
        }

        private async UniTask OnConnectedToServerAsync(NetworkRunner runner)
        {
            if (runner.LocalPlayer.PlayerId == 1)   //TODO: change to session owner
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
            
            RootObject.Instance.LEE.StepManager.OnStepChanged -= OnStepChanged;

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

        /*private void OnSyncMessageReceived(SynchronizationDataModel data)
        {
            if (data.Type == MessageType.ActivityUpdated && data is SynchronizationDataModel<ActivityUpdatedDataModel> updatedData)
            {
                if (updatedData.Data.ContentId == Content.Id)
                {
                    UpdateContent(updatedData.Data.Content);
                }
            }
        }*/

        public Content GetContent() => Content;

        public virtual async UniTask PlayAsync()
        {
            await UniTask.WaitUntil(() => Initialized);
        }

        public void UpdateContent(Content content)
        {
            OnContentUpdatedAsync(content).Forget();
        }

        protected virtual UniTask InitializeContentAsync(Content content)
        {
            Initialized = false;
            return UniTask.CompletedTask;
        }
        
        protected virtual void InitializeBoxCollider()
        {
            BoxCollider = gameObject.GetComponent<BoxCollider>();
            if (BoxCollider == null)
            {
                BoxCollider = gameObject.AddComponent<BoxCollider>();
                BoxCollider.size = Vector3.one;
            }
        }

        protected virtual void InitializeManipulator()
        {
            var rigidBody = gameObject.AddComponent<Rigidbody>();
            rigidBody.isKinematic = true;
            rigidBody.useGravity = false;

            var generalGrabTransformer = gameObject.AddComponent<XRGeneralGrabTransformer>();
            generalGrabTransformer.allowTwoHandedScaling = true;

            var xrGrabInteractable = gameObject.AddComponent<XRGrabInteractable>();
            xrGrabInteractable.movementType = XRBaseInteractable.MovementType.Instantaneous;
            xrGrabInteractable.useDynamicAttach = true;
            xrGrabInteractable.matchAttachPosition = true;
            xrGrabInteractable.matchAttachRotation = true;
            xrGrabInteractable.snapToColliderVolume = false;
            xrGrabInteractable.reinitializeDynamicAttachEverySingleGrab = false;
            xrGrabInteractable.selectMode = InteractableSelectMode.Multiple;
            xrGrabInteractable.selectEntered.AddListener(_ => OnManipulationStarted());
            xrGrabInteractable.selectExited.AddListener(_ => OnManipulationEnded());
        }

        protected virtual void InitializeBoundsControl()
        {
            /*BoundsControl = gameObject.AddComponent<BoundsControl>();
            BoundsControl.RotateStarted.AddListener(OnRotateStarted);
            BoundsControl.RotateStopped.AddListener(OnRotateStopped);
            BoundsControl.ScaleStarted.AddListener(OnScaleStarted);
            BoundsControl.ScaleStopped.AddListener(OnScaleStopped);*/
        }

        protected virtual void OnStepChanged(ActivityStep step)
        {
        }

        protected virtual void OnRotateStarted()
        {
        }

        protected virtual void OnRotateStopped()
        {
            Content.Location.Rotation = transform.localEulerAngles;
            RootObject.Instance.LEE.ContentManager.UpdateContent(Content);
        }

        protected virtual void OnScaleStarted()
        {
        }

        protected virtual void OnScaleStopped()
        {
            Content.Location.Scale = transform.localScale;
            RootObject.Instance.LEE.ContentManager.UpdateContent(Content);
        }

        protected virtual void OnManipulationStarted()
        {
        }

        protected virtual void OnManipulationEnded()
        {
            Content.Location.Position = transform.localPosition;
            Content.Location.Rotation = transform.localEulerAngles;
            RootObject.Instance.LEE.ContentManager.UpdateContent(Content);
        }

        protected virtual UniTask OnContentUpdatedAsync(Content content)
        {
            transform.SetLocalPositionAndRotation(content.Location.Position, Quaternion.Euler(content.Location.Rotation));
            transform.localScale = content.Location.Scale;
            Content = content;
            
            return UniTask.CompletedTask;
        }
    }
}