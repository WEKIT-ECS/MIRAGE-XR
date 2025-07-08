using System.Collections.Generic;
using UnityEngine;

namespace ReadyPlayerMe.Core
{
    public static class RenderSettingsHelper
    {
        public static string ToCamelCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            var letters = str.ToCharArray();
            letters[0] = char.ToLower(letters[0]);
            return new string(letters);
        }
        
        public static Dictionary<RenderPose, string> RenderPoseMap = new Dictionary<RenderPose, string>()
        {
            { RenderPose.PowerStance, "power-stance" },
            { RenderPose.Relaxed, "relaxed" },
            { RenderPose.Standing, "standing" },
            { RenderPose.ThumbsUp, "thumbs-up" }
        };
        
        public static string FloatToRGBString(Color color)
        {
            return $"{(int) (color.r * 255)},{(int) (color.g * 255)},{(int) (color.b * 255)}";
        }
    }
}
