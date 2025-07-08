using System.Collections.Generic;
using System.Linq;
using ReadyPlayerMe.Core;

namespace ReadyPlayerMe.AvatarCreator
{
    public static class CategoryHelper
    {
        public static IEnumerable<AssetType> GetCategories(BodyType bodyType)
        {
            return AssetTypeByValue
                .Select(a => a.Value)
                .Where(assetType => assetType.IsCompatibleAssetType(bodyType))
                .ToList();
        }

        public static readonly Dictionary<string, AssetType> AssetTypeByValue = new Dictionary<string, AssetType>
        {
            { "faceshape", AssetType.FaceShape },
            { "eyeshape", AssetType.EyeShape },
            { "eye", AssetType.EyeColor },
            { "eyebrows", AssetType.EyebrowStyle },
            { "noseshape", AssetType.NoseShape },
            { "lipshape", AssetType.LipShape },
            { "beard", AssetType.BeardStyle },
            { "hair", AssetType.HairStyle },
            { "outfit", AssetType.Outfit },
            { "shirt", AssetType.Shirt },
            { "glasses", AssetType.Glasses },
            { "facemask", AssetType.FaceMask },
            { "facewear", AssetType.Facewear },
            { "headwear", AssetType.Headwear },
            { "hairColor", AssetType.HairColor },
            { "eyebrowColor", AssetType.EyebrowColor },
            { "beardColor", AssetType.BeardColor },
            { "bottom", AssetType.Bottom },
            { "top", AssetType.Top },
            { "footwear", AssetType.Footwear },
            { "costume", AssetType.Costume }
        };
    }
}
