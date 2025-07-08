using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReadyPlayerMe.Core;
using UnityEngine;
using UnityEngine.Networking;

namespace ReadyPlayerMe.AvatarCreator
{
    public class AssetAPIRequests
    {
        private const string RPM_ASSET_V1_BASE_URL = Env.RPM_API_V1_BASE_URL + "assets";

        private const string TAG = nameof(AssetAPIRequests);
        private const int LIMIT = 100;

        private readonly AuthorizedRequest authorizedRequest;
        private readonly string appId;
        private readonly Dictionary<string, Texture> icons;

        public AssetAPIRequests(string appId)
        {
            authorizedRequest = new AuthorizedRequest();
            icons = new Dictionary<string, Texture>();
            this.appId = appId;
        }

        public async Task<PartnerAsset[]> Get(OutfitGender gender, CancellationToken ctx = new())
        {
            var assets = new HashSet<PartnerAsset>();
            AssetLibrary assetLibrary;

            try
            {
                assetLibrary = await GetRequest(LIMIT, 1, null, gender, ctx);
                assets.UnionWith(assetLibrary.Assets);
            }
            catch (Exception)
            {
                return assets.ToArray();
            }

            var assetRequests = new Task<AssetLibrary>[assetLibrary.Pagination.TotalPages - 1];

            for (var i = 2; i <= assetLibrary.Pagination.TotalPages; i++)
            {
                assetRequests[i - 2] = GetRequest(LIMIT, i, null, gender, ctx);
            }

            while (!assetRequests.All(x => x.IsCompleted) && !ctx.IsCancellationRequested)
            {
                await Task.Yield();
            }

            foreach (var request in assetRequests.Where(request => request.IsCompleted))
            {
                try
                {
                    assets.UnionWith(request.Result.Assets);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return assets.ToArray();
        }

        public async Task<PartnerAsset[]> Get(AssetType? category, OutfitGender gender, CancellationToken ctx = new())
        {
            var assets = new HashSet<PartnerAsset>();
            var assetData = await GetRequest(LIMIT, 1, category, gender, ctx);
            assets.UnionWith(assetData.Assets);

            for (var i = 2; i <= assetData.Pagination.TotalPages; i++)
            {
                assetData = await GetRequest(LIMIT, i, category, gender, ctx);
                assets.UnionWith(assetData.Assets);
            }

            return assets.ToArray();
        }

        private async Task<AssetLibrary> GetRequest(int limit, int pageNumber, AssetType? category, OutfitGender gender, CancellationToken ctx = new())
        {
            var startTime = Time.time;

            var type = string.Empty;
            if (category != null)
            {
                type = CategoryHelper.AssetTypeByValue.First(x => x.Value == category).Key;
            }

            var url = BuildAssetListUrl(type, limit, pageNumber, AuthManager.UserSession.Id, appId, gender == OutfitGender.Masculine ? "male" : "female");

            var response = await authorizedRequest.SendRequest<ResponseText>(new RequestData
            {
                Url = url,
                Method = HttpMethod.GET
            }, ctx);
            response.ThrowIfError();

            var json = JObject.Parse(response.Text);
            var partnerAssets = JsonConvert.DeserializeObject<PartnerAsset[]>(json["data"]!.ToString());
            var pagination = JsonConvert.DeserializeObject<Pagination>(json["pagination"]!.ToString());

            if (category != null)
            {
                SDKLogger.Log(TAG, $"Asset by category {category} with page {pageNumber} received: {Time.time - startTime}s");
            }
            else
            {
                SDKLogger.Log(TAG, $"Asset with page {pageNumber} received: {Time.time - startTime}s");
            }

            return new AssetLibrary
            {
                Assets = partnerAssets,
                Pagination = pagination
            };
        }

        public async Task<Texture> GetAssetIcon(string url, Action<Texture> completed, CancellationToken ctx = new())
        {
            if (icons.ContainsKey(url))
            {
                completed?.Invoke(icons[url]);
                return icons[url];
            }

            var downloadHandler = new DownloadHandlerTexture();
            var webRequestDispatcher = new WebRequestDispatcher();
            var response = await webRequestDispatcher.SendRequest<ResponseTexture>(url, HttpMethod.GET, downloadHandler: downloadHandler, ctx: ctx);

            response.ThrowIfError();

            // This check is needed because the same url can be requested multiple times
            if (!icons.ContainsKey(url))
            {
                icons.Add(url, response.Texture);
            }

            completed?.Invoke(response.Texture);
            return response.Texture;
        }

        private static string BuildAssetListUrl(string type, int limit, int page, string userId, string appId, string gender)
        {
            const string url = RPM_ASSET_V1_BASE_URL + "?limit={0}&page={1}&filter=viewable-by-user-and-app&filterUserId={2}&filterApplicationId={3}&gender=neutral&gender={4}";

            if (string.IsNullOrEmpty(type))
                return string.Format(url, limit, page, userId, appId, gender);

            return string.Format(url, limit, page, userId, appId, gender) + "&type=" + type;
        }
    }
}
