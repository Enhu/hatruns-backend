using HatCommunityWebsite.DB;
using HatCommunityWebsite.Repo;
using HatCommunityWebsite.Service.Dtos;
using HatCommunityWebsite.Service.Helpers;
using HatCommunityWebsite.Service.Responses;
using HatCommunityWebsite.Service.Responses.Data;

namespace HatCommunityWebsite.Service
{
    public interface ILeaderboardService
    {
        Task<LeaderboardRunsResponse> GetLeaderboardRuns(int categoryId, int? subcategoryId = null, int? levelId = null);
        Task<GameDataResponse> GetLeaderboardData(string gameId, int? levelId);
    }

    public class LeaderboardService : ILeaderboardService
    {
        private readonly IRunRepository _runRepo;
        private readonly IGameRepository _gameRepo;
        private int LeaderboardPlace { get; set; } = 1;

        public LeaderboardService(IRunRepository runRepo, IGameRepository gameRepo)
        {
            _runRepo = runRepo;
            _gameRepo = gameRepo;
        }

        public async Task<LeaderboardRunsResponse> GetLeaderboardRuns(int categoryId, int? subcategoryId = null, int? levelId = null)
        {
            var runs = await _runRepo.GetAllLeaderboardRuns(categoryId, subcategoryId, levelId);

            var response = new LeaderboardRunsResponse();
            response.Runs = new List<RunData>();

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
                    Place = SetRunPlace(run, runs),
                    VariablesValues = SetValueNames(run.RunVariableValues)
                };

                response.Runs.Add(runData);
            }

            LeaderboardPlace = 1;

            return response;
        }

        private List<string> SetValueNames(ICollection<RunVariableValue>? runVariableValues)
        {
            return runVariableValues.Select(x => x.AssociatedVariableValue.Name).ToList();
        }

        public async Task<GameDataResponse> GetLeaderboardData(string gameId, int? levelId)
        {
            var gameData = await _gameRepo.GetGameByAcronymIncludeAll(gameId);

            if (gameData == null)
                throw new AppException(string.Format("Game not found. Id: {0}", gameId));

            if (levelId.HasValue && !gameData.Levels.Any(x => x.Id == levelId))
                throw new AppException(string.Format("Level not found. Id: {0}"), levelId);

            var response = SetGameDataResponse(gameData, levelId);

            return response;
        }

        //helper methods

        private GameDataResponse SetGameDataResponse(Game gameData, int? levelId)
        {
            var response = new GameDataResponse();
            response.Game = new LbGameData();
            response.Variables = new List<LbVariableData>();
            response.Categories = new List<LbCategoryData>();
            response.Levels = new List<LbLevelData>();

            GetGame(gameData, response);

            if (!levelId.HasValue)
                GetGameCategories(gameData, response);
            else
                GetLevelCategories(gameData, response, levelId.Value);

            GetGameVariables(gameData, response);
            GetGameLevels(gameData, response);

            return response;
        }

        private void GetLevelCategories(Game gameData, GameDataResponse response, int levelId)
        {
            var level = gameData.Levels.FirstOrDefault(x => x.Id == levelId);

            foreach (var item in level.Categories)
            {
                var category = new LbCategoryData();
                category.Subcategories = new List<LbSubcategoryData>();

                category.Id = item.Id;
                category.Name = item.Name;
                category.Rules = item.Rules;

                response.Categories.Add(category);
            }
        }

        //helper methods
        private void GetGame(Game gameData, GameDataResponse response)
        {
            response.Game.Id = gameData.Id;
            response.Game.Name = gameData.Name;
            response.Game.ReleasedDate = gameData.ReleaseDate;
            response.Game.IsActive = gameData.IsActive;
            response.Game.Portrait = string.Empty;
        }

        private static void GetGameLevels(Game gameData, GameDataResponse response)
        {
            foreach (var item in gameData.Levels)
            {
                var level = new LbLevelData();
                level.Categories = new List<LbCategoryData>();

                level.Id = item.Id;
                level.Name = item.Name;
                level.Rules = item.Rules;

                foreach (var subItem in item.Categories)
                {
                    var category = new LbCategoryData();

                    category.Id = subItem.Id;
                    category.Name = subItem.Name;
                    category.Rules = subItem.Rules;

                    level.Categories.Add(category);
                }

                response.Levels.Add(level);
            }
        }

        private static void GetGameVariables(Game gameData, GameDataResponse response)
        {
            foreach (var item in gameData.Variables)
            {
                var variable = new LbVariableData();
                variable.Id = item.Id;
                variable.Name = item.Name;

                response.Variables.Add(variable);
            }
        }

        private static void GetGameCategories(Game gameData, GameDataResponse response)
        {
            foreach (var item in gameData.Categories)
            {
                var category = new LbCategoryData();
                category.Subcategories = new List<LbSubcategoryData>();

                category.Id = item.Id;
                category.Name = item.Name;
                category.Rules = item.Rules;

                foreach (var subItem in item.Subcategories)
                {
                    var subcategory = new LbSubcategoryData();
                    subcategory.Id = subItem.Id;
                    subcategory.Name = subItem.Name;
                    subcategory.Rules = subItem.Rules;
                    category.Subcategories.Add(subcategory);
                }

                response.Categories.Add(category);
            }
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

            var prevRun = index == 0 ? null : runs[index - 1];

            if (prevRun != null && prevRun.Time == run.Time)
                return LeaderboardPlace.ToString();

            var runPlace = LeaderboardPlace;
            LeaderboardPlace++;

            return runPlace.ToString();
        }
    }
}