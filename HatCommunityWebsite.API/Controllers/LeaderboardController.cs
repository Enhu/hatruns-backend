using HatCommunityWebsite.Service;
using HatCommunityWebsite.Service.Dtos;
using HatCommunityWebsite.Service.Responses;
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

        [HttpGet]
        public ActionResult<LeaderboardResponse> GetLeaderboard(LeaderboardDto request)
        {
            var response = _leaderboardService.GetLeaderboard(request);
            return Ok(response);
        }
    }
}
