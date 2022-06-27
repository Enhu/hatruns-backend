using HatCommunityWebsite.Service;
using HatCommunityWebsite.Service.Dtos;
using HatCommunityWebsite.Service.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HatCommunityWebsite.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LeaderboardController : Controller
    {
        private readonly ILeaderboardService _leaderboardService;

        public LeaderboardController(ILeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        [AllowAnonymous]
        [HttpGet("getruns/{categoryId}")]
        public async Task<ActionResult<LeaderboardRunsResponse>> GetLeaderboard(int categoryId, [FromQuery] int? subcategoryId, [FromQuery] int? levelId)
        {
            var response = await _leaderboardService.GetLeaderboardRuns(categoryId, subcategoryId, levelId);
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpGet("getdata/{gameId}")]
        public async Task<ActionResult<GameDataResponse>> GetGameData(string gameId, [FromQuery] int? levelId)
        {
            var response = await _leaderboardService.GetLeaderboardData(gameId, levelId);
            return Ok(response);
        }
    }
}
