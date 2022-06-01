using HatCommunityWebsite.Service.Responses.Data;

namespace HatCommunityWebsite.Service.Responses
{
    public class UserProfileRunsResponse
    {
        public List<FullGameRunsData> FullGameRuns { get; set; }
        public List<LevelRunsData> LevelRuns { get; set; }
        public GameData Game { get; set; }
        public List<RunData> Runs { get; set; }
    }

    public class FullGameRunsData
    {
        public GameData Game { get; set; }
        public List<RunData> Runs { get; set; }
    }

    public class LevelRunsData
    {
        public GameData Game { get; set; }
        public List<RunData> Runs { get; set; }   
    }
}