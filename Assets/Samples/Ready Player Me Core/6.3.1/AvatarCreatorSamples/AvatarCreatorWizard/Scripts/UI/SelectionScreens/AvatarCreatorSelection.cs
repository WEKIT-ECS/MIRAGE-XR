using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ReadyPlayerMe.AvatarCreator;
using ReadyPlayerMe.Core;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ReadyPlayerMe.Samples.AvatarCreatorWizard
{
    public class AvatarCreatorSelection : State, IDisposable
    {
        private const string TAG = nameof(AvatarCreatorSelection);
        private const string UPDATING_YOUR_AVATAR_LOADING_TEXT = "Updating your avatar";
        private const int NUMBER_OF_ASSETS_TO_PRECOMPILE = 20;

        [SerializeField] private CategoryUICreator categoryUICreator;
        [SerializeField] private AssetButtonCreator assetButtonCreator;
        [SerializeField] private Button saveButton;
        [SerializeField] private AvatarConfig inCreatorConfig;
        [SerializeField] private RuntimeAnimatorController animator;
        [SerializeField] private SignupElement signupElement;
        private PartnerAssetsManager partnerAssetManager;
        private AvatarManager avatarManager;

        private GameObject currentAvatar;
        private Quaternion lastRotation;

        private CancellationTokenSource ctxSource;
        public List<AssetType> categoriesAssetsLoaded;

        public override StateType StateType => StateType.Editor;
        public override StateType NextState => StateType.End;

        private void Start()
        {
            partnerAssetManager = new PartnerAssetsManager();
        }

        public override void ActivateState()
        {
            saveButton.onClick.AddListener(OnSaveButton);
            signupElement.OnContinueWithoutSignup.AddListener(Save);
            categoryUICreator.OnCategorySelected += OnCategorySelected;
            Setup();
        }

        public override void DeactivateState()
        {
            saveButton.onClick.RemoveListener(OnSaveButton);
            signupElement.OnContinueWithoutSignup.RemoveListener(Save);
            categoryUICreator.OnCategorySelected -= OnCategorySelected;
            Cleanup();
        }

        private async void Setup()
        {
            LoadingManager.EnableLoading();
            ctxSource = new CancellationTokenSource();

            avatarManager = new AvatarManager(
                inCreatorConfig,
                ctxSource.Token,
                AvatarCreatorData.AvatarProperties.Gender);
            avatarManager.OnError += OnErrorCallback;

            currentAvatar = await LoadAvatar();

            if (string.IsNullOrEmpty(avatarManager.AvatarId))
                return;

            CreateUI(AvatarCreatorData.AvatarProperties.BodyType);

            await LoadAssets();
            await LoadAvatarColors();
            ToggleCategoryButtons();

            LoadingManager.DisableLoading();
        }

        private void ToggleCategoryButtons()
        {
            var assets = AvatarCreatorData.AvatarProperties.Assets;
            if (!assets.TryGetValue(AssetType.Outfit, out var outfitId))
            {
                return;
            }

            if (partnerAssetManager.IsLockedAssetCategories(AssetType.Outfit, outfitId.ToString()))
            {
                categoryUICreator.SetActiveCategoryButtons(false);
                categoryUICreator.SetDefaultSelection(AssetType.Outfit);
            }
            else
            {
                categoryUICreator.SetActiveCategoryButtons(true);
            }
        }

        private void Cleanup()
        {
            if (currentAvatar != null)
            {
                Destroy(currentAvatar);
            }

            avatarManager.Delete(true);
            partnerAssetManager.DeleteAssets();

            Dispose();
            categoryUICreator.ResetUI();
            assetButtonCreator.ResetUI();
        }

        private void OnErrorCallback(string error)
        {
            if (error.Equals("Avatar draft not found"))
            {
                return;
            }

            SDKLogger.Log(TAG, $"An error occured: {error}");
            avatarManager.OnError -= OnErrorCallback;
            partnerAssetManager.OnError -= OnErrorCallback;

            ctxSource?.Cancel();
            StateMachine.GoToPreviousState();
            LoadingManager.EnableLoading(error, LoadingManager.LoadingType.Popup, false);
            SDKLogger.Log(TAG, "Going to previous state");
        }

        private async Task LoadAssets()
        {
            var startTime = Time.time;

            partnerAssetManager.OnError += OnErrorCallback;
            categoriesAssetsLoaded = new List<AssetType>();

            await partnerAssetManager.GetAssets(AvatarCreatorData.AvatarProperties.BodyType, AvatarCreatorData.AvatarProperties.Gender, ctxSource.Token);
            await CreateAssetsByCategory(AssetType.FaceShape);

            SDKLogger.Log(TAG, $"Loaded all partner assets {Time.time - startTime:F2}s");
        }

        private async void OnCategorySelected(AssetType category)
        {
            await CreateAssetsByCategory(category);
            avatarManager.PrecompileAvatar(AvatarCreatorData.AvatarProperties.Id, partnerAssetManager.GetPrecompileData(new[] { category }, NUMBER_OF_ASSETS_TO_PRECOMPILE));
        }

        private async Task<GameObject> LoadAvatar()
        {
            var startTime = Time.time;

            GameObject avatar;

            if (string.IsNullOrEmpty(AvatarCreatorData.AvatarProperties.Id))
            {
                AvatarCreatorData.AvatarProperties.Assets ??= GetDefaultAssets();
                var avatarResponse = await avatarManager.CreateAvatarAsync(AvatarCreatorData.AvatarProperties);
                avatar = avatarResponse.AvatarObject;
                AvatarCreatorData.AvatarProperties = avatarResponse.Properties;
            }
            else
            {
                var id = AvatarCreatorData.AvatarProperties.Id;
                var bodyType = AvatarCreatorData.AvatarProperties.BodyType;
                if (!AvatarCreatorData.IsExistingAvatar)
                {
                    var avatarTemplateResponse = await avatarManager.CreateAvatarFromTemplateAsync(id, bodyType);
                    avatar = avatarTemplateResponse.AvatarObject;
                    AvatarCreatorData.AvatarProperties = avatarTemplateResponse.Properties;
                }
                else
                {
                    avatar = await avatarManager.GetAvatar(id, bodyType, AvatarCreatorData.AvatarProperties.isDraft);
                }
            }

            if (avatar == null)
            {
                return null;
            }

            ProcessAvatar(avatar);

            SDKLogger.Log(TAG, $"Avatar loaded in {Time.time - startTime:F2}s");
            return avatar;
        }

        private async Task LoadAvatarColors()
        {
            var startTime = Time.time;
            var colors = await avatarManager.LoadAvatarColors();
            assetButtonCreator.CreateColorUI(colors, UpdateAvatar);
            SDKLogger.Log(TAG, $"All colors loaded in {Time.time - startTime:F2}s");
        }

        private void CreateUI(BodyType bodyType)
        {
            categoryUICreator.Setup(bodyType);
            assetButtonCreator.SetSelectedAssets(AvatarCreatorData.AvatarProperties.Assets);
            assetButtonCreator.CreateClearButton(UpdateAvatar);
            saveButton.gameObject.SetActive(true);
        }

        private async Task CreateAssetsByCategory(AssetType category)
        {
            if (categoriesAssetsLoaded.Contains(category))
            {
                return;
            }

            categoriesAssetsLoaded.Add(category);

            var assets = partnerAssetManager.GetAssetsByCategory(category);
            if (assets == null || assets.Count == 0)
            {
                return;
            }
            assetButtonCreator.CreateAssetButtons(assets, category, OnAssetButtonClicked);
            await partnerAssetManager.DownloadIconsByCategory(category, assetButtonCreator.SetAssetIcon, ctxSource.Token);

            if (category == AssetType.EyeShape)
            {
                await CreateAssetsByCategory(AssetType.EyeColor);
            }
        }

        private void OnAssetButtonClicked(string id, AssetType category)
        {
            categoryUICreator.SetActiveCategoryButtons(!partnerAssetManager.IsLockedAssetCategories(category, id));
            UpdateAvatar(id, category);
        }

        private void OnSaveButton()
        {
            if (AuthManager.IsSignedIn)
            {
                Save();
            }
            else
            {
                signupElement.gameObject.SetActive(true);
            }
        }

        private void OnSendEmail(string email)
        {
            Save();
        }

        private async void Save()
        {
            AuthManager.StoreLastModifiedAvatar(null);
            var startTime = Time.time;

            LoadingManager.EnableLoading("Saving avatar...", LoadingManager.LoadingType.Popup);
            if (AvatarCreatorData.AvatarProperties.isDraft)
            {
                var avatarId = await avatarManager.Save();
                AvatarCreatorData.AvatarProperties.Id = avatarId;
            }
            StateMachine.SetState(StateType.End);
            LoadingManager.DisableLoading();
            SDKLogger.Log(TAG, $"Avatar saved in {Time.time - startTime:F2}s");
        }

        private Dictionary<AssetType, object> GetDefaultAssets()
        {
            if (string.IsNullOrEmpty(AvatarCreatorData.AvatarProperties.Base64Image))
            {
                return AvatarCreatorData.AvatarProperties.Gender == OutfitGender.Feminine
                    ? AvatarPropertiesConstants.FemaleDefaultAssets
                    : AvatarPropertiesConstants.MaleDefaultAssets;
            }

            return new Dictionary<AssetType, object>();
        }

        private async void UpdateAvatar(object assetId, AssetType category)
        {
            var startTime = Time.time;

            var payload = new AvatarProperties
            {
                Assets = new Dictionary<AssetType, object>()
            };

            payload.Assets.Add(category, assetId);
            lastRotation = currentAvatar.transform.rotation;
            LoadingManager.EnableLoading(UPDATING_YOUR_AVATAR_LOADING_TEXT, LoadingManager.LoadingType.Popup);

            if (!AvatarCreatorData.AvatarProperties.isDraft)
            {
                await avatarManager.Delete(true);
            }

            var avatar = await avatarManager.UpdateAsset(category, AvatarCreatorData.AvatarProperties.BodyType, assetId);
            if (avatar == null)
            {
                return;
            }
            AvatarCreatorData.AvatarProperties.isDraft = true;
            AuthManager.StoreLastModifiedAvatar(AvatarCreatorData.AvatarProperties.Id);
            ProcessAvatar(avatar);
            Destroy(currentAvatar);
            currentAvatar = avatar;
            LoadingManager.DisableLoading();
            SDKLogger.Log(TAG, $"Avatar updated in {Time.time - startTime:F2}s");
        }

        private void ProcessAvatar(GameObject avatar)
        {
            if (AvatarCreatorData.AvatarProperties.BodyType != BodyType.None && AvatarCreatorData.AvatarProperties.BodyType != BodyType.HalfBody)
            {
                avatar.GetComponent<Animator>().runtimeAnimatorController = animator;
            }
            avatar.transform.rotation = lastRotation;
            avatar.AddComponent<MouseRotationHandler>();
            avatar.AddComponent<AvatarRotator>();
        }

        public void Dispose()
        {
            partnerAssetManager.OnError -= OnErrorCallback;
            partnerAssetManager?.Dispose();

            avatarManager.OnError -= OnErrorCallback;
            avatarManager?.Dispose();
        }
    }
}
