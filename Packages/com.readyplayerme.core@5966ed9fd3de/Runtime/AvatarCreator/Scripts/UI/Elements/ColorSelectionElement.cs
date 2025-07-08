using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ReadyPlayerMe.AvatarCreator
{
    /// <summary>
    /// This class is responsible for fetching color data based on avatar properties,
    /// and creating buttons for each color.
    /// It extends the functionality of SelectionElement to handle color-specific interactions.
    /// </summary>
    public class ColorSelectionElement : SelectionElement
    {
        [Header("Properties")]
        [SerializeField, AssetTypeFilter(AssetFilter.Color)] private AssetType assetType;
        private AssetColor[] colorAssets;
        private CancellationTokenSource cancellationTokenSource;
        private AvatarAPIRequests avatarAPIRequests;

        private void Awake()
        {
            cancellationTokenSource = new CancellationTokenSource();
            avatarAPIRequests = new AvatarAPIRequests(cancellationTokenSource.Token);
        }

        public async Task LoadColorPalette(AvatarProperties avatarProperties)
        {
            colorAssets = await avatarAPIRequests.GetAvatarColors(avatarProperties.Id, assetType);
        }

        private void OnDestroy()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }

        public async void LoadAndCreateButtons(AvatarProperties avatarProperties)
        {
            await TaskExtensions.HandleCancellation(LoadColorPalette(avatarProperties));
            if (colorAssets == null)
            {
                return;
            }
            CreateButtons(colorAssets, (button, asset) =>
            {
                button.SetColor(asset.HexColor);
            });
        }
    }
}
