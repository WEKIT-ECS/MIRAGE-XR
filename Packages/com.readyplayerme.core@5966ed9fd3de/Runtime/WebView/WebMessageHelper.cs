namespace ReadyPlayerMe.Core.WebView
{
    public struct AssetRecord
    {
        public string UserId;
        public string AssetId;
    }

    public static class WebMessageHelper
    {
        private const string DATA_URL_FIELD_NAME = "url";
        private const string ID_KEY = "id";
        private const string USER_ID_KEY = "userId";
        private const string ASSET_ID_KEY = "assetId";

        public static string GetAvatarUrl(this WebMessage webMessage)
        {
            webMessage.data.TryGetValue(DATA_URL_FIELD_NAME, out var avatarUrlObject);
            return (string) avatarUrlObject ?? string.Empty;
        }

        public static string GetUserId(this WebMessage webMessage)
        {
            webMessage.data.TryGetValue(ID_KEY, out var userIdObject);
            return (string) userIdObject ?? string.Empty;
        }

        public static AssetRecord GetAssetRecord(this WebMessage webMessage)
        {
            webMessage.data.TryGetValue(ASSET_ID_KEY, out var assetIdObject);
            webMessage.data.TryGetValue(USER_ID_KEY, out var userIdObject);
            var assetRecord = new AssetRecord();
            assetRecord.AssetId = (string) assetIdObject ?? string.Empty;
            assetRecord.UserId = (string) userIdObject ?? string.Empty;
            return assetRecord;
        }
    }
}
