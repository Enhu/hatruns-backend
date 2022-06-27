using HatCommunityWebsite.Service;
using HatCommunityWebsite.Service.Dtos;
using HatCommunityWebsite.Service.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HatCommunityWebsite.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("getruns/{username}")]
        public async Task<ActionResult<List<UserProfileRunsResponse>>> GetUserProfileRuns(string username)
        {
            var response = await _userService.GetUserProfileRuns(username);
            return Ok(response);
        }

        [HttpGet("getdata/{username}")]
        public async Task<ActionResult<UserDataResponse>> GetUserData(string username)
        {
            var response = await _userService.GetUserData(username);
            return Ok(response);
        }

        [Authorize]
        [HttpPut("update/avatar")]
        public async Task<IActionResult> UpdateUserAvatar([FromBody] NewAvatarDto request)
        {
            var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
            if (userIdentity == null)
                return Unauthorized("Could not recognize user identity");

            await _userService.UpdateUserAvatar(request, userIdentity);
            return Ok(new { message = "Avatar updated" });
        }

        [Authorize]
        [HttpPut("update/socials")]
        public async Task<IActionResult> UpdateUserSocials([FromBody] NewSocialsDto request)
        {
            var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
            if (userIdentity == null)
                return Unauthorized("Could not recognize user identity");

            await _userService.UpdateUserSocials(request, userIdentity);
            return Ok(new { message = "Socials updated" });
        }

        [Authorize]
        [HttpPut("update/pronouns")]
        public async Task<IActionResult> UpdateUserPronouns([FromBody] NewPronounsDto request)
        {
            var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
            if (userIdentity == null)
                return Unauthorized("Could not recognize user identity");

            await _userService.UpdateUserPronouns(request, userIdentity);
            return Ok(new { message = "Pronouns updated" });
        }

        [Authorize]
        [HttpPut("update/location")]
        public async Task<IActionResult> UpdateUserLocation([FromBody] NewLocationDto request)
        {
            var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
            if (userIdentity == null)
                return Unauthorized("Could not recognize user identity");

            await _userService.UpdateUserLocation(request, userIdentity);
            return Ok(new { message = "Location updated" });
        }
    }
}