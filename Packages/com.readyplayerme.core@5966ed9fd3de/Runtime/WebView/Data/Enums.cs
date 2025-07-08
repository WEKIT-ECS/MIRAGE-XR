using System;
using System.Reflection;

namespace ReadyPlayerMe.Core.WebView
{
    /// <summary>
    /// Defines all the options for the language translation of the Ready Player Me website.
    /// </summary>
    public enum Language
    {
        [StringValue("")] Default,
        [StringValue("ch")] Chinese,
        [StringValue("de")] German,
        [StringValue("en-IE")] EnglishIreland,
        [StringValue("en")] English,
        [StringValue("es-MX")] SpanishMexico,
        [StringValue("es")] Spanish,
        [StringValue("fr")] French,
        [StringValue("it")] Italian,
        [StringValue("jp")] Japanese,
        [StringValue("kr")] Korean,
        [StringValue("pt-BR")] PortugueseBrazil,
        [StringValue("pt")] Portuguese,
        [StringValue("tr")] Turkish
    }

    /// <summary>
    /// Defines the options for the avatar body type.
    /// </summary>
    public enum BodyTypeOption
    {
        Selectable,
        [StringValue("fullbody")] FullBody,
        [StringValue("halfbody")] HalfBody
    }

    /// <summary>
    /// Defines the options used for the avatars gender.
    /// </summary>
    public enum Gender
    {
        None,
        [StringValue("male")] Male,
        [StringValue("female")] Female
    }

    /// <summary>
    /// Defines the options used for the WebView and Message panel UI.
    /// </summary>
    public enum MessageType
    {
        [StringValue("Loading...")]
        Loading,
        [StringValue("Network is not reachable.")]
        NetworkError,
        [StringValue("WebView is only supported on Android and iOS.\nBuild and run on a mobile device.")]
        NotSupported
    }

    // Attribute for storing a string value in the enum field
    public class StringValueAttribute : Attribute
    {

        public StringValueAttribute(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }

    // Extension methods and helpers for enums
    public static class EnumHelpers
    {
        // Helps extracting the string value stored in the StringValue attribute of the enum field
        public static string GetValue<T>(this T enumerationValue) where T : struct
        {
            Type type = enumerationValue.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException("EnumerationValue must be of Enum type", nameof(enumerationValue));
            }

            MemberInfo[] memberInfo = type.GetMember(enumerationValue.ToString());
            if (memberInfo.Length > 0)
            {
                var attrs = memberInfo[0].GetCustomAttributes(typeof(StringValueAttribute), false);

                if (attrs.Length > 0)
                {
                    return ((StringValueAttribute) attrs[0]).Value;
                }
            }

            return enumerationValue.ToString();
        }
    }
}
