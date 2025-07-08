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
    /// For downloading and filtering all partner assets.
    /// </summary>
    public class PartnerAssetsManager : IDisposable
    {
        private const string TAG = nameof(PartnerAssetsManager);
        private const string ASSET_ICON_SIZE = "?w=64";

        private readonly AssetAPIRequests assetAPIRequests;

        private Dictionary<AssetType, List<PartnerAsset>> assetsByCategory;
        public Action<string> OnError { get; set; }

        public PartnerAssetsManager()
        {
            assetAPIRequests = new AssetAPIRequests(CoreSettingsHandler.CoreSettings.AppId);
            assetsByCategory = new Dictionary<AssetType, List<PartnerAsset>>();
        }

        public async Task<Dictionary<AssetType, List<PartnerAsset>>> GetAssets(OutfitGender gender, CancellationToken token = default)
        {
            var startTime = Time.time;

            var assets = await assetAPIRequests.Get(gender, token);

            assetsByCategory = assets.GroupBy(asset => asset.AssetType).ToDictionary(
                group => group.Key,
                group => group.ToList()
            );

            if (assets.Length != 0)
            {
                SDKLogger.Log(TAG, $"All assets received: {Time.time - startTime:F2}s");
            }

            return assetsByCategory;
        }

        public List<string> GetAssetsByCategory(AssetType assetType)
        {
            return assetsByCategory.TryGetValue(assetType, out var _) ? assetsByCategory[assetType].Select(x => x.Id).ToList() : new List<string>();
        }

        public async Task DownloadIconsByCategory(AssetType assetType, Action<string, Texture> onDownload, CancellationToken token = default)
        {
            var startTime = Time.time;
            var chunkList = assetsByCategory[assetType].ChunkBy(20);

            foreach (var list in chunkList)
            {
                try
                {
                    await DownloadIcons(list, onDownload, token);
                    SDKLogger.Log(TAG, $"Download chunk of {assetType} icons: " + (Time.time - startTime) + "s");
                }
                catch (Exception e)
                {
                    OnError?.Invoke(e.Message);
                    return;
                }

                if (token.IsCancellationRequested)
                {
                    return;
                }
                await Task.Yield();
            }
        }

        public bool IsLockedAssetCategories(AssetType assetType, string id)
        {
            if (!assetsByCategory.ContainsKey(assetType))
            {
                return false;
            }

            var asset = assetsByCategory[assetType].FirstOrDefault(x => x.Id == id);
            return asset.LockedCategories != null && asset.LockedCategories.Length > 0;
        }

        private async Task DownloadIcons(List<PartnerAsset> chunk, Action<string, Texture> onDownload, CancellationToken token = default)
        {
            var assetIconMap = new Dictionary<string, Task<Texture>>();

            foreach (var asset in chunk)
            {
                var url = $"{asset.ImageUrl}{ASSET_ICON_SIZE}";
                var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
                var iconTask = assetAPIRequests.GetAssetIcon(url, icon =>
                    {
                        onDownload?.Invoke(asset.Id, icon);
                    },
                    linkedTokenSource.Token);
                assetIconMap.Add(asset.Id, iconTask);
            }

            while (!assetIconMap.Values.All(x => x.IsCompleted) && !token.IsCancellationRequested)
            {
                await Task.Yield();
            }
        }

        public void DeleteAssets()
        {
            assetsByCategory.Clear();
        }

        public void Dispose()
        {
            DeleteAssets();
        }

        public PrecompileData GetPrecompileData(AssetType[] categories, int numberOfAssetsPerCategory)
        {
            var categoriesFromMap = CategoryHelper.AssetTypeByValue
                .Where(kvp => categories.Contains(kvp.Value))
                .Select(kvp => kvp.Key)
                .ToArray();

            var dictionary = categoriesFromMap.ToDictionary(category => category, category =>
                GetAssetsByCategory(CategoryHelper.AssetTypeByValue[category])
                    .Take(numberOfAssetsPerCategory)
                    .ToArray());

            return new PrecompileData { data = dictionary };
        }
    }
}
