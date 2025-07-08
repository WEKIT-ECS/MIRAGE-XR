using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReadyPlayerMe.AvatarCreator.Responses;
using ReadyPlayerMe.Core;

namespace ReadyPlayerMe.AvatarCreator
{
    public class AvatarAPIRequests
    {
        private const string INVALID_AVATAR_ID_ERROR_MESSAGE = "Avatar ID is null or empty. Please provide a valid avatar ID.";

        private const string RPM_AVATAR_V1_BASE_URL = Env.RPM_API_V1_BASE_URL + "avatars";
        private const string RPM_AVATAR_V2_BASE_URL = Env.RPM_API_V2_BASE_URL + "avatars";

        private const string COLOR_PARAMETERS = "colors?type=skin,beard,hair,eyebrow";
        private const string PARTNER = "partner";
        private const string DATA = "data";
        private const string BODY_TYPE = "bodyType";
        private const string ID = "id";

        private readonly AuthorizedRequest authorizedRequest;
        private readonly CancellationToken ctx;

        public AvatarAPIRequests(CancellationToken ctx = default)
        {
            this.ctx = ctx;
            authorizedRequest = new AuthorizedRequest();
        }

        public async Task<List<UserAvatarResponse>> GetUserAvatars(string userId)
        {
            var response = await authorizedRequest.SendRequest<ResponseText>(
                new RequestData
                {
                    Url = $"{RPM_AVATAR_V1_BASE_URL}/?userId={userId}&select={ID},{PARTNER},{DATA}.{BODY_TYPE}",
                    Method = HttpMethod.GET
                },
                ctx
            );
            response.ThrowIfError();

            var json = JObject.Parse(response.Text);
            var data = json[DATA]!;
            var avatars = data.AsEnumerable().Select(avatarData => new UserAvatarResponse()
            {
                BodyType = EnumExtensions.GetValueFromDescription<BodyType>(avatarData[DATA][BODY_TYPE]!.ToString()),
                Id = avatarData[ID]!.ToString(),
                Partner = avatarData[PARTNER]!.ToString()
            });

            return avatars.Where(avatar => avatar.BodyType == CoreSettingsHandler.CoreSettings.BodyType).ToList();
        }

        public async Task<List<AvatarTemplateData>> GetAvatarTemplates()
        {
            var response = await authorizedRequest.SendRequest<ResponseText>(
                new RequestData
                {
                    Url = $"{RPM_AVATAR_V2_BASE_URL}/templates?{BODY_TYPE}={CoreSettingsHandler.CoreSettings.BodyType.GetDescription()}",
                    Method = HttpMethod.GET
                },
                ctx
            );
            response.ThrowIfError();

            var json = JObject.Parse(response.Text);
            var data = json[DATA]!;
            return JsonConvert.DeserializeObject<List<AvatarTemplateData>>(data.ToString());
        }

        public async Task<AvatarProperties> CreateFromTemplateAvatar(string templateId, string partner)
        {
            var payloadData = new Dictionary<string, string>
            {
                { nameof(partner), partner },
                { BODY_TYPE, CoreSettingsHandler.CoreSettings.BodyType.GetDescription() }
            };

            var payload = AuthDataConverter.CreateDataPayload(payloadData);

            var response = await authorizedRequest.SendRequest<ResponseText>(
                new RequestData
                {
                    Url = $"{RPM_AVATAR_V2_BASE_URL}/templates/{templateId}",
                    Method = HttpMethod.POST,
                    Payload = payload
                },
                ctx
            );

            response.ThrowIfError();

            var json = JObject.Parse(response.Text);
            var data = json[DATA]!.ToString();
            return JsonConvert.DeserializeObject<AvatarProperties>(data);
        }

        public async Task<AssetColor[]> GetAvatarColors(string avatarId, AssetType assetType = AssetType.None)
        {
            ValidateAvatarId(avatarId);
            var colorParameters = assetType.GetColorProperty();
            if (string.IsNullOrEmpty(colorParameters))
            {
                colorParameters = "skin,beard,hair,eyebrow";
            }

            var response = await authorizedRequest.SendRequest<ResponseText>(
                new RequestData
                {
                    Url = $"{RPM_AVATAR_V2_BASE_URL}/{avatarId}/colors?type={colorParameters}",
                    Method = HttpMethod.GET
                },
                ctx
            );

            response.ThrowIfError();
            return ColorResponseHandler.GetColorsFromResponse(response.Text);
        }

        public async Task<AvatarProperties> GetAvatarMetadata(string avatarId, bool isDraft = false)
        {
            ValidateAvatarId(avatarId);
            var url = $"{RPM_AVATAR_V2_BASE_URL}/{avatarId}.json?";

            if (isDraft)
                url += "preview=true";

            var response = await authorizedRequest.SendRequest<ResponseText>(
                new RequestData
                {
                    Url = url,
                    Method = HttpMethod.GET
                },
                ctx
            );

            response.ThrowIfError();

            var json = JObject.Parse(response.Text);
            var data = json[DATA]!.ToString();
            return JsonConvert.DeserializeObject<AvatarProperties>(data);
        }

        public async Task<AvatarProperties> CreateNewAvatar(AvatarProperties avatarProperties)
        {
            var response = await authorizedRequest.SendRequest<ResponseText>(
                new RequestData
                {
                    Url = RPM_AVATAR_V2_BASE_URL,
                    Method = HttpMethod.POST,
                    Payload = avatarProperties.ToJson(true)
                },
                ctx
            );
            response.ThrowIfError();

            var metadata = JObject.Parse(response.Text);
            var data = metadata[DATA]!.ToString();
            return JsonConvert.DeserializeObject<AvatarProperties>(data);
        }

        public async Task<byte[]> GetAvatar(string avatarId, bool isPreview = false, string parameters = null)
        {
            ValidateAvatarId(avatarId);
            var url = $"{RPM_AVATAR_V2_BASE_URL}/{avatarId}.glb?";

            if (!string.IsNullOrEmpty(parameters))
                url += parameters?.Substring(1) + "&";

            if (isPreview)
                url += "preview=true";

            var response = await authorizedRequest.SendRequest<ResponseData>(
                new RequestData
                {
                    Url = url,
                    Method = HttpMethod.GET
                },
                ctx);

            response.ThrowIfError();
            return response.Data;
        }

        public async Task<AvatarProperties> GetAvatarProperties(string avatarId)
        {
            ValidateAvatarId(avatarId);
            var url = $"{RPM_AVATAR_V2_BASE_URL}/{avatarId}.json?";

            var response = await authorizedRequest.SendRequest<ResponseText>(
                new RequestData
                {
                    Url = url,
                    Method = HttpMethod.GET
                },
                ctx);

            response.ThrowIfError();
            var json = JObject.Parse(response.Text);
            var data = json[DATA]!.ToString();
            return JsonConvert.DeserializeObject<AvatarProperties>(data);
        }

        public async Task<byte[]> UpdateAvatar(string avatarId, AvatarProperties avatarProperties, string parameters = null)
        {
            ValidateAvatarId(avatarId);
            var url = $"{RPM_AVATAR_V2_BASE_URL}/{avatarId}?responseType=glb&{parameters}";

            var response = await authorizedRequest.SendRequest<ResponseData>(
                new RequestData
                {
                    Url = url,
                    Method = HttpMethod.PATCH,
                    Payload = avatarProperties.ToJson(true)
                },
                ctx);
            response.ThrowIfError();
            return response.Data;
        }

        public async Task PrecompileAvatar(string avatarId, PrecompileData precompileData, string parameters = null)
        {
            ValidateAvatarId(avatarId);
            var json = JsonConvert.SerializeObject(precompileData);

            var response = await authorizedRequest.SendRequest<ResponseText>(
                new RequestData
                {
                    Url = $"{RPM_AVATAR_V2_BASE_URL}/{avatarId}/precompile?{parameters ?? string.Empty}",
                    Method = HttpMethod.POST,
                    Payload = json
                },
                ctx);

            response.ThrowIfError();
        }

        public async Task<string> SaveAvatar(string avatarId)
        {
            ValidateAvatarId(avatarId);
            var response = await authorizedRequest.SendRequest<ResponseText>(
                new RequestData
                {
                    Url = $"{RPM_AVATAR_V2_BASE_URL}/{avatarId}",
                    Method = HttpMethod.PUT
                },
                ctx);

            response.ThrowIfError();
            return response.Text;
        }

        public async Task DeleteAvatar(string avatarId, bool isDraft = false)
        {
            ValidateAvatarId(avatarId);
            var url = $"{RPM_AVATAR_V2_BASE_URL}/{avatarId}/";

            if (isDraft)
                url += "draft";

            var response = await authorizedRequest.SendRequest<ResponseText>(
                new RequestData
                {
                    Url = url,
                    Method = HttpMethod.DELETE
                },
                ctx);

            response.ThrowIfError();
        }

        private void ValidateAvatarId(string avatarId)
        {
            if (string.IsNullOrEmpty(avatarId))
            {
                throw new Exception(INVALID_AVATAR_ID_ERROR_MESSAGE);
            }
        }
    }
}
