using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace ReadyPlayerMe.AvatarCreator
{
    public static class ColorResponseHandler
    {
        [Serializable]
        public struct ColorResponse
        {
            public string[] skin;
            public string[] eyebrow;
            public string[] beard;
            public string[] hair;
        }

        public static AssetColor[] GetColorsFromResponse(string response)
        {
            var responseData = JObject.Parse(response);
            ColorResponse colorResponse = ((JObject) responseData["data"])!.ToObject<ColorResponse>();
            return ResponseToAssetColors(colorResponse);
        }

        private static AssetColor[] ResponseToAssetColors(ColorResponse colorResponse)
        {
            var skinColors = colorResponse.skin != null ? ConvertToAssetColors(colorResponse.skin, AssetType.SkinColor) : Array.Empty<AssetColor>();
            var eyebrowColors = colorResponse.eyebrow != null ? ConvertToAssetColors(colorResponse.eyebrow, AssetType.EyebrowColor) : Array.Empty<AssetColor>();
            var beardColors = colorResponse.beard != null ? ConvertToAssetColors(colorResponse.beard, AssetType.BeardColor) : Array.Empty<AssetColor>();
            var hairColors = colorResponse.hair != null ? ConvertToAssetColors(colorResponse.hair, AssetType.HairColor) : Array.Empty<AssetColor>();

            return skinColors.Concat(eyebrowColors).Concat(beardColors).Concat(hairColors).ToArray();
        }

        private static AssetColor[] ConvertToAssetColors(IReadOnlyCollection<string> hexColors, AssetType assetType)
        {
            if (hexColors == null || hexColors.Count == 0)
            {
                return Array.Empty<AssetColor>();
            }

            return hexColors.Select((color, index) => new AssetColor(index.ToString(), assetType, color)).ToArray();
        }
    }
}
