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
        public ActionResult<GameDashboardResponse> GetDashboardData(int gameId)
        {
            var response = _adminService.GetDashboardData(gameId);
            return Ok(response);
        }

        [Authorize(Policy = "Admin")]
        [HttpPut("game/update")]
        public IActionResult UpdateGameInfo(UpdateGameInfoDto request)
        {
            _adminService.UpdateGameInfo(request);
            return Ok(new { message = "Game updated" });
        }

        [Authorize(Policy = "Admin")]
        [HttpPut("game/levels/update")]
        public IActionResult HandleGameLevels(List<GameLevelDto> request)
        {
            _adminService.HandleGameLevels(request);
            return Ok(new { message = "Levels updated" });
        }

        [Authorize(Policy = "Admin")]
        [HttpPut("game/categories/update")]
        public IActionResult HandleGameCategories(List<CategoryDto> request)
        {
            _adminService.HandleGameCategories(request);
            return Ok(new { message = "Categories updated" });
        }

        [Authorize(Policy = "Admin")]
        [HttpPut("game/variables/update")]
        public IActionResult HandleGameVariables(List<VariableDto> request)
        {
            _adminService.HandleGameVariables(request);
            return Ok(new { message = "Variables updated" });
        }
    }
}