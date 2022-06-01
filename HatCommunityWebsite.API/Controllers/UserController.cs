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
        public ActionResult<UserProfileRunsResponse> GetUserProfileRuns(string username)
        {
            var response = _userService.GetUserProfileRuns(username);
            return Ok(response);
        }

        [HttpGet("getdata/{username}")]
        public ActionResult<UserDataResponse> GetUserData(string username)
        {
            var response = _userService.GetUserData(username);
            return Ok(response);
        }

        [Authorize]
        [HttpPut("update/avatar")]
        public IActionResult UpdateUserAvatar(NewAvatarDto request)
        {
            var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
            if (userIdentity == null)
                return Unauthorized("Could not recognize user identity");

            _userService.UpdateUserAvatar(request, userIdentity);
            return Ok(new { message = "Avatar updated" });
        }

        [Authorize]
        [HttpPut("update/socials")]
        public IActionResult UpdateUserSocials(NewSocialsDto request)
        {
            var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
            if (userIdentity == null)
                return Unauthorized("Could not recognize user identity");

            _userService.UpdateUserSocials(request, userIdentity);
            return Ok(new { message = "Socials updated" });
        }

        [Authorize]
        [HttpPut("update/pronouns")]
        public IActionResult UpdateUserPronouns(NewPronounsDto request)
        {
            var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
            if (userIdentity == null)
                return Unauthorized("Could not recognize user identity");

            _userService.UpdateUserPronouns(request, userIdentity);
            return Ok(new { message = "Pronouns updated" });
        }

        [Authorize]
        [HttpPut("update/location")]
        public IActionResult UpdateUserLocation(NewLocationDto request)
        {
            var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
            if (userIdentity == null)
                return Unauthorized("Could not recognize user identity");

            _userService.UpdateUserLocation(request, userIdentity);
            return Ok(new { message = "Location updated" });
        }
    }
}