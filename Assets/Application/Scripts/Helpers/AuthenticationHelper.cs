namespace MRTKUtilities.Application
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Identity.Client;
    using UnityEngine;

#if WINDOWS_UWP
    using Windows.Security.Authentication.Web;
    using Windows.Security.Authentication.Web.Core;
    using Windows.Security.Credentials;
    using Windows.Security.Credentials.UI;
#endif

    /// <summary>
    /// Authentication helper methods.
    /// </summary>
    public static class AuthenticationHelper
    {
#if WINDOWS_UWP
    private const string WebBrokerReturnUrlTemplate = "ms-appx-web://Microsoft.AAD.BrokerPlugIn/{0}";
#endif
        private const string NativeClientRedirectUri = "https://login.microsoftonline.com/common/oauth2/nativeclient";
        private const string AuthorityTemplate = "https://login.microsoftonline.com/{0}";

        /// <summary>
        /// The delegate to show the message with instructions of the device code flow.
        /// </summary>
        /// <param name="message">The message with instructions.</param>
        public delegate void MessageDelegate(string message);

        /// <summary>
        /// The event to handle by the caller to receive user instructions in device code flow.
        /// </summary>
        public static event MessageDelegate DeviceCodeMessage;

        /// <summary>
        /// The event to handle by the caller to receive debug information.
        /// </summary>
        public static event MessageDelegate DebugMessage;

        /// <summary>
        /// Return the redirect URL based on the package id of this app (UWP).
        /// This is needed in the app registration as redirect URI.
        /// For new UWP apps (or HoloLens apps) execute this function once to
        /// retrieve the URL so it can be registered in the client app registration
        /// as a return URL.
        /// </summary>
        /// <returns>The redirect URL.</returns>
#pragma warning disable CA1055 // URI-like return values should not be strings
        public static string GetRedirectUrl()
#pragma warning restore CA1055 // URI-like return values should not be strings
        {
            string url = "This can only be retrieved as app. Deploy the app to your HoloLens.";
#if WINDOWS_UWP
url = string.Format(
    CultureInfo.InvariantCulture,
    WebBrokerReturnUrlTemplate,
    WebAuthenticationBroker.GetCurrentApplicationCallbackUri().Host.ToUpper(CultureInfo.InvariantCulture));
#endif
            return url;
        }

#pragma warning disable CS1998
#pragma warning disable CA1801
        /// <summary>
        /// Authenticate silent using Windows Account Manager (WAM) with the
        /// identity in the OS.
        /// </summary>
        /// <param name="clientId">Client ID from the app registration.</param>
        /// <param name="scopes">Scopes. Multiple scopes should be separated by a ' ' (space).
        /// Mostly this is the scope of the backend API, like "api://[client id]/user_impersonation".</param>
        /// <param name="resource">Resource to authenticate against. For the web api this
        /// is something like "api://[client id]"</param>
        /// <returns>Tuple with access token and username, or (null, null) on error.</returns>
        public static async Task<(string token, string username)> AuthenticateSilentWAMAsync(
            string clientId,
            string scopes,
            string resource)
#pragma warning restore CS1998
#pragma warning restore CA1801
        {
#if WINDOWS_UWP
        WebAccountProvider wap;
        WebTokenRequest wtr = null;

        try
        {
            wap = await WebAuthenticationCoreManager
                        .FindAccountProviderAsync("https://login.microsoft.com", "organizations");

            wtr = new WebTokenRequest(wap, scopes, clientId);
            wtr.Properties.Add("resource", resource);
        }
        catch (Exception ex)
        {
            Debug.Log($"WAM.Request: {ex.Message}");
            return (null, null);
        }

        WebTokenRequestResult tokenResponse = null;

        try
        {
            tokenResponse = await WebAuthenticationCoreManager.GetTokenSilentlyAsync(wtr);
        }
        catch (Exception ex)
        {
            Debug.Log($"WAM.GetToken: {ex.Message}");
        }

        if (tokenResponse.ResponseError != null)
        {
            Debug.Log($"Error Code: {tokenResponse.ResponseError.ErrorCode}");
            Debug.Log($"Error Msg: {tokenResponse.ResponseError.ErrorMessage}");
            Debug.Log($"Error props:");
        }

        if (tokenResponse.ResponseStatus == WebTokenRequestStatus.Success)
        {
            if (tokenResponse.ResponseData.Count > 0)
            {
                WebTokenResponse resp = tokenResponse.ResponseData[0];

                string username = string.Empty;
                if (!resp.Properties.TryGetValue("DisplayName", out username))
                {
                    resp.Properties.TryGetValue("Name", out username);
                }

                return (resp.Token, username);
            }
        }

        return (null, null);
#else
            return (null, null);
#endif
        }

        /// <summary>
        /// Authenticate silent (if possible). It will use WAM under the hood
        /// and works both with app registrations for 1 org or multi org.
        /// It obtains the current user from the OS (UWP only).
        /// The AcquireTokenSilent call will first try to get the token from the MSAL
        /// owned cache. That also supports token refreshes.
        /// </summary>
        /// <param name="clientId">Client ID from the app registration.</param>
        /// <param name="scopes">Scopes. Multiple scopes should be separated by a ' ' (space).
        /// Mostly this is the scope of the backend API, like "api://[client id]/user_impersonation".</param>
        /// <param name="cancellationTokenSource">Cancellation token.</param>
        /// <returns>A <see cref="AuthenticationResult"/> object or null on error.</returns>
        public static async Task<AuthenticationResult> AuthenticateSilentAsync(
            string clientId,
            string scopes,
            CancellationTokenSource cancellationTokenSource)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            if (string.IsNullOrEmpty(scopes))
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            if (cancellationTokenSource == null)
            {
                throw new ArgumentNullException(nameof(cancellationTokenSource));
            }

            IPublicClientApplication app;
            IAccount account;

            try
            {
                // first create the auth client
                // we'll use the broker of the OS to obtain the current user.
                app = PublicClientApplicationBuilder
                    .Create(clientId)
                    .WithAuthority(AadAuthorityAudience.AzureAdMultipleOrgs)
                    .WithBroker()
                    .WithWindowsBrokerOptions(new WindowsBrokerOptions
                    {
                        ListWindowsWorkAndSchoolAccounts = true,
                    })
                    .Build();
            }
            catch (Exception ex)
            {
                Debug.Log($"AuthSilent.CreateApp.Exception: {ex.Message}");
                return null;
            }

            try
            {
                // get the logged in user account from the OS
                // this UWP only and is some placeholder understood by MSAL.
                account = PublicClientApplication.OperatingSystemAccount;
                string[] scopesList = scopes.Split(' ');

                // acquire the access token silent
                AuthenticationResult result =
                    await app.AcquireTokenSilent(scopesList, account)
                             .ExecuteAsync(cancellationTokenSource.Token);
                return result;
            }
            catch (Exception ex)
            {
                cancellationTokenSource.Cancel();
                Debug.Log($"AuthSilent.Acquire.Exception: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Authenticate with device code flow. The acquire request will return
        /// instructions to show to the user. It's essentially a message to go to
        /// a standard URL where the user has to enter the provided code. Once
        /// the user authenticates succesfully after that, the code returns with
        /// the logged in user.
        /// The caller has to register to the <see cref="DeviceCodeMessage"/> event
        /// to receive the instructions to be able to show it to the user.
        /// </summary>
        /// <param name="clientId">Client ID from the app registration.</param>
        /// <param name="scopes">Scopes. Multiple scopes should be separated by a ' ' (space).
        /// Mostly this is the scope of the backend API, like "api://[client id]/user_impersonation".</param>
        /// <param name="tenantId">Tenant ID to authenticate against.</param>
        /// <param name="cancellationTokenSource">Cancellation token.</param>
        /// <returns>A <see cref="AuthenticationResult"/> object or null on error.</returns>
        public static async Task<AuthenticationResult> AuthenticateWithDeviceCodeAsync(
            string clientId,
            string scopes,
            string tenantId,
            CancellationTokenSource cancellationTokenSource)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            if (string.IsNullOrEmpty(scopes))
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ArgumentNullException(nameof(tenantId));
            }

            if (cancellationTokenSource == null)
            {
                throw new ArgumentNullException(nameof(cancellationTokenSource));
            }

            IPublicClientApplication app;

            try
            {
                // authority url is a defined start with addition of the tenant
                // which can be 'common', 'organizations' or a tenant Id.
                string authority = string.Format(CultureInfo.InvariantCulture, AuthorityTemplate, tenantId);

#pragma warning disable CA2234 // Pass system uri objects instead of strings
                // create the auth client
                // we'll be using a fixed redirect Uri that must be enabled in the
                // client app registration.
                app = PublicClientApplicationBuilder
                    .Create(clientId)
                    .WithRedirectUri(NativeClientRedirectUri)
                    .WithAuthority(authority)
                    .Build();
#pragma warning restore CA2234 // Pass system uri objects instead of strings
            }
            catch (Exception ex)
            {
                Debug.LogError($"AuthDevCode.CreateApp.Exception: {ex.Message}");
                return null;
            }

            try
            {
                string[] scopesList = scopes.Split(' ');

                // Start the device code flow. This returns a message
                // that needs to be handled by the user. Once authenticated in a browser
                // on any device, the code will return here with an access token
                // if successful.
                AuthenticationResult result = await app.AcquireTokenWithDeviceCode(scopesList, flow =>
                {
                    DeviceCodeMessage?.Invoke(flow.Message);
                    return Task.CompletedTask;
                }).ExecuteAsync(cancellationTokenSource.Token);
                return result;
            }
            catch (Exception ex)
            {
                cancellationTokenSource.Cancel();
                Debug.Log($"AuthDevCode.Acquire.Exception: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Authenticate interactive. The user will be prompted to enter the
        /// account credentials.
        /// NOTE: Do NOT use this on the HoloLens! Only on desktop.
        /// </summary>
        /// <param name="clientId">Client ID from the app registration.</param>
        /// <param name="scopes">Scopes. Multiple scopes should be separated by a ' ' (space).
        /// Mostly this is the scope of the backend API, like "api://[client id]/user_impersonation".</param>
        /// <param name="cancellationTokenSource">Cancellation token.</param>
        /// <returns>A <see cref="AuthenticationResult"/> object or null on error.</returns>
        public static async Task<AuthenticationResult> AuthenticateInteractiveAsync(
            string clientId,
            string scopes,
            CancellationTokenSource cancellationTokenSource)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            if (string.IsNullOrEmpty(scopes))
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            if (cancellationTokenSource == null)
            {
                throw new ArgumentNullException(nameof(cancellationTokenSource));
            }

            IPublicClientApplication app;
            IAccount account;

            try
            {
                // create the auth client
                // using the broker of the OS to get the current user.
                app = PublicClientApplicationBuilder
                    .Create(clientId)
                    .WithAuthority(AadAuthorityAudience.AzureAdMultipleOrgs)
                    .WithBroker()
                    .WithWindowsBrokerOptions(new WindowsBrokerOptions
                    {
                        ListWindowsWorkAndSchoolAccounts = true,
                    })
                    .Build();
            }
            catch (Exception ex)
            {
                Debug.LogError($"AuthInteractive.CreateApp.Exception: {ex.Message}");
                return null;
            }

            try
            {
                // get the logged in user from the OS.
                // This is UWP only (and HoloLens app) and is some placeholder
                // that's understood by MSAL internals.
                account = PublicClientApplication.OperatingSystemAccount;

                // Start the interactive login. This will
                // popup UI to provide a user and password.
                string[] scopesList = scopes.Split(' ');
                AuthenticationResult result = await app
                    .AcquireTokenInteractive(scopesList)
                    .WithAccount(account)
                    .WithPrompt(Prompt.SelectAccount)
                    .ExecuteAsync(cancellationTokenSource.Token);
                return result;
            }
            catch (Exception ex)
            {
                cancellationTokenSource.Cancel();
                Debug.LogError($"AuthInteractive.Acquire.Exception: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Logout removes all cached accounts.
        /// </summary>
        /// <param name="clientId">Client ID from the app registration.</param>
        /// <param name="tenantId">Tenant ID to authenticate against.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task LogoutAllAsync(
            string clientId,
            string tenantId)
        {
            IPublicClientApplication app;

            try
            {
                // authority is fixed URL with tenant added
                // tenant can be 'common', 'organizations' or a tenant Id.
                string authority = string.Format(CultureInfo.InvariantCulture, AuthorityTemplate, tenantId);
#pragma warning disable CA2234 // Pass system uri objects instead of strings

                // create the auth client
                app = PublicClientApplicationBuilder
                    .Create(clientId)
                    .WithRedirectUri(NativeClientRedirectUri)
                    .WithAuthority(authority)
                    .Build();
#pragma warning restore CA2234 // Pass system uri objects instead of strings
            }
            catch (Exception ex)
            {
                Debug.LogError($"Logout.CreateApp.Exception: {ex.Message}");
                return;
            }

            try
            {
                // get all cached accounts and remove them
                List<IAccount> accounts = (await app.GetAccountsAsync()).ToList();
                while (accounts.Count > 0)
                {
                    await app.RemoveAsync(accounts[0]);
                    accounts = (await app.GetAccountsAsync()).ToList();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Logout.Remove.Exception: {e.Message}");
            }
        }

        /// <summary>
        /// Validate current user with Windows Hello. It will popup the Hello interface
        /// as configured (pin, face, finger, iris) for the logged in user. When
        /// properly validated, we return true.
        /// If Windows Hello is not available, we'll return false.
        /// </summary>
        /// <param name="message">Message to show in the login window.</param>
        /// <returns>Validated true/false.</returns>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
#pragma warning disable CA1801 // Review unused parameters
        public static async Task<bool> ValidateUserWithHelloAsync(string message = "Please verify your credentials")
#pragma warning restore CA1801 // Review unused parameters
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
#if WINDOWS_UWP
        Debug.Log($"Check if Hello is available.");
        if (await UserConsentVerifier.CheckAvailabilityAsync() == UserConsentVerifierAvailability.Available)
        {
            Debug.Log($"Verify user.");
            UserConsentVerificationResult consentResult = await UserConsentVerifier.RequestVerificationAsync(message);
            Debug.Log($"Result: {consentResult}");
            return consentResult == UserConsentVerificationResult.Verified;
        }
        else
        {
            Debug.LogError($"We don't have Hello access.");
            return false;
        }
#else
            Debug.Log($"Standard: no validation.");
            return false;
#endif
        }
    }
}