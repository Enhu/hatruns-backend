using HatCommunityWebsite.DB;
using HatCommunityWebsite.Repo;
using HatCommunityWebsite.Service.Dtos;
using HatCommunityWebsite.Service.Responses;
using HatCommunityWebsite.Service.Responses.Data;

namespace HatCommunityWebsite.Service
{
    public interface ILeaderboardService
    {
        LeaderboardResponse GetLeaderboard(LeaderboardDto request);
    }

    public class LeaderboardService : ILeaderboardService
    {
        private readonly IRunRepository _runRepo;
        private int Place { get; set; } = 1;

        public LeaderboardService(IRunRepository runRepo)
        {
            _runRepo = runRepo;
        }

        public LeaderboardResponse GetLeaderboard(LeaderboardDto request)
        {
            var runs = _runRepo.GetAllLeaderboardRuns(request.CategoryId, request.SubcategoryId, request.LevelId).Result;

            var response = new LeaderboardResponse();

            foreach (var run in runs)
            {
                var runData = new RunData
                {
                    Id = run.Id,
                    PlayerNames = SetPlayerNames(run.RunUsers),
                    CategoryName = run.Category.Name,
                    Date = run.Date,
                    Time = run.Time,
                    IsObsolete = run.IsObsolete,
                    Place = SetRunPlace(run, runs)
                };

                response.Runs.Add(runData);
            }

            Place = 1;

            return response;
        }

        private List<string> SetPlayerNames(ICollection<RunUser> runUsers)
        {
            var usernames = new List<string>();

            foreach (var runUser in runUsers)
            {
                usernames.Add(runUser.AssociatedUser.Username);
            }

            return usernames;
        }

        private string SetRunPlace(Run run, List<Run> runs)
        {
            if (run.IsObsolete)
                return "-";

            var index = runs.IndexOf(run);

            var prevRun = runs[index - 1];

            if (prevRun != null && prevRun.Time == run.Time)
                return Place.ToString();

            Place++;

            return (Place - 1).ToString();
        }
    }
}