using HatCommunityWebsite.Service;
using HatCommunityWebsite.Service.Dtos;
using HatCommunityWebsite.Service.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> Register(UserDto request)
        {
            await _accountService.Register(request, Request.Headers["origin"]);
            return Ok(new { message = "Registration successful, please check your email for verification instructions" });
        }

        [AllowAnonymous]
        [HttpPost("verify-account")]
        public async Task<IActionResult> VerifyAccount(VerifyUserDto request)
        {
            await _accountService.VerifyEmail(request.Token);
            return Ok(new { message = "Verification successful, you can now login" });
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<ActionResult<AuthenticateResponse>> Authenticate(LogInDto request)
        {
            var response = await _accountService.Authenticate(request, ipAddress());
            setRefreshTokenCookie(response.RefreshToken.Token, response.RefreshToken.Expires);
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthenticateResponse>> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var response = await _accountService.RefreshToken(refreshToken, ipAddress());
            setRefreshTokenCookie(response.RefreshToken.Token, response.RefreshToken.Expires);
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<ActionResult<AuthenticateResponse>> LogOut()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            await _accountService.Logout(refreshToken);
            return Ok(new { message = "User logged out" });
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto request)
        {
            await _accountService.ForgotPassword(request, Request.Headers["origin"]);
            return Ok(new { message = "Please check your email for password reset instructions" });
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto request)
        {
            await _accountService.ResetPassword(request);
            return Ok(new { message = "Password reset successful, you can now login" });
        }

        [AllowAnonymous]
        [HttpPost("validate-reset-password")]
        public async Task<IActionResult> ValidateResetPassword(ValidateResetPasswordDto request)
        {
            await _accountService.ValidateResetPassword(request);
            return Ok(new { message = "Reset password token valid" });
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