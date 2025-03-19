using System;
using Cysharp.Threading.Tasks;
using Fusion;
using LearningExperienceEngine.DataModel;
using MemoryPack;
using UnityEngine;

namespace MirageXR.View
{
    [RequireComponent(typeof(ContentView), typeof(NetworkObject))]
    public class NetworkContent : NetworkBehaviour
    {
        [SerializeField, ReadOnly] private int dataSize;
        [SerializeField, ReadOnly] private string contentId;

        [Networked] private Guid ContentId { get; set; }
        [Networked, OnChangedRender(nameof(OnDataSizeChanged))] private int DataSize { get; set; }
        [Networked, Capacity(4096), OnChangedRender(nameof(OnDataChanged))] private NetworkArray<byte> Data { get; }

        private Content Content
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
                var content = MemoryPackSerializer.Deserialize<Content>(bytes);
                return content;
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

        private ContentView _contentView;
        private NetworkObject _netObject;

        public override void Spawned()
        {
            base.Spawned();

            _contentView = GetComponent<ContentView>();
            _netObject = GetComponent<NetworkObject>();
            OnDataChanged();
            OnDataSizeChanged();

            /*if (_netObject.HasStateAuthority)
            {
                _netObject.ReleaseStateAuthority();
            }*/
        }

        public void Initialize(Content content)
        {
            if (Object.Runner.IsSharedModeMasterClient)
            {
                ContentId = content.Id;
                Content = content;
            }
        }

        private void Start()
        {
            _contentView.InitializeAsync(Content).Forget();
            _contentView.OnManipulationStartedEvent.AddListener(OnManipulationStarted);
            _contentView.OnManipulationStartedEvent.AddListener(OnManipulation);
            _contentView.OnManipulationEndedEvent.AddListener(OnManipulationEnded);
        }

        private void OnManipulationStarted(Transform target)
        {
            // if (!_netObject.HasStateAuthority)
            // {
            //     _netObject.RequestStateAuthority();
            // }
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
                UpdateContentLocation(target.position, target.rotation, target.localScale);
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
            UpdateContentLocation(transform.localPosition, transform.localRotation, transform.localScale);
        }

        private void SetLocation(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            transform.position = position;
            transform.rotation = rotation;
            transform.localScale = scale;
        }

        private void UpdateContentLocation(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            var content = _contentView.GetContent();
            content.Location.Position = position;
            content.Location.Rotation = rotation.eulerAngles;
            content.Location.Scale = scale;
            RootObject.Instance.LEE.ContentManager.UpdateContent(content);
        }

        public void UpdateView(Content content)
        {
            if (Object.Runner.IsSharedModeMasterClient)
            {
                ContentId = content.Id;
                Content = content;
            }
        }

        private void OnDataChanged()
        {
            var content = Content;
            if (content != null)
            {
                contentId = content.Id.ToString();
                _contentView.UpdateContent(content);
            }
        }

        private void OnDataSizeChanged()
        {
            dataSize = DataSize;
        }

        public void Despawn()
        {
            throw new NotImplementedException();
        }
    }
}