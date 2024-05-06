using UnityEngine;

namespace MirageXR.AIManagerDataModel
{
    public class Token
    {
        public string BackendToken { get; }
        public Token(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                UnityEngine.Debug.LogError("Text cannot be null or empty");
            }
            BackendToken = text;
        }
    }
}