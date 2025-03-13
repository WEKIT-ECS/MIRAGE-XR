using System;
using Fusion;
using LearningExperienceEngine.DataModel;
using MemoryPack;
using UnityEngine;

namespace MirageXR.View
{
    [RequireComponent(typeof(StepView), typeof(NetworkObject))]
    public class NetworkStep : NetworkBehaviour
    {
        [SerializeField, ReadOnly] private int dataSize;
        [SerializeField, ReadOnly] private string stepId;

        [Networked] private Guid StepId { get; set; }
        [Networked, OnChangedRender(nameof(OnDataSizeChanged))] private int DataSize { get; set; }
        [Networked, Capacity(4096), OnChangedRender(nameof(OnDataChanged))] private NetworkArray<byte> Data { get; }

        private ActivityStep Step
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
                var step = MemoryPackSerializer.Deserialize<ActivityStep>(bytes);
                return step;
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
        }

        private StepView _stepView;
        private NetworkTransform _networkTransform;
        private NetworkObject _netObject;

        public override void Spawned()
        {
            base.Spawned();

            _stepView = GetComponent<StepView>();
            _networkTransform = GetComponent<NetworkTransform>();
            _netObject = GetComponent<NetworkObject>();
            OnDataChanged();
            OnDataSizeChanged();

            /*if (_netObject.HasStateAuthority)
            {
                _netObject.ReleaseStateAuthority();
            }*/
        }

        private void Start()
        {
            _stepView.Initialize(Step);
            _stepView.OnManipulationStartedEvent.AddListener(OnManipulationStarted);
            _stepView.OnManipulationEvent.AddListener(OnManipulation);
            _stepView.OnManipulationEndedEvent.AddListener(OnManipulationEnded);
        }

        private void OnManipulationStarted(Transform target)
        {
            /*if (!_netObject.HasStateAuthority)
            {
                _netObject.RequestStateAuthority();
            }*/
        }

        private void OnManipulation(Transform target)
        {
            if (!_netObject.HasStateAuthority)
            {
                Rpc_Manipulation(target.position, target.rotation, target.localScale);
            }
        }

        private void OnManipulationEnded(Transform target)
        {
            if (!_netObject.HasStateAuthority)
            {
                Rpc_ManipulationEnded(target.position, target.rotation, target.localScale);
            }
            else
            {
                UpdateStepLocation(target.position, target.rotation, target.localScale);
            }
        }

        public void Initialize(ActivityStep step)
        {
            if (Object.Runner.IsSharedModeMasterClient)
            {
                StepId = step.Id;
                Step = step;
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void Rpc_Manipulation(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            SetLocation(position, rotation, scale);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void Rpc_ManipulationEnded(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            SetLocation(position, rotation, scale);
            UpdateStepLocation(transform.localPosition, transform.localRotation, transform.localScale);
        }

        private void SetLocation(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            transform.GetComponent<NetworkTransform>().
            transform.position = position;
            transform.rotation = rotation;
            transform.localScale = scale;
        }

        private void UpdateStepLocation(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            _stepView.Step.Location.Position = position;
            _stepView.Step.Location.Rotation = rotation.eulerAngles;
            _stepView.Step.Location.Scale = scale;
            RootObject.Instance.LEE.StepManager.UpdateStep(_stepView.Step);
        }
        
        public void UpdateView(ActivityStep step)
        {
            if (Object.Runner.IsSharedModeMasterClient)
            {
                StepId = step.Id;
                Step = step;
            }
        }

        private void OnDataChanged()
        {
            var step = Step;
            if (step != null)
            {
                stepId = step.Id.ToString();
                _networkTransform.enabled = false;
                _stepView.UpdateView(step);
                _networkTransform.enabled = true;
            }
        }

        private void OnDataSizeChanged()
        {
            dataSize = DataSize;
        }

        /*private void OnReliableData(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {
            key.GetInts(out var k0, out var k1,out var k2,out var k3);
            Debug.Log($"----- player [{player.PlayerId}] my id [{CollaborationManager.LocalPlayer}]; k0 {k0}; k1 {k1}; k2 {k2}; k3 {k3}");
            if (k0 == 0 && k1 == 0 && k2 == 2)
            {
                switch (k3)
                {
                    case 0:
                        RpcContentUpdated(data.Array);
                        break;
                    case 1:
                        RpcActivityUpdated(data.Array);
                        break;
                    case 2:
                        RpcContentActivated(data.Array);
                        break;
                    case 3:
                        RpcStepChanged(data.Array);
                        break;
                    default:
                        break;
                }
            }
        }*/
    }
}