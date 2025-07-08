using Newtonsoft.Json;
using ReadyPlayerMe.Core;

namespace ReadyPlayerMe.AvatarCreator
{
    public struct PartnerAsset : IAssetData
    {
        public string Id { get; set; }

        [JsonProperty("type"), JsonConverter(typeof(CategoryConverter))]
        public AssetType AssetType { get; set; }

        [JsonConverter(typeof(GenderConverter))]
        public OutfitGender Gender;
        [JsonProperty("iconUrl")]
        public string ImageUrl;
        [JsonProperty("lockedCategories")]
        public string[] LockedCategories;
        [JsonProperty("locked")]
        public bool Locked;
    }
}
