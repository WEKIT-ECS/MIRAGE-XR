using System.Collections.Generic;
using LearningExperienceEngine;
using UnityEngine.Networking;

namespace MirageXR
{
    /// <summary>
    /// Builds the authorization header for requests to the backend.
    /// </summary>
    public static class AuthorizationHeaders
    {
        private const string AuthorizationHeaderName = "Authorization";

        private static readonly IReadOnlyDictionary<string, string> NoHeaders = new Dictionary<string, string>();

        /// <summary>Returns the bearer token header, or no headers if the user is not logged in.</summary>
        public static IReadOnlyDictionary<string, string> Create(IAuthorizationManager authorizationManager)
        {
            var accessToken = authorizationManager?.AccessToken;
            if (string.IsNullOrEmpty(accessToken))
            {
                return NoHeaders;
            }

            return new Dictionary<string, string> { [AuthorizationHeaderName] = $"Bearer {accessToken}" };
        }

        public static void Apply(UnityWebRequest webRequest, IAuthorizationManager authorizationManager)
        {
            foreach (var header in Create(authorizationManager))
            {
                webRequest.SetRequestHeader(header.Key, header.Value);
            }
        }
    }
}
