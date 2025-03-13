using System;
using UnityEngine;

#if FUSION2
using Fusion;
using LearningExperienceEngine.DataModel;
#endif

namespace MirageXR.View
{
#if FUSION2
    [RequireComponent(typeof(NetworkObject), typeof(NetworkTransform))]
    public class NetworkObjectSynchronizer : NetworkBehaviour
#else
    public class NetworkObjectSynchronizer : MonoBehaviour
#endif
    {
        [Serializable]
        public enum SyncObjectType
        {
            Step,
            Content,
        }

#if FUSION2
        [Networked] private Guid SourceId { get; set; }
        //[Networked] private Content Content { get; set; }
        [Networked] private SyncObjectType SyncObject { get; set; }
        [Networked] private bool IsLocked { get; set; }
        [Networked] private int LockerId { get; set; }

        public NetworkObject NetworkObject => _networkObject;

#if UNITY_EDITOR
        [SerializeField, ReadOnly] private string sourceIdString;
        [SerializeField, ReadOnly] private string typeString;
#endif

        private NetworkObject _networkObject;
        private NetworkTransform _networkTransform;
        private Transform _sourceTransform;

        private void Update()
        {
            if (SourceId == Guid.Empty)
            {
                return;
            }

            CheckSyncObject();
            SyncTransforms();
        }

        private void CheckSyncObject()
        {
            if (_sourceTransform is not null)
            {
                return;
            }

            /*switch (SyncObject)
            {
                case SyncObjectType.Step:
                    var step = ActivityView.Instance.GetStep(SourceId);
                    Initialization(step);
                    break;
                case SyncObjectType.Content:
                    var content = ActivityView.Instance.GetContent(SourceId);
                    Initialization(content);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }*/
        }

        private void SyncTransforms()
        {
            if (IsLocked && RootObject.Instance.CollaborationManager.PlayerId != LockerId)
            {
                _sourceTransform.position = transform.position;
                _sourceTransform.rotation = transform.rotation;
                _sourceTransform.localScale = transform.localScale;
            }
            else
            {
                transform.position = _sourceTransform.position;
                transform.rotation = _sourceTransform.rotation;
                transform.localScale = _sourceTransform.localScale;
            }

#if UNITY_EDITOR
            sourceIdString = SourceId.ToString();
            typeString = SyncObject.ToString();
#endif
        }

        public void Initialization(ContentView contentView)
        {
            if (contentView is null)
            {
                Destroy(gameObject);
                return;
            }

            _networkObject = GetComponent<NetworkObject>();
            _networkTransform = GetComponent<NetworkTransform>();

            SyncObject = SyncObjectType.Content;
            SourceId = contentView.Id;
            //Content = contentView.GetContent();

            Initialization(contentView.transform);
        }

        public void Initialization(StepView stepView)
        {
            if (stepView is null)
            {
                Destroy(gameObject);
                return;
            }

            _networkObject = GetComponent<NetworkObject>();
            _networkTransform = GetComponent<NetworkTransform>();

            SyncObject = SyncObjectType.Step;
            SourceId = stepView.Id;

            Initialization(stepView.transform);
        }

        private void Initialization(Transform sourceTransform)
        {
            _sourceTransform = sourceTransform;
        }
#endif
    }
}