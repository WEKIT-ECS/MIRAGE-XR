using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ReadyPlayerMe.Core;
using UnityEngine;
using UnityEngine.Events;

namespace ReadyPlayerMe.AvatarCreator
{
    public class AvatarListElement : MonoBehaviour
    {
        private enum AvatarListFilter
        {
            Application,
            None
        }

        private const string TAG = nameof(AvatarListElement);

        [SerializeField] private Transform avatarsContainer;
        [SerializeField] private UserAvatarElement userAvatarElement;
        [SerializeField] private AvatarListFilter avatarListFilter;

        public UnityEvent<string> OnAvatarSelect;
        public UnityEvent<string> OnAvatarModify;
        public UnityEvent<string> OnAvatarDeletionStarted;
        public UnityEvent<IEnumerable<string>> OnAvatarsLoaded;

        private AvatarAPIRequests avatarAPIRequests;

        private readonly Dictionary<string, UserAvatarElement> partnerByAvatarId = new();
        private readonly CancellationTokenSource cancellationTokenSource = new();

        public async void LoadAndCreateUserAvatars()
        {
            if (!AuthManager.IsSignedIn && !AuthManager.IsSignedInAnonymously)
            {
                SDKLogger.Log(TAG, "Not signed in");
                return;
            }

            avatarAPIRequests ??= new AvatarAPIRequests(cancellationTokenSource.Token);

            var avatarPartnerArr = await TaskExtensions.HandleCancellation(avatarAPIRequests.GetUserAvatars(AuthManager.UserSession.Id));

            if (avatarPartnerArr == null)
            {
                return;
            }

            if (avatarListFilter == AvatarListFilter.Application)
            {
                var avatarIds = avatarPartnerArr.Where((avatar) => avatar.Partner == CoreSettingsHandler.CoreSettings.Subdomain).Select(avatar => avatar.Id).ToList();
                CreateButtons(avatarIds);
            }
            else
            {
                CreateButtons(avatarPartnerArr.Select(avatar => avatar.Id).ToList());
            }

        }

        private void OnDestroy()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }

        public void RemoveItem(string avatarId)
        {
            if (!partnerByAvatarId.ContainsKey(avatarId))
            {
                return;
            }

            Destroy(partnerByAvatarId[avatarId].gameObject);
            partnerByAvatarId.Remove(avatarId);
        }

        private void CreateButtons(IEnumerable<string> avatars)
        {
            ClearButtons();
            foreach (var avatarId in avatars)
            {
                CreateButton(avatarId);
            }
            OnAvatarsLoaded?.Invoke(avatars);
        }

        private void ClearButtons()
        {
            if (partnerByAvatarId.Count > 0)
            {
                foreach (var avatarListItem in partnerByAvatarId)
                {
                    Destroy(avatarListItem.Value.gameObject);
                }
            }
            partnerByAvatarId.Clear();
        }

        private void CreateButton(string avatarId)
        {
            var avatarListElement = Instantiate(userAvatarElement, avatarsContainer).GetComponent<UserAvatarElement>();
            avatarListElement.SetupButton(avatarId, OnAvatarElementButtonClicked);
            partnerByAvatarId.Add(avatarId, avatarListElement);
        }

        private void OnAvatarElementButtonClicked(UserAvatarElement.AvatarListItemAction action)
        {
            switch (action.ActionType)
            {
                case UserAvatarElement.ButtonAction.Customize:
                    OnAvatarModified(action.AvatarId);
                    break;
                case UserAvatarElement.ButtonAction.Delete:
                    OnAvatarDeletionStarted?.Invoke(action.AvatarId);
                    break;
                case UserAvatarElement.ButtonAction.Select:
                    OnAvatarSelected(action.AvatarId);
                    break;
            }
        }

        private void OnAvatarModified(string avatarId)
        {
            SDKLogger.Log(TAG, $"Started modifying avatar with id {avatarId}");
            OnAvatarModify?.Invoke(avatarId);
        }

        private void OnAvatarSelected(string avatarId)
        {
            SDKLogger.Log(TAG, $"Selected avatar with id {avatarId}");
            OnAvatarSelect?.Invoke(avatarId);
        }
    }
}
