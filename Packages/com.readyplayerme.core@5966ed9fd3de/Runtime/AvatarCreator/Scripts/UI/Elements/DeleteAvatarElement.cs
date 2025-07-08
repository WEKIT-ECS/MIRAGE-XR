using System;
using System.Threading;
using ReadyPlayerMe.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ReadyPlayerMe.AvatarCreator
{
    public class DeleteAvatarElement : MonoBehaviour
    {
        private const string TAG = nameof(AvatarListElement);

        [SerializeField]
        private Button confirmButton;
        [SerializeField]
        private Button cancelButton;

        public UnityEvent<string> onCancel;
        public UnityEvent<string> onConfirm;
        public UnityEvent<string> onError;

        private string avatarId;
        private AvatarAPIRequests avatarAPIRequests;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private void Awake()
        {
            confirmButton.onClick.AddListener(DeleteAvatar);
            cancelButton.onClick.AddListener(Cancel);
            avatarAPIRequests = new AvatarAPIRequests(cancellationTokenSource.Token);
        }

        private void OnDestroy()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }

        public void SetAvatarId(string avatarId)
        {
            this.avatarId = avatarId;
        }

        private void Cancel()
        {
            avatarId = null;
            onCancel?.Invoke(this.avatarId);
        }

        private async void DeleteAvatar()
        {
            if (avatarId == null)
            {
                SDKLogger.LogWarning(TAG, "AvatarId is not set");
                return;
            }
            try
            {
                await avatarAPIRequests.DeleteAvatar(avatarId);
                onConfirm?.Invoke(avatarId);
                avatarId = null;
            }
            catch (Exception e)
            {
                onError?.Invoke(e.Message);
            }

        }
    }
}
