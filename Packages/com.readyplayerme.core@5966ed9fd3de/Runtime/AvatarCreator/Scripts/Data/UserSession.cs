using Newtonsoft.Json;

namespace ReadyPlayerMe.AvatarCreator
{
    public struct UserSession
    {
        [JsonProperty("_id")]
        public string Id;
        public string Name;
        public string Email;
        public string Token;
        public string RefreshToken;
        public string LastModifiedAvatarId;

        public UserSession(CreatedUser createdUser)
        {
            Id = createdUser.Id;
            Name = createdUser.Name;
            Email = createdUser.Email;
            Token = createdUser.Token;
            RefreshToken = createdUser.RefreshToken;
            LastModifiedAvatarId = createdUser.LastModifiedAvatarId;
        }
    }
}

