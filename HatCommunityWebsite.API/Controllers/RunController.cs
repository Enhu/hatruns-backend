using FullRuns.DB;
using HatCommunityWebsite.API.Dtos;
using HatCommunityWebsite.Core;
using HatCommunityWebsite.DB;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace HatCommunityWebsite.API.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class RunController : ControllerBase
    {
        public enum Status
        {
            Pending = 0,
            Verified = 1,
            Rejected = 2
        }

        private readonly ILogger<RunController> _logger;
        private readonly AppDbContext _context;

        public RunController(AppDbContext context, ILogger<RunController> logger)
        {
            _logger = logger;
            _context = context;
        }     

        [HttpPost("submit"), Authorize]
        public async Task<ActionResult<Submission>> QueueRun(RunDto request)
        {
            var game = await _context.Games.FindAsync(request.GameId);
            if (game == null)
                return NotFound();

            var category = await _context.Categories.FindAsync(request.CategoryId);
            if (category == null)
                return NotFound();

            var user = await _context.Users.Where(x => x.Username == request.PlayerName).FirstOrDefaultAsync();
            if (user == null)
                return NotFound();

            var subCategory = await _context.Subcategories.FindAsync(request.SubcategoryId);
            if (subCategory == null && request.SubcategoryId != 0)
                return NotFound();

            var newSubmission = new Submission
            {
                PlayerName = request.PlayerName,
                GameId = request.GameId,
                CategoryId = request.CategoryId,
                Description = request.Description,
                Platform = request.Platform,
                CategoryName = category.Name,
                GameName = game.Name,
                SubcategoryName = subCategory?.Name,
                UserId = user?.Id,
                Time = request.Time,
                VideoLinks = request.VideoLinks,
                Date = request.Date,
                SubmittedDate = DateTime.UtcNow,
                SubmittedBy = request.SubmittedBy,
                Status = (int)Status.Pending,
                SubcategoryId = request.SubcategoryId,
                Variables = request.Variables,
            };

            _context.Submissions.Add(newSubmission);
            await _context.SaveChangesAsync();

            return Ok("Submission created.");
        }

        [HttpPost("verify"), Authorize]
        public async Task<ActionResult<Run>> VerifyRun(VerifyRunDto request)
        {
            var submission = await _context.Submissions.FindAsync(request.SubmissionId);
            if (submission == null)
                return NotFound();

            var game = await _context.Games.FindAsync(submission.GameId);
            if (game == null)
                return NotFound();

            var category = await _context.Categories.FindAsync(submission.CategoryId);
            if (category == null)
                return NotFound();

            var user = await _context.Users.Where(x => x.Username == submission.PlayerName).FirstOrDefaultAsync();
            if (user == null)
                return NotFound();

            var subCategory = await _context.Subcategories.FindAsync(submission.SubcategoryId);
            if (subCategory == null && submission.SubcategoryId != 0)
                return NotFound();

            submission.Status = (int)Status.Verified;

            var newRun = new Run
            {
                PlayerName = submission.PlayerName,
                Game = game,
                User = user,
                Category = category,
                Description = submission.Description,
                Platform = submission.Platform,
                Time = submission.Time,
                VideoLinks = submission.VideoLinks,
                Date = submission.Date,
                SubmittedBy = submission.SubmittedBy,
                SubCategory = subCategory,
                SubmissionId = submission.Id,
                VerifiedBy = request.VerifierName,
                VerifiedDate = DateTime.UtcNow
            };

            if (submission.Variables != null) //remove this later? maybe just use json formatted strings?
            {
                var variables = JsonConvert.DeserializeObject<List<Variable>>(submission.Variables);

                foreach (var item in variables)
                {
                    var variable = await _context.Variables
                        .FirstAsync(v => v.Name == item.Name && v.Id == item.Id);

                    if (variable != null)
                    {
                        var runvarRelationship = new RunVariable() { AssociatedRun = newRun, AssociatedVariable = variable };
                        _context.RunVariables.Add(runvarRelationship);
                    }
                }
            }

            var currentVerifiedRun = new Run();

            if (submission.SubcategoryId != 0)
            {
                currentVerifiedRun = _context.Runs
                    .Where(p => p.PlayerName == submission.PlayerName)
                    .Where(c => c.CategoryId == submission.CategoryId)
                    .Where(sc => sc.SubcategoryId == submission.SubcategoryId)
                    .Where(o => o.IsObsolete == false)
                    .OrderBy(x => x.Time).FirstOrDefault();
            }
            else
            {
                currentVerifiedRun = _context.Runs
                    .Where(p => p.PlayerName == submission.PlayerName)
                    .Where(c => c.CategoryId == submission.CategoryId)
                    .Where(o => o.IsObsolete == false)
                    .OrderBy(x => x.Time).FirstOrDefault();
            }

            if(currentVerifiedRun != null)
            {
                if (currentVerifiedRun.Time < newRun.Time)
                {
                    newRun.IsObsolete = true;
                }
                else
                {
                    newRun.IsObsolete = false;
                    currentVerifiedRun.IsObsolete = true;
                }
            }

            _context.Runs.Add(newRun);
            _context.SaveChanges();

            return Ok("Run verified");
        }

        [HttpPost("reject/{submissionId}"), Authorize]
        public async Task<ActionResult<string>> RejectRun(int submissionId)
        {
            var submission = await _context.Submissions.FindAsync(submissionId);
            if (submission == null)
                return NotFound();

            var leaderboardRun = await _context.Runs.Where(x => x.SubmissionId == submissionId).FirstOrDefaultAsync();

            if(leaderboardRun != null)
               _context.Runs.Remove(leaderboardRun);

            submission.Status = (int)Status.Rejected;
            _context.SaveChanges();

            return Ok("Run rejected");
        }

        [HttpPost("importruns"), Authorize] //for imports/admins only
        public async Task<ActionResult<List<Run>>> CreateRuns(List<RunDto> request)
        {
            foreach (var run in request)
            {
                SubCategory? subCategory = null;

                var game = await _context.Games.FindAsync(run.GameId);
                if (game == null)
                    return NotFound("Game not found.");

                var category = await _context.Categories.FindAsync(run.CategoryId);
                if (category == null)
                    return NotFound("Category not found");

                if (run.SubcategoryId != 0)
                {
                    subCategory = await _context.Subcategories.FindAsync(run.SubcategoryId);
                    if (subCategory == null)
                        return NotFound("Sub Category not found.");
                }

                var newRun = new Run
                {
                    PlayerName = run.PlayerName,
                    Game = game,
                    Category = category,
                    SubCategory = subCategory,
                    Description = run.Description,
                    Platform = run.Platform,
                    Time = run.Time,
                    VideoLinks = run.VideoLinks,
                    Date = run.Date,
                    SubmittedBy = run.SubmittedBy,
                    IsObsolete = run.IsObsolete.HasValue,
                };

                if (run.Variables != null)
                {
                    var variables = JsonConvert.DeserializeObject<List<Variable>>(run.Variables);

                    foreach (var item in variables)
                    {
                        if (string.IsNullOrEmpty(item.Value))
                            continue;

                        var variable = await _context.Variables
                            .FirstAsync(v => v.Name == item.Name && v.Value == item.Value);

                        if (variable != null)
                        {
                            var runvarRelationship = new RunVariable() { AssociatedRun = newRun, AssociatedVariable = variable };
                            _context.RunVariables.Add(runvarRelationship);
                        }
                    }
                }

                _context.Runs.Add(newRun);
            }

            await _context.SaveChangesAsync();
            return Ok("Runs imported.");
        }

        [HttpGet("getrun/{runId}")]
        public async Task<ActionResult<Run>> GetRun(int runId)
        {
            var run = await _context.Runs
                .Include(c => c.Category)
                .Include(sc => sc.SubCategory)
                .Include(g => g.Game)
                .FirstOrDefaultAsync(x => x.Id == runId);

            if (run == null) 
                return NotFound();

            return run;
        }

        [HttpGet("getrunplace/{runId}")] //optimize
        public async Task<ActionResult<string>> GetRunPlace(int runId)
        {
            var runs = new List<Run>();

            var run = await _context.Runs.FirstOrDefaultAsync(r => r.Id == runId);

            if (run == null)
                return NotFound();

            if (run.IsObsolete)
                return string.Empty;
            
            runs = await _context.Runs
                .Where(x => x.IsObsolete == false)
                .Where(x => x.CategoryId == run.CategoryId)
                .Where(x => x.SubcategoryId == run.SubcategoryId)
                .ToListAsync();

            var orderedRuns = runs.OrderBy(x => x.Time).ToList();

            return (orderedRuns.FindIndex(x => x.Id == runId) + 1).ToString();
        }

        [HttpGet("getsubmissionplace/{submissionId}")] //optimize
        public async Task<ActionResult<string>> GetSubmissionPlace(int submissionId)
        {
            var run = new Run();

            var submission = await _context.Submissions.FirstOrDefaultAsync(r => r.Id == submissionId);

            if (submission == null)
                return NotFound();


            if (submission.SubcategoryId != 0)
            {
                run = await _context.Runs
                    .Where(x => x.IsObsolete == false)
                    .Where(x => x.CategoryId == submission.CategoryId)
                    .Where(x => x.SubcategoryId == submission.SubcategoryId)
                    .OrderBy(x => x.Time)
                    .FirstOrDefaultAsync();

            }
            else
            {
                run = await _context.Runs
                    .Where(x => x.IsObsolete == false)
                    .Where(x => x.CategoryId == submission.CategoryId)
                    .OrderBy(x => x.Time)
                    .FirstOrDefaultAsync();
            }

            if (run != null && run.Time < submission.Time)
                return "(Obsolete)";

            var runTimes = new List<double>();

            if (submission.SubcategoryId != 0)
            {
                runTimes = await _context.Runs
                    .Where(x => x.IsObsolete == false)
                    .Where(x => x.CategoryId == submission.CategoryId)
                    .Where(x => x.SubcategoryId == submission.SubcategoryId)
                    .OrderBy(x => x.Time)
                    .Select(x => x.Time)
                    .ToListAsync();

            }
            else
            {
                runTimes = await _context.Runs
                    .Where(x => x.IsObsolete == false)
                    .Where(x => x.CategoryId == submission.CategoryId)
                    .OrderBy(x => x.Time)
                    .Select(x => x.Time)
                    .ToListAsync();
            }

            runTimes.Add(submission.Time);

            var orderedRuns = runTimes.OrderBy(x => x).ToList();

            return (orderedRuns.FindIndex(x => x == submission.Time) + 1).ToString();
        }

        [HttpGet("getruns/{gameName}")]
        public async Task<ActionResult<List<Run>>> GetRunsByGame(string gameName)
        {
            var runs = await _context.Runs
                .Where(x => x.Game.Acronym == gameName)
                .Include(x => x.Category)
                .Include(x => x.Game)
                .Include(sc => sc.SubCategory)
                .Include(v => v.RunVariables)
                .ThenInclude(v => v.AssociatedVariable)
                .ToListAsync();

            return runs;
        }

        [HttpGet("getsubmissions")]
        public async Task<ActionResult<List<Submission>>> GetSubmissions()
        {
            var submissions = await _context.Submissions.ToListAsync();

            return submissions;
        }

        [HttpGet("getsubmission/{submissionId}")]
        public async Task<ActionResult<Submission>> GetSubmission(int submissionId)
        {
            var submission = await _context.Submissions.FindAsync(submissionId);
            if (submission == null)
                return NotFound();

            return submission;
        }


        [HttpGet("getlevelruns/{levelId}")]
        public async Task<ActionResult<List<Run>>> GetIndividualLevelRuns(int levelId)
        {
            var runs = await _context.Runs
                .Where(x => x.CategoryId == levelId && x.Category.IsLevel == true)
                .Include(g => g.Game)
                .ToListAsync();

            return runs;
        }

        [HttpGet("getplatforms")]
        public async Task<ActionResult<List<Platform>>> GetPlatforms()
        {
            var platforms = await _context.Platforms.ToListAsync();

            return platforms;
        }

        [HttpGet("getsubmissionscount")]
        public async Task<ActionResult<int>> GetSubmissionsCount()
        {
            var submissions = await _context.Submissions.Where(x => x.Status == (int)Status.Pending).ToListAsync();

            return submissions.Count;
        }

        [HttpDelete("delete/{runId}"), Authorize]
        public async Task<ActionResult<Run>> DeleteRun(int runId)
        {
            var run = await _context.Runs.FindAsync(runId);

            if (run == null)
                return NotFound();

            var submission = await _context.Submissions.FindAsync(run.SubmissionId);

            if(submission != null)
                _context.Submissions.Remove(submission);

            _context.Runs.Remove(run);
            _context.SaveChanges();

            return Ok("Run deleted.");
        }

        //fix this later

        //[HttpDelete("deleterun/{runId}")]
        //public IActionResult DeleteRun(int runId)
        //{
        //    _hatCommunityWebsiteServices.DeleteFullGameRun(runId);
        //    return Ok();
        //}        

        //[HttpPut("editrun/{id}")] //fix
        //public IActionResult EditRun([FromBody] FullGameRun run)
        //{
        //    _hatCommunityWebsiteServices.EditFullGameRun(run);
        //    return Ok();
        //}
    }
}