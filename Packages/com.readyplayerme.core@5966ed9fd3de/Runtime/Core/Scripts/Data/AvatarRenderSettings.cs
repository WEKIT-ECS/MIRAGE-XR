using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace ReadyPlayerMe.Core
{
    [System.Serializable]
    public struct BlendShape
    {
        public string Name;
        public float Value;

        public BlendShape(string name, float value)
        {
            Name = name;
            Value = value;
        }
    }

    /// <summary>
    /// This structure holds all the data required a request to the Avatar Render API.
    /// </summary>
    [System.Serializable]
    public class AvatarRenderSettings
    {
        public Expression Expression = Expression.None;
        public RenderPose Pose = RenderPose.None;
        public RenderCamera Camera = RenderCamera.Portrait;
        public int Quality = 100;
        public int Size = 800;
        public Color Background = Color.white;
        public bool IsTransparent;
        public List<BlendShape> BlendShapes;

        public string GetParametersAsString()
        {
            var queryBuilder = new QueryBuilder();
            if (Expression != Expression.None)
            {
                queryBuilder.AddKeyValue(nameof(Core.Expression).ToCamelCase(), Expression.ToString().ToCamelCase());
            }
            if (Pose != RenderPose.None)
            {
                queryBuilder.AddKeyValue(nameof(Pose).ToCamelCase(), RenderSettingsHelper.RenderPoseMap[Pose]);
            }

            if (BlendShapes != null)
            {
                foreach (var blendShape in BlendShapes)
                {
                    var key = $"{AvatarAPIParameters.RENDER_BLEND_SHAPES}[{blendShape.Name}]";
                    var value = blendShape.Value.ToString(CultureInfo.InvariantCulture);
                    queryBuilder.AddKeyValue(key, value);
                }
            }

            queryBuilder.AddKeyValue(nameof(Camera).ToCamelCase(), Camera.ToString().ToLower());

            if (Size == 0)
            {
                Size = 800;
            }

            queryBuilder.AddKeyValue(nameof(Size).ToCamelCase(), Size.ToString());

            if (!IsTransparent)
            {
                queryBuilder.AddKeyValue(nameof(Background).ToCamelCase(), RenderSettingsHelper.FloatToRGBString(Background));
            }

            return queryBuilder.Query;
        }

    }
}
