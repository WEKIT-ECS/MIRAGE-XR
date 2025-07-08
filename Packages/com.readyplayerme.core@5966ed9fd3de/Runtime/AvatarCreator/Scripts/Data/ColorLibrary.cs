using System;

namespace ReadyPlayerMe.AvatarCreator
{
    [Serializable]
    public struct ColorLibrary
    {
        public AssetColor[] Skin;
        public AssetColor[] Eyebrow;
        public AssetColor[] Beard;
        public AssetColor[] Hair;
    }

    [Serializable]
    public struct AssetColor : IAssetData
    {
        public string Id { get; set; }
        public AssetType AssetType { get; set; }
        public string HexColor;

        public AssetColor(string id, AssetType assetType, string hexColor)
        {
            Id = id;
            AssetType = assetType;
            HexColor = hexColor;
        }
    }
}
