using i5.Toolkit.Core.Utilities;
using System.Threading.Tasks;

namespace i5.Toolkit.Core.OpenIDConnectClient
{
    /// <summary>
    /// Implementation of the OpenID Connect Sketchfab Provider
    /// More information can be found here: https://sketchfab.com/developers/oauth
    /// </summary>
    public class SketchfabOidcProvider : AbstractOidcProvider
    {
        /// <summary>
        /// Creates a new instance of the Sketchfab client
        /// </summary>
        public SketchfabOidcProvider()
            : base()
        {
            serverName = "https://sketchfab.com/oauth2";
        }

        /// <summary>
        /// Gets information about the logged in user from the Sketchfab provider
        /// </summary>
        /// <param name="accessToken">The access token to authenticate the user</param>
        /// <returns>Returns information about the logged in user if the request was successful, otherwise null</returns>
        public override async Task<IUserInfo> GetUserInfoAsync(string accessToken)
        {
            WebResponse<string> webResponse = await RestConnector.GetAsync(userInfoEndpoint + "?access_token=" + accessToken);
            if (webResponse.Successful)
            {
                SketchfabUserInfo userInfo = JsonSerializer.FromJson<SketchfabUserInfo>(webResponse.Content);
                if (userInfo == null)
                {
                    i5Debug.LogError("Could not parse user info", this);
                }
                return userInfo;
            }
            else
            {
                i5Debug.LogError("Error fetching the user info: " + webResponse.ErrorMessage, this);
                return default;
            }
        }
    }
}