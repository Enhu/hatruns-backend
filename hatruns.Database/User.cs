using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HatCommunityWebsite.DB
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        //user info
        public string Username { get; set; }

        public string? Country { get; set; }
        public string? CountryCode { get; set; }
        public byte[]? Avatar { get; set; }
        public string? ImageType { get; set; }

        public byte[] PasswordHash { get; set; }

        public byte[] PasswordSalt { get; set; }

        public string Email { get; set; }

        public string? TwitchHandle { get; set; }
        public string? DiscordHandle { get; set; }
        public string? YoutubeHandle { get; set; }
        public bool HePronouns { get; set; }
        public bool ShePronouns { get; set; }
        public bool TheyPronouns { get; set; }
        public DateTime Created { get; set; }

        //reset password token
        public string? ResetPasswordToken { get; set; }

        public DateTime? ResetPasswordTokenExpires { get; set; }
        public bool ResetTokenExpired => ResetPasswordTokenExpires.HasValue && ResetPasswordTokenExpires.Value > DateTime.UtcNow;

        //user verification
        public string? VerificationToken { get; set; }

        public DateTime? VerifiedDate { get; set; }
        public bool IsVerified => VerifiedDate.HasValue;

        public bool IsImported { get; set; }

        //jwt token refresh
        public bool RememberLogIn { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenCreated { get; set; }

        public DateTime? RefreshTokenExpires { get; set; }

        //user role
        public int Role { get; set; }

        //navigation properties
        public ICollection<RunUser> RunUsers { get; set; }
    }
}