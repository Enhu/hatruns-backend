using FullRuns.DB;
using HatCommunityWebsite.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HatCommunityWebsite.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly ILogger<RunController> _logger;
        private readonly AppDbContext _context;

        public UserController(AppDbContext context, ILogger<RunController> logger)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet("getruns/{username}")]
        public async Task<ActionResult<string>> GetUserRuns(string username)
        {
            var rawFullRuns = await _context.Runs
                .Include(x => x.Category)
                .Include(x => x.Game)
                .Include(x => x.SubCategory)
                .Where(x => x.User.Username == username && x.Category.IsLevel == false && x.Status == (int)RunController.Status.Verified)
                .OrderByDescending(x => x.Date)
                .ToListAsync();

            var rawLevelRuns = await _context.Runs
                .Include(x => x.Category)
                .Include(x => x.Game)
                .Include(x => x.SubCategory)
                .Where(x => x.User.Username == username && x.Category.IsLevel == true && x.Status == (int)RunController.Status.Verified)
                .OrderByDescending(x => x.Date)
                .ToListAsync();

            var gameList = await _context.Games
                .ToListAsync();

            var userFullRuns = new List<UserProfileResponse>();
            var userLevelRuns = new List<UserProfileResponse>();

            foreach (var game in gameList)
            {
                userFullRuns.Add(new UserProfileResponse { Game = game, Runs = new List<RunData>() });
                userLevelRuns.Add(new UserProfileResponse { Game = game, Runs = new List<RunData>() });
            }

            foreach (var run in rawFullRuns)
            {
                var game = userFullRuns.Where(x => x.Game.Acronym == run.Game.Acronym).FirstOrDefault();

                var runData = new RunData
                {
                    Id = run.Id,
                    Place = await GetRunPlace(run),
                    CategoryName = run.Category.Name,
                    SubcategoryName = run.SubCategory?.Name,
                    Date = run.Date,
                    IsObsolete = run.IsObsolete,
                    Time = run.Time
                };

                game.Runs.Add(runData);
            }

            foreach (var run in rawLevelRuns)
            {
                var game = userLevelRuns.Where(x => x.Game.Acronym == run.Game.Acronym).FirstOrDefault();

                var runData = new RunData
                {
                    Id = run.Id,
                    Place = await GetRunPlace(run),
                    CategoryName = run.Category.Name,
                    SubcategoryName = run.SubCategory?.Name,
                    Date = run.Date,
                    IsObsolete = run.IsObsolete,
                    Time = run.Time
                };

                game.Runs.Add(runData);
            }

            userFullRuns.RemoveAll(x => x.Runs.Count == 0);
            userLevelRuns.RemoveAll(x => x.Runs.Count == 0);

            return Ok(new { fullGameRuns = userFullRuns, levelRuns = userLevelRuns });
        }

        [HttpGet("getinfo/{username}")]
        public async Task<ActionResult<User>> GetUserInfo(string username)
        {
            var user = await _context.Users.Where(x => x.Username == username).FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            return user;
        }

        [HttpGet("getavatar/{username}")]
        public async Task<ActionResult<string?>> GetUserAvatar(string username)
        {
            var user = await _context.Users.Where(x => x.Username == username).FirstOrDefaultAsync();

            if (user == null)
                return NotFound("User not found.");

            string? avatar = null;

            if(user.Avatar != null && user.ImageType != null)
            {
                var avatarString = Convert.ToBase64String(user.Avatar);
                avatar = string.Format("{0},{1}", user.ImageType, avatarString);
            } 

            return avatar;
        }

        private async Task<string> GetRunPlace(Run run)
        {
            var runs = new List<Run>();

            runs = await _context.Runs
                .Where(x => x.IsObsolete == false)
                .Where(x => x.CategoryId == run.CategoryId)
                .Where(x => x.SubcategoryId == run.SubcategoryId)
                .ToListAsync();

            var orderedRuns = runs.OrderBy(x => x.Time).ToList();

            return (orderedRuns.FindIndex(x => x.Id == run.Id) + 1).ToString();
        }
        [HttpPut("updateavatar")]
        public async Task<ActionResult<string>> UpdateAvatar(AvatarDto request)
        {
            var user = await _context.Users.Where(x => x.Username == request.Username).FirstOrDefaultAsync();

            if (user == null)
                return NotFound("User not found.");

            byte[]? imgBytes = null;
            string? imgType = null;

            if(request.Avatar != null)
            {
                imgType = request.Avatar.Split(',')[0];
                imgBytes = Convert.FromBase64String(request.Avatar.Split(',')[1]);
            }

            user.Avatar = imgBytes;
            user.ImageType = imgType;

            _context.Users.Update(user);
            _context.SaveChanges();

            return Ok("Avatar updated.");
        }
        [HttpPut("updatesocials")]
        public async Task<ActionResult<string>> UpdateSocials(SocialsDto request)
        {
            var user = await _context.Users.Where(x => x.Username == request.Username).FirstOrDefaultAsync();

            if (user == null)
                return NotFound("User not found.");

            user.TwitchHandle = request.TwitchHandle;
            user.DiscordHandle = request.DiscordHandle;
            user.YoutubeHandle = request.YoutubeHandle;

            _context.Users.Update(user);
            _context.SaveChanges();

            return Ok("Socials updated.");
        }
        [HttpPut("updatepronouns")]
        public async Task<ActionResult<string>> UpdatePronouns(PronounsDto request)
        {
            var user = await _context.Users.Where(x => x.Username == request.Username).FirstOrDefaultAsync();

            if (user == null)
                return NotFound("User not found.");

            user.HePronouns = request.HePronouns;
            user.ShePronouns = request.ShePronouns;
            user.TheyPronouns = request.TheyPronouns;

            _context.Users.Update(user);
            _context.SaveChanges();

            return Ok("Pronouns updated.");
        }
        [HttpPut("updatecountry")]
        public async Task<ActionResult<string>> UpdateCountry(CountryDto request)
        {
            var user = await _context.Users.Where(x => x.Username == request.Username).FirstOrDefaultAsync();

            if (user == null)
                return NotFound("User not found.");

            user.Country = request.Country;
            user.CountryCode = request.CountryCode;

            _context.Users.Update(user);
            _context.SaveChanges();

            return Ok("Country updated.");
        }
        [HttpPut("updatepassword")]
        public async Task<ActionResult<string>> UpdatePassword(NewPasswordDto request)
        {
            return Ok("Password updated.");
        }
        [HttpPut("updateemail")]
        public async Task<ActionResult<string>> UpdateEmail(NewEmailDto request)
        {
            return Ok("Email updated.");
        }
    }

    public class UserProfileResponse
    {
        public Game Game { get; set; }
        public List<RunData> Runs { get; set; }
    }

    public class RunData
    {
        public int Id { get; set; }
        public string Place { get; set; }
        public string CategoryName { get; set; }
        public string SubcategoryName { get; set; }
        public double Time { get; set; }
        public DateTime Date { get; set; }
        public bool IsObsolete { get; set; }
    }
    public class AvatarDto
    {
        public string Username { get; set; }
        public string? Avatar { get; set; }
    }
    public class SocialsDto
    {
        public string Username { get; set; }
        public string? TwitchHandle { get; set; }
        public string? DiscordHandle { get; set; }
        public string? YoutubeHandle { get; set; }
    }
    public class PronounsDto
    {
        public string Username { get; set; }
        public bool HePronouns { get; set; }
        public bool ShePronouns{ get; set; }
        public bool TheyPronouns { get; set; }
    }
    public class CountryDto
    {
        public string Username { get; set; }
        public string? Country { get; set; }
        public string? CountryCode { get; set; }
    }
    public class NewPasswordDto
    {
        public string Username { get; set; }
        public string NewPassword { get; set; }
    }
    public class NewEmailDto
    {
        public string Username { get; set; }
        public string NewEmail { get; set; }
    }
}
