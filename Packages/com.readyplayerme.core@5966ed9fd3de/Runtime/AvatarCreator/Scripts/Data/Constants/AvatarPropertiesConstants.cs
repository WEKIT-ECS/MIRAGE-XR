using System.Collections.Generic;

namespace ReadyPlayerMe.AvatarCreator
{
    public static class AvatarPropertiesConstants
    {
        public static readonly Dictionary<AssetType, object> MaleDefaultAssets =
            new Dictionary<AssetType, object>
            {
                { AssetType.SkinColor, 5 },
                { AssetType.EyeColor, "9781796" },
                { AssetType.HairStyle, "9247476" },
                { AssetType.EyebrowStyle, "16858292" },
                { AssetType.Outfit, "109373713" },
                { AssetType.HairColor, 0 },
                { AssetType.EyebrowColor, 0 },
                { AssetType.BeardColor, 0 }
            };

        public static readonly Dictionary<AssetType, object> FemaleDefaultAssets =
            new Dictionary<AssetType, object>
            {
                { AssetType.SkinColor, 5 },
                { AssetType.EyeColor, "9781796" },
                { AssetType.HairStyle, "9247476" },
                { AssetType.EyebrowStyle, "16858292" },
                { AssetType.Outfit, "109376347" },
                { AssetType.HairColor, 0 },
                { AssetType.EyebrowColor, 0 },
                { AssetType.BeardColor, 0 }
            };
    }
}
