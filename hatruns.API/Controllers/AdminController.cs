using HatCommunityWebsite.Service;
using HatCommunityWebsite.Service.Dtos;
using HatCommunityWebsite.Service.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HatCommunityWebsite.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [Authorize(Policy = "Admin")]
        [HttpGet("dashboard/{gameId}")]
        public async Task<ActionResult<GameDashboardResponse>> GetDashboardData(int gameId)
        {
            var response = await _adminService.GetDashboardData(gameId);
            return Ok(response);
        }

        [Authorize(Policy = "Admin")]
        [HttpPost("game/update")]
        public async Task<IActionResult> UpdateGameInfo([FromBody] UpdateGameInfoDto request)
        {
            await _adminService.UpdateGameInfo(request);
            return Ok(new { message = "Game updated" });
        }

        [Authorize(Policy = "Admin")]
        [HttpPost("game/levels/update")]
        public async Task<IActionResult> HandleGameLevels([FromBody] List<GameLevelDto> request)
        {
            var response = await _adminService.HandleGameLevels(request);
            return Ok(new { message = response });
        }

        [Authorize(Policy = "Admin")]
        [HttpPost("game/categories/update")]
        public async Task<IActionResult> HandleGameCategories([FromBody] List<CategoryDto> request)
        {
            var response = await _adminService.HandleGameCategories(request);
            return Ok(new { message = response });
        }

        [Authorize(Policy = "Admin")]
        [HttpPost("game/variables/update")]
        public async Task<IActionResult> HandleGameVariables([FromBody] List<VariableDto> request)
        {
            var response = await _adminService.HandleGameVariables(request);
            return Ok(new { message = response });
        }
    }
}