namespace MRTKUtilities.Application
{
    using Microsoft.Identity.Client;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// This is the manager for authentication. It can be used to logon
    /// the user and to retrieve an access token for API access.
    /// </summary>
    [RequireComponent(typeof(MRTKUtilities.Application.SettingsManager))]
    public class AuthenticationManager : MonoBehaviour
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="ApplicationManager"/>. The rest
        /// of the code in the application can just use AuthenticationManager.Instance.
        /// </summary>
        public static AuthenticationManager Instance { get; private set; }

        /// <summary>
        /// Gets the username. This is set once the auth is succesfull.
        /// </summary>
        private string _username;
        public string Username => _username;

        /// <summary>
        /// Gets the object id of the user. Can be used for unique identification.
        /// </summary>
        private string _objectId;
        public string ObjectId => _objectId;

#if !WINDOWS_UWP
        /// <summary>
        /// This is a local cache of the token in case of using Device Code Flow.
        /// That mechanism can be used to login to another account then the standard
        /// account of the OS. The MSAL cache cannot handle that and it is for
        /// development use only anyway.
        /// </summary>
        private string accessToken = string.Empty;
#endif

        private SettingsManager _settingsManager;

        /// <summary>
        /// Get the access token for authorization use. When the <see cref="accessToken"/>
        /// is set we'll return that (local cache), otherwise we'll get the token
        /// from the MSAL cache, which also handles token refresh in the cache.
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetAccessTokenAsync()
        {
#if !WINDOWS_UWP
            if (!string.IsNullOrEmpty(accessToken))
            {
                return accessToken;
            }
#endif

            if (_settingsManager == null)
            {
                _settingsManager = GetComponent<SettingsManager>();
            }

            // get the token silently, probably from the cache
            using CancellationTokenSource cancellationToken = new CancellationTokenSource();
            AuthenticationResult result = await AuthenticationHelper.AuthenticateSilentAsync(
                _settingsManager.Settings.ClientId, _settingsManager.Settings.Scopes, cancellationToken);

            if (result == null)
            {
                Debug.Log($"Couldn't retrieve access token from cache. Start authentication.");
                // retrieval from cache failed, so authenticate (again)
                string token = await AuthenticateUserAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    return token;
                }
            }
            else
            {
                return result.AccessToken;
            }

            Debug.LogError($"Auth ERROR: Couldn't retrieve access token from cache.");
            return null;
        }

        /// <summary>
        /// Get the current user (in the app) or trigger the device code flow (in the editor)
        /// and get an access token for the backend Web API.
        /// In the editor using device code flow, we open up a browser for the devicelogin url
        /// and copy the code into the clipboard, so you can just paste it and log in.
        /// </summary>
        /// <returns>The access token.</returns>
        public async Task<string> AuthenticateUserAsync()
        {
            AuthenticationResult result = null;

            if (_settingsManager == null)
            {
                _settingsManager = GetComponent<SettingsManager>();
            }

#if WINDOWS_UWP
        // get token for current user silently (ON HOLOLENS)
        using CancellationTokenSource cancellationtoken = new CancellationTokenSource();
        result = await AuthenticationHelper.AuthenticateSilentAsync(
                           _settingsManager.Settings.ClientId, 
                           _settingsManager.Settings.Scopes, 
                           cancellationtoken);
#else
            // handle the device code flow message (ONLY IN UNITY EDITOR)
            AuthenticationHelper.DeviceCodeMessage += message =>
            {
            // get the code from the message
            int start = message.IndexOf("the code") + 9;
                int end = message.IndexOf("to auth");
                string code = message.Substring(start, end - start - 1);

            // this is code to make it easier for the Unity dev
            // browser is opened and code is in the clipboard.
            UnityDispatcher.InvokeOnAppThread(() =>
                {
                // put it in the users clipboard
                GUIUtility.systemCopyBuffer = code;
                // open up the default browser for the device code login
                Application.OpenURL("https://microsoft.com/devicelogin");
                });

                Debug.Log(message);
            };

            // get the user using device code flow
            // message with link and code will appear in debug window.
            using CancellationTokenSource cancellationToken = new CancellationTokenSource();
            result = await AuthenticationHelper.AuthenticateWithDeviceCodeAsync(
                                _settingsManager.Settings.ClientId,
                                _settingsManager.Settings.Scopes,
                                _settingsManager.Settings.TenantId,
                                cancellationToken);

            if (result != null)
            {
                // Special for device code flow: cache access token manual
                // only used for dev scenario in Unity player.
                // user can be in another tenant, so token cache will not work.
                accessToken = result.AccessToken;
            }
#endif
            if (result != null)
            {
                // now get some user details from the acquired token
                _objectId = result.Account.HomeAccountId.ObjectId;
                if (string.IsNullOrEmpty(_objectId))
                {
                    _objectId = result.Account.Username;
                }
                Debug.Log($"Object ID: {_objectId.Substring(0, 4)}***");

                // we'll try to get a real name from the token
                System.Security.Claims.Claim claim = result.ClaimsPrincipal.FindFirst("name");
                if (claim != null)
                {
                    _username = claim.Value;
                }
                else
                {
                    // otherwise we'll use the default username (probably the e-mail address)
                    _username = result.Account.Username;
                }

                Debug.Log($"Username: {Username}");

                return result.AccessToken;
            }

            return string.Empty;
        }
    }
}