using System;
using Newtonsoft.Json.Linq;
using ReadyPlayerMe.Core;

namespace ReadyPlayerMe.AvatarCreator
{
    public static class ResponseExtensions
    {
        public static void ThrowIfError(this IResponse response)
        {
            if (!response.IsSuccess)
            {
                throw new Exception(response.Error);
            }
        }

        public static void ThrowIfError(this ResponseText response)
        {
            if (!response.IsSuccess)
            {
                if (!string.IsNullOrEmpty(response.Text))
                {
                    var json = JObject.Parse(response.Text);
                    throw new Exception(json["message"]!.ToString());
                }
                throw new Exception(response.Error);
            }
        }
    }
}
