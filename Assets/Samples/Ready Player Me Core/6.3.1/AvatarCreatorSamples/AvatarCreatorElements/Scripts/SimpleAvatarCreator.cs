using System.Collections.Generic;
using System.Threading.Tasks;
using ReadyPlayerMe.AvatarCreator;
using ReadyPlayerMe.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

#pragma warning disable CS4014
#pragma warning disable CS1998

namespace ReadyPlayerMe.Samples.AvatarCreatorElements
{

    /// <summary>
    ///     A class responsible for creating and customizing avatars using asset and color selections.
    /// </summary>
    [RequireComponent(typeof(SessionHandler))]
    public class SimpleAvatarCreator : MonoBehaviour
    {
        public UnityEvent<AvatarProperties> OnAvatarLoaded;
        public UnityEvent OnAvatarSelected;
        
        [SerializeField] private List<AssetSelectionElement> assetSelectionElements;
        [SerializeField] private List<ColorSelectionElement> colorSelectionElements;
        [SerializeField] private BodyShapeSelectionElement bodyShapeSelectionElement;
        [SerializeField] private RuntimeAnimatorController animationController;
        [SerializeField] private GameObject loading;

        [SerializeField] private BodyType bodyType = BodyType.FullBody;
        [SerializeField] private GameObject createRPMAccount;
        private OutfitGender gender = OutfitGender.Masculine;

        private AvatarManager avatarManager;
        private GameObject avatar;

        public async void SelectAvatar(string avatarId)
        {
            await LoadAvatarWithId(avatarId);
            OnAvatarSelected?.Invoke();
        }
        
        public async void LoadAvatar(string avatarId)
        {
            await LoadAvatarWithId(avatarId);
        }

        private async Task<AvatarProperties> LoadAvatarWithId(string avatarId)
        {
            loading.SetActive(true);
            var newAvatar = await avatarManager.GetAvatar(avatarId, bodyType);
            // Destroy the old avatar and replace it with the new one.
            if (avatar != null)
            {
                Destroy(avatar);
            }
            avatar = newAvatar;
            var avatarProperties = await avatarManager.GetAvatarProperties(avatarId);

            var previousGender = gender;
            gender = avatarProperties.Gender;
            if (avatarProperties.Gender != previousGender)
            {
                LoadAssets();
            }

            SetupAvatar();

            OnAvatarLoaded?.Invoke(avatarProperties);
            AuthManager.StoreLastModifiedAvatar(avatarId);
            loading.SetActive(false);

            return avatarProperties;
            
        }

        public async void SignupAndSaveAvatar()
        {
            if (!AuthManager.IsSignedIn)
            {
                createRPMAccount.SetActive(true);
                return;
            }
            SaveAvatar();
        }

        public async void SaveAvatar()
        {
            loading.SetActive(true);
            await avatarManager.Save();
            loading.SetActive(false);
            OnAvatarSelected?.Invoke();
            AuthManager.StoreLastModifiedAvatar(avatarManager.AvatarId);
        }

        public void CreateSecondTemplateAvatar()
        {
            CreateTemplateAvatar();
        }

        public void LoadAvatarFromTemplate(IAssetData template)
        {
            LoadAvatarFromTemplate(template.Id);
        }

        public async Task<AvatarProperties> LoadAvatarFromTemplate(string templateId)
        {
            loading.SetActive(true);
            var templateAvatarResponse = await avatarManager.CreateAvatarFromTemplateAsync(templateId, bodyType);

            // Destroy the old avatar and replace it with the new one.
            if (avatar != null)
            {
                Destroy(avatar);
            }
            var previousGender = gender;
            avatar = templateAvatarResponse.AvatarObject;
            gender = templateAvatarResponse.Properties.Gender;
            if (gender != previousGender)
            {
                LoadAssets();
            }
            SetupAvatar();

            OnAvatarLoaded?.Invoke(templateAvatarResponse.Properties);
            loading.SetActive(false);
            
            return templateAvatarResponse.Properties;
        }

        private void Awake()
        {
            avatarManager = new AvatarManager();
        }


        /// <summary>
        ///     LoadUser is used to initialize the avatar creator and loads initial avatar assets.
        /// </summary>
        public async void LoadUserAssets(UserSession session)
        {
            loading.SetActive(true);
            LoadAssets();
            
            var avatarProperties = string.IsNullOrEmpty(session.LastModifiedAvatarId) ? await CreateTemplateAvatar(): await LoadAvatarWithId(session.LastModifiedAvatarId);
            GetColors(avatarProperties);
            loading.SetActive(false);
        }

        private void OnEnable()
        {
            bodyShapeSelectionElement.OnAssetSelected.AddListener(OnAssetSelection);
            // Subscribes to asset selection events when this component is enabled.
            foreach (var element in assetSelectionElements)
            {
                element.SetBodyType(bodyType);
                element.OnAssetSelected.AddListener(OnAssetSelection);
            }

            foreach (var element in colorSelectionElements)
            {
                element.OnAssetSelected.AddListener(OnAssetSelection);
            }
        }

        private void OnDisable()
        {
            bodyShapeSelectionElement.OnAssetSelected.RemoveListener(OnAssetSelection);
            // Unsubscribes from asset selection events when this component is disabled.
            foreach (var element in assetSelectionElements)
            {
                element.OnAssetSelected.RemoveListener(OnAssetSelection);
            }

            foreach (var element in colorSelectionElements)
            {
                element.OnAssetSelected.RemoveListener(OnAssetSelection);
            }
        }

        /// <summary>
        ///     Handles the selection of an asset and updates the avatar accordingly.
        /// </summary>
        /// <param name="assetData">The selected asset data.</param>
        private async void OnAssetSelection(IAssetData assetData)
        {
            loading.SetActive(true);
            var newAvatar = await avatarManager.UpdateAsset(assetData.AssetType, bodyType, assetData.Id);

            // Destroy the old avatar and replace it with the new one.
            if (avatar != null)
            {
                Destroy(avatar);
            }
            avatar = newAvatar;
            SetupAvatar();
            loading.SetActive(false);
        }

        /// <summary>
        ///     Loads and initializes asset selection elements for avatar customization.
        /// </summary>
        private async void LoadAssets()
        {
            bodyShapeSelectionElement.LoadAndCreateButtons();
            foreach (var element in assetSelectionElements)
            {
                element.LoadAndCreateButtons(gender);
            }
        }

        /// <summary>
        ///     Loads and initializes color selection elements for choosing avatar colors.
        /// </summary>
        /// <param name="avatarProperties">The properties of the avatar.</param>
        private void GetColors(AvatarProperties avatarProperties)
        {
            foreach (var element in colorSelectionElements)
            {
                element.LoadAndCreateButtons(avatarProperties);
            }
        }

        /// <summary>
        ///     Creates an avatar from a template and sets its initial properties.
        /// </summary>
        /// <returns>The properties of the created avatar.</returns>
        private async Task<AvatarProperties> CreateTemplateAvatar()
        {
            var avatarTemplateFetcher = new AvatarTemplateFetcher();
            var templates = await avatarTemplateFetcher.GetTemplates();
            var avatarTemplate = templates[1];
            return await LoadAvatarFromTemplate(avatarTemplate.Id);
        }

        /// <summary>
        ///     Sets additional elements and components on the created avatar, such as mouse rotation and animation controller.
        /// </summary>
        private void SetupAvatar()
        {
            avatar.AddComponent<MouseRotationHandler>();
            avatar.AddComponent<AvatarRotator>();
            var animator = avatar.GetComponent<Animator>();
            AvatarAnimationHelper.SetupAnimator(new AvatarMetadata
                { BodyType = bodyType, OutfitGender = gender }, animator);
            animator.runtimeAnimatorController = animationController;
        }

        public async void OnAvatarDeleted(string avatarId)
        {
            if (AuthManager.UserSession.LastModifiedAvatarId == avatarId)
            {
                AuthManager.StoreLastModifiedAvatar(null);
            }
            if (avatarManager.AvatarId == avatarId)
            {
                loading.SetActive(true);
                await CreateTemplateAvatar();
                loading.SetActive(false);
            }
        }
    }
}
