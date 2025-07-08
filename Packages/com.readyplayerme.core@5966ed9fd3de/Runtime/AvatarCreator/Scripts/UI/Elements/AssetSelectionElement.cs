using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ReadyPlayerMe.Core;
using UnityEngine;

namespace ReadyPlayerMe.AvatarCreator
{

    /// <summary>
    /// Manages the selection of assets for a given body type, gender, and asset type.
    /// This class is responsible for loading asset data and creating corresponding buttons for each asset.
    /// It extends the functionality of SelectionElement to handle PartnerAsset-specific interactions.
    /// </summary>
    public class AssetSelectionElement : SelectionElement
    {
        [Header("Properties"), SerializeField]
        private SelectionButton clearSelectionButton;
        [SerializeField, AssetTypeFilter(AssetFilter.Style)] private AssetType assetType;
        [SerializeField] private int iconSize = 64;

        private PartnerAsset[] assets;

        private AssetAPIRequests assetsRequests;
        private AssetAPIRequests AssetsRequests => assetsRequests ??= new AssetAPIRequests(CoreSettingsHandler.CoreSettings.AppId);

        private readonly CancellationTokenSource cancellationTokenSource = new();

        private void Awake()
        {
            if (clearSelectionButton == null) return;
            AddClearButton(clearSelectionButton, assetType);
        }

        private void OnDestroy()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }

        private void CreateClearButton()
        {
            var clearButton = Instantiate(clearSelectionButton, ButtonContainer);
            clearButton.transform.SetAsFirstSibling();
            var assetData = new PartnerAsset { Id = "0", AssetType = assetType };
            clearButton.GetComponent<SelectionButton>().AddListener(() => OnAssetSelected?.Invoke(assetData));
        }

        /// <summary>
        /// Asynchronously loads asset data based on the specified asset type, body type, and gender.
        /// This method updates the internal partnerAssets collection with the fetched data.
        /// </summary>
        /// <param name="bodyType">The body type to filter the assets and determine the type of avatar to load.</param>
        /// <param name="gender">The gender to filter the assets and determine which skeleton will be loaded.</param>
        /// <returns>A Task representing the asynchronous operation of fetching and loading the partner asset data.</returns>
        public async Task LoadAssetData(OutfitGender gender)
        {
            assets = await AssetsRequests.Get(assetType, gender, cancellationTokenSource.Token);
        }

        /// <summary>
        /// Asynchronously loads the template data and creates button elements for each asset. 
        /// Buttons are created with icons fetched based on the asset's image URL and icon size.
        /// </summary>
        public async Task LoadAndCreateButtons(OutfitGender gender)
        {
            await LoadAssetData(gender);
            CreateButtons(assets.ToArray(), OnButtonCreated);
        }

        private async void OnButtonCreated(SelectionButton button, PartnerAsset asset)
        {
            var webRequestDispatcher = new WebRequestDispatcher();
            var url = iconSize > 0 ? $"{asset.ImageUrl}?w={iconSize}" : asset.ImageUrl;

            var texture = await TaskExtensions.HandleCancellation(webRequestDispatcher.DownloadTexture(url, cancellationTokenSource.Token));
            if (texture == null)
            {
                return;
            }
            button.SetIcon(texture);
        }

        public void SetAssetSelected(AvatarProperties avatarProperties)
        {
            if (!avatarProperties.Assets.ContainsKey(assetType))
            {
                Debug.Log($"Avatar properties does not contain asset type {assetType}");
                return;
            }
            var assetId = avatarProperties.Assets[assetType] as string;
            if (string.IsNullOrEmpty(assetId))
            {
                Debug.Log($"Asset id is null or empty {assetId} on type {assetType}");
                return;
            }
            SetButtonSelected(assetId);
        }
    }
}
