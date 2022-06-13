using HatCommunityWebsite.Service.Helpers;
using Newtonsoft.Json;

namespace HatCommunityWebsite.Service.Responses
{
    public class AuthenticateResponse
    {
        public string JwtToken { get; set; }
        public string Username { get; set; }
        public bool IsAdmin { get; set; }

        [JsonIgnore]
        public RefreshToken RefreshToken { get; set; }
    }
}