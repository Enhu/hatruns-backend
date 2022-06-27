namespace HatCommunityWebsite.Service.Responses
{
    public class UserDataResponse
    {
        public string Avatar;

        //automapper properties
        public string TwitchHandle { get; set; }
        public string DiscordHandle { get; set; }
        public string YoutubeHandle { get; set; }
        public bool HePronouns { get; set; }
        public bool ShePronouns { get; set; }
        public bool TheyPronouns { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
    }
}