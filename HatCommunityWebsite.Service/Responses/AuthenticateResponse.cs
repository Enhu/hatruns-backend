using HatCommunityWebsite.Service.Helpers;
using Newtonsoft.Json;

namespace HatCommunityWebsite.Service.Responses
{
    public class AuthenticateResponse
    {
        public string JwtToken { get; set; }

        [JsonIgnore]
        public RefreshToken RefreshToken { get; set; }
    }
}