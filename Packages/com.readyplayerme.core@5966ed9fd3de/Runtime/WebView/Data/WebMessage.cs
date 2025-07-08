using System.Collections.Generic;

namespace ReadyPlayerMe.Core.WebView
{
    public struct WebMessage
    {
        public string type;
        public string source;
        public string eventName;
        public Dictionary<string, object> data;
    }
}
