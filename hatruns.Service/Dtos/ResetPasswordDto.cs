namespace HatCommunityWebsite.Service.Dtos
{
    public class ResetPasswordDto
    {
        public string Password { get; set; }
        public string Token { get; set; }
    }

    public class ValidateResetPasswordDto
    {
        public string Token { get; set; }

    }
}
