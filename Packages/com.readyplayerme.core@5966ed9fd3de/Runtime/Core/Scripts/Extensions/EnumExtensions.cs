using System;
using System.ComponentModel;
using System.Reflection;

namespace ReadyPlayerMe.Core
{
    public static class EnumExtensions
    {
        public static T GetValueFromDescription<T>(string description) where T : Enum
        {
            foreach (var field in typeof(T).GetFields())
            {
                if (Attribute.GetCustomAttribute(field,
                        typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description == description)
                        return (T) field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T) field.GetValue(null);
                }
            }

            return default;
        }

        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());

            var attribute = field.GetCustomAttribute<DescriptionAttribute>();

            return attribute == null ? value.ToString() : attribute.Description;
        }

    }
}
