using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using LearningExperienceEngine.DataModel;
using MemoryPack;
using UnityEngine;

namespace MirageXR.View
{
    [RequireComponent(typeof(NetworkActivityView))]
    public class NetworkActivitySynchronizer : NetworkBehaviour
    {
        [SerializeField, ReadOnly] private int dataSize;
        [SerializeField, ReadOnly] private string activityId;

        private static CollaborationManager CollaborationManager => RootObject.Instance.CollaborationManager;

        public NetworkActivityView ActivityView => _activityView;

        [Networked, OnChangedRender(nameof(OnActivityIdChanged))] private Guid ActivityId { get; set; }
        [Networked, OnChangedRender(nameof(OnIsEditModeChanged))] private bool IsEditMode { get; set; }

        // [Networked, OnChangedRender(nameof(OnDataSizeChanged))] private int DataSize { get; set; }
        // [Networked, Capacity(4096), OnChangedRender(nameof(OnDataChanged))] private NetworkArray<byte> Data { get; }

        /*private Activity Activity
        {
            get
            {
                if (DataSize <= 0)
                {
                    return null;
                }

                var bytes = new byte[DataSize];
                for (var i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = Data[i];
                }
                var activity = MemoryPackSerializer.Deserialize<Activity>(bytes);
                return activity;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                var bytes = MemoryPackSerializer.Serialize(value);
                if (bytes.Length > Data.Length)
                {
                    throw new IndexOutOfRangeException($"Step data is too big ({bytes.Length})");
                }

                DataSize = bytes.Length;
                for (var i = 0; i < bytes.Length; i++)
                {
                    Data.Set(i, bytes[i]);
                }
            }
        }*/

        //[Networked] private Guid ActivityId { get; set; }
        //[Networked] private Guid StepId { get; set; }

        //private Guid _lastActivityId;
        private NetworkActivityView _activityView;
        //private Activity _activity;
        //private readonly ReliableKey _keyOnContentUpdated = ReliableKey.FromInts(0, 0, 1, 0);
        //private readonly ReliableKey _keyOnActivityUpdated = ReliableKey.FromInts(0, 0, 1, 1);
        //private readonly ReliableKey _keyOnContentActivated = ReliableKey.FromInts(0, 0, 1, 2);
        //private readonly ReliableKey _keyOnStepChanged = ReliableKey.FromInts(0, 0, 1, 3);
        //private List<Guid> _guidList = new();

        public override void Spawned()
        {
            base.Spawned();

            _activityView = GetComponent<NetworkActivityView>(); 
            //CollaborationManager.OnReliableDataEvent.AddListener(OnReliableData);

            if (CollaborationManager.IsSharedModeMasterClient)
            {
                RootObject.Instance.LEE.ActivityManager.OnActivityLoaded += OnActivityLoaded;
                RootObject.Instance.LEE.ActivityManager.OnActivityUpdated += OnActivityUpdated;
                RootObject.Instance.LEE.ActivityManager.OnEditorModeChanged += OnEditorModeChanged;
                RootObject.Instance.LEE.StepManager.OnStepChanged += OnStepChanged;
                RootObject.Instance.LEE.ContentManager.OnContentUpdated += OnContentUpdated;
                RootObject.Instance.LEE.ContentManager.OnContentActivated += OnContentActivated;
            }

            OnActivityIdChanged();

            //_activityView.OnContentViewChanged.AddListener(OnContentChanged);
            //_activityView.OnStepViewChanged.AddListener(OnStepChanged);

            /*if (_isSharedModeMasterClient)
            {
                Debug.Log("[NetworkActivitySynchronizer] Shared Mode master client");
                RootObject.Instance.LEE.ActivityManager.OnActivityUpdated += OnOnActivityUpdated;
            }*/
        }

        /*private void OnDataChanged()
        {
            var activity = Activity;
            if (activity != null)
            {
                activityId = activity.Id.ToString();
                _activityView.UpdateActivity(activity);
            }
        }*/

        /*private void OnDataSizeChanged()
        {
            dataSize = DataSize;
        }*/

        private void OnActivityIdChanged()
        {
            activityId = ActivityId.ToString();
            _activityView.UpdateActivityId(ActivityId);
        }

        private void OnIsEditModeChanged()
        {
            activityId = ActivityId.ToString();
            _activityView.UpdateActivityId(ActivityId);
        }

        private void OnEditorModeChanged(bool value)
        {
            IsEditMode = value;
        }

        // private void OnReliableData(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        // {
        //     key.GetInts(out var k0, out var k1,out var k2,out var k3);
        //     Debug.Log($"----- player [{player.PlayerId}] my id [{CollaborationManager.LocalPlayer}]; k0 {k0}; k1 {k1}; k2 {k2}; k3 {k3}");
        //
        //     if (data == null || data.Count == 0)
        //     {
        //         return;
        //     }
        //
        //     if (k0 != 0 || k1 != 0 || k2 != 1)
        //     {
        //         return;
        //     }
        //
        //     switch (k3)
        //     {
        //         case 0:
        //             RpcContentUpdated(data.Array);
        //             break;
        //         case 1:
        //             RpcActivityUpdated(data.Array);
        //             break;
        //         case 2:
        //             RpcContentActivated(data.Array);
        //             break;
        //         case 3:
        //             RpcStepChanged(data.Array);
        //             break;
        //         default:
        //             Debug.LogWarning($"Unknown content type {k3}");
        //             break;
        //     }
        // }

        private void OnActivityLoaded(Activity activity)
        {
            ActivityId = activity.Id;
            //Activity = activity;
            _activityView.UpdateActivity(activity);
            //var bytes = MemoryPackSerializer.Serialize(activity);
            //RpcActivityUpdated(bytes);
        }

        private void OnActivityUpdated(Activity activity)
        {
            ActivityId = activity.Id;
            _activityView.UpdateActivity(activity);
            //Activity = activity;
            //var bytes = MemoryPackSerializer.Serialize(activity);
            //RpcActivityUpdated(bytes);
            //CollaborationManager.SendReliableDataToAllPlayers(_keyOnActivityUpdated, bytes);
        }

        private void OnContentUpdated(List<Content> contents)
        {
            _activityView.UpdateContent(contents);
            //var bytes = MemoryPackSerializer.Serialize(contents);
            //RpcContentUpdated(bytes);
            //CollaborationManager.SendReliableDataToAllPlayers(_keyOnContentUpdated, bytes);
        }

        private void OnContentActivated(List<Content> contents)
        {
            _activityView.ActivateContent(contents);
            //var bytes = MemoryPackSerializer.Serialize(contents);
            //RpcContentActivated(bytes);
            //CollaborationManager.SendReliableDataToAllPlayers(_keyOnContentActivated, bytes);
        }

        private void OnStepChanged(ActivityStep step)
        {
            _activityView.ChangeStep(step);
            //var bytes = MemoryPackSerializer.Serialize(step);
            //RpcStepChanged(bytes);
            //CollaborationManager.SendReliableDataToAllPlayers(_keyOnStepChanged, bytes);
        }

        /*private void OnOnActivityUpdated(Activity activity)
        {
            _activity = activity;
            ActivityId = activity.Id;
            _lastActivityId = activity.Id;
        }*/
/*
        public void FixedUpdate()
        {
            CheckActivityChanged();
        }

        private void CheckActivityChanged()
        {
            if (ActivityId != _lastActivityId)
            {
                _lastActivityId = ActivityId;
                HandleActivityIdChanged();
            }
        }

        private void HandleActivityIdChanged()
        {
            LoadActivityAsync(ActivityId).Forget();
        }
        private async UniTask LoadActivityAsync(Guid activityId)
        {
            _activity = await RootObject.Instance.LEE.NetworkDataProvider.GetActivityAsync(activityId);
            //_activityView.
        }
*/

        // private void RpcContentUpdated(byte[] data)
        // {
        //     var contents = MemoryPackSerializer.Deserialize<List<Content>>(data);
        //     Debug.Log($"----- [RpcContentUpdated] {contents}");
        //     _activityView.UpdateContent(contents);
        // }
        //
        // private void RpcActivityUpdated(byte[] data)
        // {
        //     var activity = MemoryPackSerializer.Deserialize<Activity>(data);
        //     Debug.Log($"----- [RpcActivityUpdated] {activity.Id}");
        //     _activityView.UpdateActivity(activity);
        // }
        //
        // private void RpcContentActivated(byte[] data)
        // {
        //     var contents = MemoryPackSerializer.Deserialize<List<Content>>(data);
        //     Debug.Log($"----- [RpcContentActivated] {contents}");
        //     _activityView.ActivateContent(contents);
        // }
        //
        // private void RpcStepChanged(byte[] data)
        // {
        //     var step = MemoryPackSerializer.Deserialize<ActivityStep>(data);
        //     Debug.Log($"----- [RpcStepChanged] {step.Id}");
        //     _activityView.ChangeStep(step);
        // }

        /*[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RpcContentUpdated(string json)
        {
            Debug.Log($"----- [RpcContentUpdated] {json}");
            var contents = JsonConvert.DeserializeObject<List<Content>>(json);
            _activityView.UpdateContent(contents);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RpcActivityUpdated(string json)
        {
            Debug.Log($"[RpcActivityUpdated] {json}");
            var activity = JsonConvert.DeserializeObject<Activity>(json);
            _activityView.UpdateActivity(activity);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RpcContentActivated(string json)
        {
            Debug.Log($"[RpcContentActivated] {json}");
            var contents = JsonConvert.DeserializeObject<List<Content>>(json);
            _activityView.ActivateContent(contents);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RpcStepChanged(string json)
        {
            Debug.Log($"[RpcStepChanged] {json}");
            var step = JsonConvert.DeserializeObject<ActivityStep>(json);
            _activityView.ChangeStep(step);
        }*/
    }
}