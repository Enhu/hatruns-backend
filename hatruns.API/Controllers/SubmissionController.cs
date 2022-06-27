using HatCommunityWebsite.Service;
using HatCommunityWebsite.Service.Dtos;
using HatCommunityWebsite.Service.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HatCommunityWebsite.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SubmissionController : Controller
    {
        private readonly ISubmissionService _submissionService;

        public SubmissionController(ISubmissionService submissionService)
        {
            _submissionService = submissionService;
        }

        [Authorize]
        [HttpPost("submit")]
        public async Task<IActionResult> Submit(SubmissionDto request)
        {
            var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
            if (userIdentity == null)
                return Unauthorized("Could not recognize user identity");

            await _submissionService.Submit(request, userIdentity);
            return Ok(new { message = "Run successfully submitted" });
        }

        [Authorize]
        [HttpDelete("delete/{runId}")]
        public async Task<IActionResult> DeleteSubmission(int runId)
        {
            var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
            if (userIdentity == null)
                return Unauthorized("Could not recognize user identity");

            await _submissionService.DeleteSubmission(runId, userIdentity);
            return Ok(new { message = "Run successfully deleted" });
        }

        [Authorize(Policy = "Admin")]
        [HttpPut("verify")]
        public async Task<IActionResult> VerifySubmission(VerifySubmissionDto request)
        {
            var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
            if (userIdentity == null)
                return Unauthorized("Could not recognize user identity");

            await _submissionService.VerifySubmission(request, userIdentity);
            return Ok(new { message = "Run successfully verified" });
        }

        [Authorize(Policy = "Admin")]
        [HttpPut("reject")]
        public async Task<IActionResult> RejectSubmission(RejectSubmissionDto request)
        {
            var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
            if (userIdentity == null)
                return Unauthorized("Could not recognize user identity");

            await _submissionService.RejectSubmission(request, userIdentity);
            return Ok(new { message = "Run successfully rejected" });
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateSubmission(SubmissionDto request)
        {
            var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
            if (userIdentity == null)
                return Unauthorized("Could not recognize user identity");

            await _submissionService.UpdateSubmission(request, userIdentity);
            return Ok(new { message = "Run successfully updated" });
        }

        [AllowAnonymous]
        [HttpGet("get/{runId}")]
        public async Task<ActionResult<SubmissionResponse>> GetSubmission(int runId)
        {
            var response = await _submissionService.GetSubmission(runId);
            return Ok(response);
        }

        [Authorize(Policy = "Admin")]
        [HttpPost("import")]
        public async Task<IActionResult> ImportSubmissions(List<ImportDto> request)
        {
            var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
            if (userIdentity == null)
                return Unauthorized("Could not recognize user identity");

            await _submissionService.ImportSubmissions(request, userIdentity);
            return Ok(new { message = "Runs successfully imported. Users created accordingly" });
        }
    }
}