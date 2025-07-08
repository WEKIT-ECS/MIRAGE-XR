using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReadyPlayerMe.Core;

namespace ReadyPlayerMe.AvatarCreator
{
    public struct RefreshTokenResponse
    {
        public string Token { get; private set; }
        public string RefreshToken { get; private set; }

        public RefreshTokenResponse(JToken data)
        {
            Token = data[AuthConstants.TOKEN]!.ToString();
            RefreshToken = data[AuthConstants.REFRESH_TOKEN]!.ToString();
        }
    }

    public class AuthAPIRequests
    {
        private readonly string domain;
        private readonly IDictionary<string, string> headers = CommonHeaders.GetHeadersWithAppId();
        private readonly string rpmAuthBaseUrl;
        private readonly string appName;

        private readonly WebRequestDispatcher webRequestDispatcher;

        public AuthAPIRequests(string domain)
        {
            this.domain = domain;
            webRequestDispatcher = new WebRequestDispatcher();

            rpmAuthBaseUrl = Env.RPM_API_V1_BASE_URL;
            appName = CoreSettingsHandler.CoreSettings.Subdomain;
        }

        public async Task<UserSession> LoginAsAnonymous(CancellationToken cancellationToken = default)
        {
            var payload = JsonConvert.SerializeObject(new {
                data = new {
                    appName = appName,
                    requestToken = true
                }
            });

            var response = await webRequestDispatcher.SendRequest<ResponseText>($"{rpmAuthBaseUrl}users", HttpMethod.POST, headers, payload, ctx:cancellationToken);
            response.ThrowIfError();

            var data = AuthDataConverter.ParseDataResponse(response.Text);
            var createdUser = JsonConvert.DeserializeObject<CreatedUser>(data!.ToString());

            return new UserSession(createdUser);
        }

        public async Task SendCodeToEmail(string email, string userId = "",CancellationToken cancellationToken = default)
        {
            var data = new Dictionary<string, string>
            {
                { AuthConstants.EMAIL, email },
            };

            if (!string.IsNullOrEmpty(userId))
            {
                data.Add(AuthConstants.USER_ID, userId);
            }

            var payload = AuthDataConverter.CreatePayload(data);

            var response = await webRequestDispatcher.SendRequest<ResponseText>($"{rpmAuthBaseUrl}auth/request-login-code", HttpMethod.POST, headers, payload, ctx:cancellationToken);
            response.ThrowIfError();
        }

        public async Task<UserSession> LoginWithCode(string code, string userIdToMerge = null, CancellationToken cancellationToken = default)
        {
            var body = new Dictionary<string, string>
            {
                { AuthConstants.CODE, code },
                { AuthConstants.APP_NAME, appName  }
            };
            if (userIdToMerge != null)
            {
                body.Add(AuthConstants.USER_ID, userIdToMerge);
            }
            var payload = AuthDataConverter.CreatePayload(body);

            var response = await webRequestDispatcher.SendRequest<ResponseText>($"{rpmAuthBaseUrl}auth/login", HttpMethod.POST, headers, payload, ctx:cancellationToken);
            response.ThrowIfError();

            var data = AuthDataConverter.ParseResponse(response.Text);
            return JsonConvert.DeserializeObject<UserSession>(data!.ToString());
        }

        public async Task Signup(string email, string userId, CancellationToken cancellationToken = default)
        {
            var data = new Dictionary<string, string>
            {
                { AuthConstants.EMAIL, email },
                { AuthConstants.USER_ID, userId }
            };

            var payload = AuthDataConverter.CreatePayload(data);
            var response = await webRequestDispatcher.SendRequest<ResponseText>($"{rpmAuthBaseUrl}auth/signup/passwordless", HttpMethod.POST, headers, payload, ctx:cancellationToken);
            response.ThrowIfError();
        }

        [Obsolete("This method is deprecated. Use GetRefreshToken instead.")]
        public async Task<(string, string)> RefreshToken(string token, string refreshToken, CancellationToken cancellationToken = default)
        {
            var data = await RefreshRequest(token, refreshToken, cancellationToken);
            var newToken = data[AuthConstants.TOKEN]!.ToString();
            var newRefreshToken = data[AuthConstants.REFRESH_TOKEN]!.ToString();
            return (newToken, newRefreshToken);
        }

        public async Task<RefreshTokenResponse> GetRefreshToken(string token, string refreshToken, CancellationToken cancellationToken = default)
        {
            var data = await RefreshRequest(token, refreshToken, cancellationToken);
            return new RefreshTokenResponse(data);
        }

        private async Task<JToken> RefreshRequest(string token, string refreshToken, CancellationToken cancellationToken)
        {
            var payload = AuthDataConverter.CreatePayload(new Dictionary<string, string>
            {
                { AuthConstants.TOKEN, token },
                { AuthConstants.REFRESH_TOKEN, refreshToken }
            });

            var response = await webRequestDispatcher.SendRequest<ResponseText>($"{rpmAuthBaseUrl}auth/refresh", HttpMethod.POST, headers, payload, ctx:cancellationToken);
            response.ThrowIfError();

            return AuthDataConverter.ParseDataResponse(response.Text);
        }
    }
}
