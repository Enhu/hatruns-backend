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
                    PlayerName = run.PlayerName,
                    CategoryName = run.Category.Name,
                    Date = run.Date,
                    Time = run.Time,
                    IsObsolete = run.IsObsolete,
                    Place = GetRunPlace(run, runs)
                };

                response.Runs.Add(runData);
            }

            Place = 1;

            return response;
        }

        private string GetRunPlace(Run run, List<Run> runs)
        {
            if (run.IsObsolete)
                return string.Empty;

            var index = runs.IndexOf(run);

            var prevRun = runs[index - 1];

            if (prevRun != null && prevRun.Time == run.Time)
                return Place.ToString();

            Place++;

            return (Place - 1).ToString();
        }
    }
}