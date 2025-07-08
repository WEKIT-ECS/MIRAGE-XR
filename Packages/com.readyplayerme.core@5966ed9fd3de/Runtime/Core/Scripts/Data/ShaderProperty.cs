using System;
using UnityEngine;

namespace ReadyPlayerMe.Core
{
    [Serializable]
    public class ShaderProperty
    {
        public TextureChannel TextureChannel;
        public string PropertyName;

        public ShaderProperty(TextureChannel textureChannel, string propertyName)
        {
            TextureChannel = textureChannel;
            PropertyName = propertyName;
        }
    }

    [Serializable]
    public class ShaderPropertyMapping
    {
        public string SourceProperty;
        public string TargetProperty;
        public ShaderPropertyType Type;

        public ShaderPropertyMapping(string sourceProperty, string targetProperty, ShaderPropertyType type)
        {
            SourceProperty = sourceProperty;
            TargetProperty = targetProperty;
            Type = type;
        }
    }

    public enum ShaderPropertyType
    {
        Texture,
        Float,
        Color,
        Vector
    }

    public static class ShaderMaterialOverrideHelper
    {
        public static void ApplyOverrides(Material sourceMaterial, Material targetMaterial, ShaderPropertyMapping[] propertyMappings)
        {
            if (sourceMaterial == null || targetMaterial == null)
            {
                Debug.LogError("Source or Target Material is not set.");
                return;
            }

            foreach (var mapping in propertyMappings)
            {
                if (!sourceMaterial.HasProperty(mapping.SourceProperty) || !targetMaterial.HasProperty(mapping.TargetProperty))
                {
                    if (!string.IsNullOrEmpty(mapping.TargetProperty))
                    {
                        Debug.LogWarning($"Property not found in source or target material. SourceProperty:{mapping.SourceProperty} TargetProperty:{mapping.TargetProperty}");
                    }
                    continue;
                }

                switch (mapping.Type)
                {
                    case ShaderPropertyType.Texture:
                        var texture = sourceMaterial.GetTexture(mapping.SourceProperty);
                        targetMaterial.SetTexture(mapping.TargetProperty, texture);
                        break;

                    case ShaderPropertyType.Float:
                        var floatValue = sourceMaterial.GetFloat(mapping.SourceProperty);
                        targetMaterial.SetFloat(mapping.TargetProperty, floatValue);
                        break;

                    case ShaderPropertyType.Color:
                        var colorValue = sourceMaterial.GetColor(mapping.SourceProperty);
                        targetMaterial.SetColor(mapping.TargetProperty, colorValue);
                        break;

                    case ShaderPropertyType.Vector:
                        var vectorValue = sourceMaterial.GetVector(mapping.SourceProperty);
                        targetMaterial.SetVector(mapping.TargetProperty, vectorValue);
                        break;
                    default:
                        Debug.LogWarning("Unknown property type.");
                        break;
                }
            }
        }
    }
}
