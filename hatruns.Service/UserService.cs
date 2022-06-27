using AutoMapper;
using HatCommunityWebsite.DB;
using HatCommunityWebsite.Repo;
using HatCommunityWebsite.Service.Dtos;
using HatCommunityWebsite.Service.Helpers;
using HatCommunityWebsite.Service.Responses;
using HatCommunityWebsite.Service.Responses.Data;
using System.Security.Claims;

namespace HatCommunityWebsite.Service
{
    public interface IUserService
    {
        Task<List<UserProfileRunsResponse>> GetUserProfileRuns(string username);

        Task<UserDataResponse> GetUserData(string username);

        Task UpdateUserLocation(NewLocationDto request, ClaimsIdentity userIdentity);

        Task UpdateUserPronouns(NewPronounsDto request, ClaimsIdentity userIdentity);

        Task UpdateUserSocials(NewSocialsDto request, ClaimsIdentity userIdentity);

        Task UpdateUserAvatar(NewAvatarDto request, ClaimsIdentity userIdentity);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRunRepository _runRepo;
        private readonly IGameRepository _gameRepo;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepo, IRunRepository runRepo, IGameRepository gameRepo, IMapper mapper)
        {
            _userRepo = userRepo;
            _runRepo = runRepo;
            _gameRepo = gameRepo;
            _mapper = mapper;
        }

        public async Task<List<UserProfileRunsResponse>> GetUserProfileRuns(string username)
        {
            var runs = await _runRepo.GetVerifiedUserProfileRuns(username);
            var gameList = await _gameRepo.GetAllGames();

            var response = new List<UserProfileRunsResponse>();

            var fullGameRuns = runs.Where(x => x.Category.LevelId == null).ToList();
            var levelRuns = runs.Where(x => x.Category.LevelId != null).ToList();

            SetGames(gameList, response);
            await SetFullGameRuns(fullGameRuns, response);
            await SetLevelRuns(levelRuns, response);

            response.RemoveAll(x => x.FullGameRuns.Count == 0 && x.LevelRuns.Count == 0);

            return response;
        }

        public async Task<UserDataResponse> GetUserData(string username)
        {
            var user = await _userRepo.GetUserByUsername(username);

            var response = _mapper.Map<UserDataResponse>(user);
            response.Avatar = SetUserAvatar(user.Avatar, user.ImageType);

            return response;
        }

        public async Task UpdateUserLocation(NewLocationDto request, ClaimsIdentity userIdentity)
        {
            var userId = int.Parse(userIdentity.FindFirst("UserId").Value);
            var user = await _userRepo.GetUserById(userId);

            if (user == null)
                throw new AppException("User not found");

            user.Country = request.Country;
            user.CountryCode = request.CountryCode;

            await _userRepo.UpdateUser(user);
        }

        public async Task UpdateUserPronouns(NewPronounsDto request, ClaimsIdentity userIdentity)
        {
            var userId = int.Parse(userIdentity.FindFirst("UserId").Value);
            var user = await _userRepo.GetUserById(userId);

            if (user == null)
                throw new AppException("User not found");

            user.HePronouns = request.HePronouns;
            user.ShePronouns = request.ShePronouns;
            user.TheyPronouns = request.TheyPronouns;

            await _userRepo.UpdateUser(user);
        }

        public async Task UpdateUserSocials(NewSocialsDto request, ClaimsIdentity userIdentity)
        {
            var userId = int.Parse(userIdentity.FindFirst("UserId").Value);
            var user = await _userRepo.GetUserById(userId);

            if (user == null)
                throw new AppException("User not found");

            user.TwitchHandle = request.TwitchHandle;
            user.DiscordHandle = request.DiscordHandle;
            user.YoutubeHandle = request.YoutubeHandle;

            await _userRepo.UpdateUser(user);
        }

        public async Task UpdateUserAvatar(NewAvatarDto request, ClaimsIdentity userIdentity)
        {
            var userId = int.Parse(userIdentity.FindFirst("UserId").Value);
            var user = await _userRepo.GetUserById(userId);

            if (user == null)
                throw new AppException("User not found");

            byte[]? imgBytes = null;
            string? imgType = null;

            if (request.Avatar != null)
            {
                imgType = request.Avatar.Split(',')[0];
                imgBytes = Convert.FromBase64String(request.Avatar.Split(',')[1]);
            }

            var imageMB = imgBytes.Length / 1024F / 1024F;
            if (imageMB > 2)
                throw new AppException("Image cannot be bigger than 2MB");

            user.Avatar = imgBytes;
            user.ImageType = imgType;

            await _userRepo.UpdateUser(user);
        }

        //helper methods
        private async Task<string> GetRunPlace(int runId, int categoryId, int? subcategoryId)
        {
            var runs = await _runRepo.GetLeaderboardRuns(categoryId, subcategoryId);

            return (runs.FindIndex(x => x.Id == runId) + 1).ToString();
        }

        private string SetUserAvatar(byte[] avatarBytes, string imageType)
        {
            string avatar = string.Empty;

            if (avatarBytes != null && imageType != null)
            {
                var avatarString = Convert.ToBase64String(avatarBytes);
                avatar = string.Format("{0},{1}", imageType, avatarString);
            }

            return avatar;
        }

        private async Task SetLevelRuns(List<Run> levelRuns, List<UserProfileRunsResponse> response)
        {

            foreach (var run in levelRuns)
            {
                var game = response.Where(x => x.Game.Name == run.Category.Game.Name).FirstOrDefault();

                var runData = new RunData
                {
                    Id = run.Id,
                    Place = await GetRunPlace(run.Id, run.CategoryId.Value, run.SubcategoryId),
                    CategoryName = run.Category.Name,
                    SubcategoryName = run.SubCategory?.Name,
                    Date = run.Date,
                    IsObsolete = run.IsObsolete,
                    Time = run.Time,
                    LevelName = run.Category.Level.Name
                };

                game.LevelRuns.Add(runData);
            }
        }

        private async Task SetFullGameRuns(List<Run> fullGameRuns, List<UserProfileRunsResponse> response)
        {
            foreach (var run in fullGameRuns)
            {
                var game = response.Where(x => x.Game.Name == run.Category.Game.Name).FirstOrDefault();

                var runData = new RunData
                {
                    Id = run.Id,
                    Place = await GetRunPlace(run.Id, run.CategoryId.Value, run.SubcategoryId),
                    CategoryName = run.Category.Name,
                    SubcategoryName = run.SubCategory?.Name,
                    Date = run.Date,
                    IsObsolete = run.IsObsolete,
                    Time = run.Time
                };

                game.FullGameRuns.Add(runData);
            }
        }

        private static void SetGames(List<Game> gameList, List<UserProfileRunsResponse> response)
        {
            foreach (var game in gameList)
            {
                var gameData = new GameData
                {
                    Name = game.Name,
                    Acronym = game.Acronym
                };

                var gameRuns = new UserProfileRunsResponse
                {
                    Game = gameData,
                    FullGameRuns = new List<RunData>(),
                    LevelRuns = new List<RunData>()
                };

                response.Add(gameRuns);
            }
        }
    }
}