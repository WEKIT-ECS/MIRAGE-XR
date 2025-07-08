using UnityEngine;

namespace ReadyPlayerMe.Core.Data
{
    public class CoreSettings : ScriptableObject
    {
        public const string DEFAULT_SUBDOMAIN = "demo";

        public string Subdomain = DEFAULT_SUBDOMAIN;
        public string AppId = string.Empty;
        public bool EnableLogging;
        public bool EnableAnalytics;
        public BodyType BodyType = BodyType.FullBody;
    }
}
