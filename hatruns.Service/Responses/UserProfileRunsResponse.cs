using HatCommunityWebsite.Service.Responses.Data;

namespace HatCommunityWebsite.Service.Responses
{
    public class UserProfileRunsResponse
    {
        public List<RunData> FullGameRuns { get; set; }
        public List<RunData> LevelRuns { get; set; }
        public GameData Game { get; set; }
    }
}