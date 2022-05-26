using FullRuns.DB;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatCommunityWebsite.DB
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string? Country { get; set; }
        public string? CountryCode { get; set; }
        public byte[]? Avatar { get; set; }
        public string? ImageType { get; set; }
        [JsonIgnore]
        public byte[] PasswordHash { get; set; }
        [JsonIgnore]
        public byte[] PasswordSalt { get; set; }
        [JsonIgnore]
        public string Email { get; set; }
        public string? TwitchHandle { get; set; }
        public string? DiscordHandle { get; set; }
        public string? YoutubeHandle { get; set; }
        public bool HePronouns { get; set; }
        public bool ShePronouns { get; set; }
        public bool TheyPronouns { get; set; }
        public DateTime Created { get; set; }
        public string? VerificationToken { get; set; }
        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordTokenExpires { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public bool IsVerified => VerifiedDate.HasValue;
        public int Role { get; set; }
        public List<Run>? Runs { get; set; }
    }
}
