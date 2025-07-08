using System;
using Newtonsoft.Json;
using ReadyPlayerMe.Core;
using UnityEngine;

namespace ReadyPlayerMe.AvatarCreator
{
    [Serializable]
    public class AvatarTemplateData : IAssetData
    {
        public string Id { get; set; }
        public AssetType AssetType { get; set; } = AssetType.AvatarTemplate;
        public string ImageUrl;
        [JsonConverter(typeof(GenderConverter))]
        public OutfitGender Gender;
        public Texture Texture;
        public string UsageType;
    }
}
