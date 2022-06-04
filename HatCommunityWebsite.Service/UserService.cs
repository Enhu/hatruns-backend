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
        UserProfileRunsResponse GetUserProfileRuns(string username);

        UserDataResponse GetUserData(string username);

        void UpdateUserLocation(NewLocationDto request, ClaimsIdentity userIdentity);

        void UpdateUserPronouns(NewPronounsDto request, ClaimsIdentity userIdentity);

        void UpdateUserSocials(NewSocialsDto request, ClaimsIdentity userIdentity);

        void UpdateUserAvatar(NewAvatarDto request, ClaimsIdentity userIdentity);
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

        public UserProfileRunsResponse GetUserProfileRuns(string username)
        {
            var runs = _runRepo.GetVerifiedUserProfileRuns(username).Result;
            var gameList = _gameRepo.GetAllGames().Result;

            var response = new UserProfileRunsResponse();

            var fullGameRuns = runs.Where(x => x.Category.Level == null).ToList();
            var levelRuns = runs.Where(x => x.Category.Level != null).ToList();

            SetGames(gameList, response);
            SetFullGameRuns(fullGameRuns, response);
            SetLevelRuns(levelRuns, response);

            response.FullGameRuns.RemoveAll(x => x.Runs.Count == 0);
            response.LevelRuns.RemoveAll(x => x.Runs.Count == 0);

            return response;
        }

        public UserDataResponse GetUserData(string username)
        {
            var user = _userRepo.GetUserByUsername(username).Result;

            var response = _mapper.Map<UserDataResponse>(user);
            response.Avatar = SetUserAvatar(user.Avatar, user.ImageType);

            return response;
        }

        public void UpdateUserLocation(NewLocationDto request, ClaimsIdentity userIdentity)
        {
            var userId = int.Parse(userIdentity.FindFirst("UserId").Value);
            var user = _userRepo.GetUserById(userId).Result;

            if (user == null)
                throw new AppException("User not found");

            user.Country = request.Country;
            user.CountryCode = request.CountryCode;

            _userRepo.UpdateUser(user);
        }

        public void UpdateUserPronouns(NewPronounsDto request, ClaimsIdentity userIdentity)
        {
            var userId = int.Parse(userIdentity.FindFirst("UserId").Value);
            var user = _userRepo.GetUserById(userId).Result;

            if (user == null)
                throw new AppException("User not found");

            user.HePronouns = request.HePronouns;
            user.ShePronouns = request.ShePronouns;
            user.TheyPronouns = request.TheyPronouns;

            _userRepo.UpdateUser(user);
        }

        public void UpdateUserSocials(NewSocialsDto request, ClaimsIdentity userIdentity)
        {
            var userId = int.Parse(userIdentity.FindFirst("UserId").Value);
            var user = _userRepo.GetUserById(userId).Result;

            if (user == null)
                throw new AppException("User not found");

            user.TwitchHandle = request.TwitchHandle;
            user.DiscordHandle = request.DiscordHandle;
            user.YoutubeHandle = request.YoutubeHandle;

            _userRepo.UpdateUser(user);
        }

        public void UpdateUserAvatar(NewAvatarDto request, ClaimsIdentity userIdentity)
        {
            var userId = int.Parse(userIdentity.FindFirst("UserId").Value);
            var user = _userRepo.GetUserById(userId).Result;

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

            _userRepo.UpdateUser(user);
        }

        //helper methods
        private string GetRunPlace(int runId, int categoryId, int? subcategoryId)
        {
            var runs = _runRepo.GetLeaderboardRuns(categoryId, subcategoryId).Result;

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

        private void SetLevelRuns(List<Run> levelRuns, UserProfileRunsResponse response)
        {
            foreach (var run in levelRuns)
            {
                var game = response.LevelRuns.Where(x => x.Game.Name == run.Category.Game.Name).FirstOrDefault();

                var runData = new RunData
                {
                    Id = run.Id,
                    Place = GetRunPlace(run.Id, run.CategoryId.Value, run.SubcategoryId),
                    CategoryName = run.Category.Name,
                    SubcategoryName = run.SubCategory?.Name,
                    Date = run.Date,
                    IsObsolete = run.IsObsolete,
                    Time = run.Time,
                    LevelName = run.Category.Level.Name
                };

                game.Runs.Add(runData);
            }
        }

        private void SetFullGameRuns(List<Run> fullGameRuns, UserProfileRunsResponse response)
        {
            foreach (var run in fullGameRuns)
            {
                var game = response.FullGameRuns.Where(x => x.Game.Name == run.Category.Game.Name).FirstOrDefault();

                var runData = new RunData
                {
                    Id = run.Id,
                    Place = GetRunPlace(run.Id, run.CategoryId.Value, run.SubcategoryId),
                    CategoryName = run.Category.Name,
                    SubcategoryName = run.SubCategory?.Name,
                    Date = run.Date,
                    IsObsolete = run.IsObsolete,
                    Time = run.Time
                };

                game.Runs.Add(runData);
            }
        }

        private static void SetGames(List<Game> gameList, UserProfileRunsResponse response)
        {
            foreach (var game in gameList)
            {
                response.FullGameRuns.Add(new FullGameRunsData { Game = new GameData { Name = game.Name }, Runs = new List<RunData>() });
                response.LevelRuns.Add(new LevelRunsData { Game = new GameData { Name = game.Name }, Runs = new List<RunData>() });
            }
        }
    }
}