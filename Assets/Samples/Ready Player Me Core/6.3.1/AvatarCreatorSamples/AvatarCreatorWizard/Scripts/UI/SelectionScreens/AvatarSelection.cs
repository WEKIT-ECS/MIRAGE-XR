using System;
using System.Collections.Generic;
using ReadyPlayerMe.AvatarCreator;
using ReadyPlayerMe.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ReadyPlayerMe.Samples.AvatarCreatorWizard
{
    public class AvatarSelection : State
    {
        private const string TAG = nameof(AvatarSelection);

        [SerializeField] private Button partnerAvatarsButton;
        [SerializeField] private Button allAvatarsButton;
        [SerializeField] private Button createAvatarButton;
        [SerializeField] private GameObject buttonPrefab;
        [SerializeField] private Transform parent;
        public override StateType StateType => StateType.AvatarSelection;
        public override StateType NextState => StateType.Editor;

        private Dictionary<string, string> avatarPartnerMap;
        private Dictionary<string, GameObject> avatarButtonsMap;
        private AvatarAPIRequests avatarAPIRequests;

        public override void ActivateState()
        {
            partnerAvatarsButton.onClick.AddListener(OnPartnerAvatarsButton);
            allAvatarsButton.onClick.AddListener(OnAllAvatarsButton);
            createAvatarButton.onClick.AddListener(OnCreateAvatarButton);
            AuthManager.OnSignedOut += OnSignedOut;

            CreateAvatarButtons();
        }

        public override void DeactivateState()
        {
            partnerAvatarsButton.onClick.RemoveListener(OnPartnerAvatarsButton);
            allAvatarsButton.onClick.RemoveListener(OnAllAvatarsButton);
            createAvatarButton.onClick.RemoveListener(OnCreateAvatarButton);
            AuthManager.OnSignedOut -= OnSignedOut;
        }

        private async void CreateAvatarButtons()
        {
            var startTime = Time.time;
            if (avatarPartnerMap != null && avatarPartnerMap.Count != 0)
            {
                return;
            }

            LoadingManager.EnableLoading();

            avatarAPIRequests = new AvatarAPIRequests();
            try
            {
                avatarPartnerMap = await avatarAPIRequests.GetUserAvatars(AuthManager.UserSession.Id);
            }
            catch (Exception e)
            {
                SDKLogger.Log(TAG, $"Fetching all users failed with exception: {e}");
                SDKLogger.Log(TAG, "Logging out user");
                AuthManager.Logout();
                LoadingManager.DisableLoading();
                return;
            }

            SDKLogger.Log(TAG, $"Fetched all users templates in {Time.time - startTime:F2}s ");

            avatarButtonsMap = new Dictionary<string, GameObject>();
            foreach (var avatar in avatarPartnerMap)
            {
                CreateButton(avatar.Key);
            }

            OnPartnerAvatarsButton();
            LoadingManager.DisableLoading();
        }

        private void OnAllAvatarsButton()
        {
            foreach (var avatar in avatarPartnerMap)
            {
                avatarButtonsMap[avatar.Key].SetActive(true);
            }
        }

        private void OnPartnerAvatarsButton()
        {
            foreach (var avatar in avatarPartnerMap)
            {
                avatarButtonsMap[avatar.Key].SetActive(avatar.Value == AvatarCreatorData.AvatarProperties.Partner);
            }
        }

        private void OnCreateAvatarButton()
        {
            AvatarCreatorData.AvatarProperties.Id = string.Empty;
            StateMachine.SetState(StateType.GenderSelection);
        }

        private void OnSignedOut()
        {
            if (avatarButtonsMap == null) return;
            foreach (var avatars in avatarButtonsMap)
            {
                Destroy(avatars.Value);
            }
            avatarButtonsMap.Clear();
            avatarPartnerMap.Clear();
        }

        private void CreateButton(string avatarId)
        {
            var button = Instantiate(buttonPrefab, parent);
            button.GetComponent<AvatarButton>().Init(avatarId, () => OnCustomize(avatarId), () => OnSelected(avatarId),
                avatarPartnerMap[avatarId] == AvatarCreatorData.AvatarProperties.Partner);
            avatarButtonsMap.Add(avatarId, button);
        }

        private async void OnCustomize(string avatarId)
        {
            AvatarCreatorData.AvatarProperties.Id = avatarId;
            AvatarCreatorData.AvatarProperties = await avatarAPIRequests.GetAvatarMetadata(avatarId);
            AvatarCreatorData.IsExistingAvatar = true;
            StateMachine.SetState(StateType.Editor);
        }

        private void OnSelected(string avatarId)
        {
            AvatarCreatorData.AvatarProperties.Id = avatarId;
            StateMachine.SetState(StateType.End);
        }
    }
}
