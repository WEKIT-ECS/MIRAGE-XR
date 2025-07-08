using System.Collections.Generic;
using ReadyPlayerMe.Core;

namespace ReadyPlayerMe.AvatarCreator
{
    public static class AssetTypeExtensions
    {
        private static readonly Dictionary<AssetType, string> ColorPropertyByAssetType = new Dictionary<AssetType, string>
        {
            { AssetType.SkinColor, "skin" },
            { AssetType.BeardColor, "beard" },
            { AssetType.EyebrowColor, "eyebrow" },
            { AssetType.HairColor, "hair" }
        };

        public static string GetColorProperty(this AssetType assetType)
        {
            return ColorPropertyByAssetType.TryGetValue(assetType, out var property) ? property : string.Empty;
        }
        
        public static bool IsOutfitAsset(this AssetType assetType)
        {
            switch (assetType)
            {
                case AssetType.Outfit:
                case AssetType.Shirt:
                case AssetType.Bottom:
                case AssetType.Top:
                case AssetType.Footwear:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsFaceAsset(this AssetType assetType)
        {
            switch (assetType)
            {
                case AssetType.FaceShape:
                case AssetType.EyeShape:
                case AssetType.EyeColor:
                case AssetType.EyebrowStyle:
                case AssetType.NoseShape:
                case AssetType.LipShape:
                case AssetType.BeardStyle:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsCompatibleAssetType(this AssetType assetType, BodyType bodyType)
        {
            // Filter asset type based on body type.
            if (bodyType == BodyType.FullBody)
            {
                return assetType != AssetType.Shirt;
            }
            return assetType != AssetType.Outfit;
        }

        public static bool IsOptionalAsset(this AssetType assetType)
        {
            switch (assetType)
            {
                case AssetType.Top:
                case AssetType.Bottom:
                case AssetType.Footwear:
                case AssetType.Outfit:
                case AssetType.Shirt:
                case AssetType.EyebrowStyle:
                case AssetType.Costume:
                    return false;
                default:
                    return !assetType.IsColorAsset();
            }
        }

        public static bool IsColorAsset(this AssetType assetType)
        {
            switch (assetType)
            {
                case AssetType.EyeColor:
                case AssetType.BeardColor:
                case AssetType.EyebrowColor:
                case AssetType.HairColor:
                case AssetType.SkinColor:
                    return true;
                default:
                    return false;
            }
        }
    }
}
