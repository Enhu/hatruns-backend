using HatCommunityWebsite.DB;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Security.Cryptography;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using HatCommunityWebsite.DB;
using Microsoft.EntityFrameworkCore;
using HatCommunityWebsite.Service.Responses;
using HatCommunityWebsite.Service;
using HatCommunityWebsite.Service.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace HatCommunityWebsite.API.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _accountService;

        public AuthController(IAuthService accountService)
        {
            _accountService = accountService;

         }
        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register(UserDto request)
        {
            _accountService.Register(request, Request.Headers["origin"]);
            return Ok(new { message = "Registration successful, please check your email for verification instructions" });
        }
        [AllowAnonymous]
        [HttpPost("verify-account")]
        public IActionResult VerifyAccount(VerifyUserDto request)
        {
            _accountService.VerifyEmail(request.Token);
            return Ok(new { message = "Verification successful, you can now login" });
        }
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public ActionResult<AuthenticateResponse> Authenticate(HatCommunityWebsite.Service.Dtos.LogInDto request)
        {
            var response = _accountService.Authenticate(request, ipAddress());
            setRefreshTokenCookie(response.RefreshToken.Token, response.RefreshToken.Expires);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("refresh-token")]
        public ActionResult<AuthenticateResponse> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var response = _accountService.RefreshToken(refreshToken, ipAddress());
            setRefreshTokenCookie(response.RefreshToken.Token, response.RefreshToken.Expires);
            return Ok(response);
        }
        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword(ForgotPasswordDto request)
        {
            _accountService.ForgotPassword(request, Request.Headers["origin"]);
            return Ok(new { message = "Please check your email for password reset instructions" });
        }
        [AllowAnonymous]
        [HttpPost("reset-password")]
        public IActionResult ResetPassword(ResetPasswordDto request)
        {
            _accountService.ResetPassword(request);
            return Ok(new { message = "Password reset successful, you can now login" });
        }

        private void setRefreshTokenCookie(string token, DateTime expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expires
            };
            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        private string ipAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
    }
}
