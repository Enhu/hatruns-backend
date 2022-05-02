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
        public async Task<ActionResult<Run>> SubmitRun(RunDto request)
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

            var newRun = new Run
            {
                PlayerName = request.PlayerName,
                Game = game,
                SubCategory = subCategory,
                Category = category,
                Description = request.Description,
                Platform = request.Platform,
                UserId = user?.Id,
                Time = request.Time,
                VideoLinks = request.VideoLinks,
                Date = request.Date,
                SubmittedDate = DateTime.UtcNow,
                SubmittedBy = request.SubmittedBy,
                SubcategoryId = request.SubcategoryId,
                Status = (int)Status.Pending,
            };

            await GetRunVariables(request, newRun);

            if (request.AutoVerify)
            {
                var hasSubcategory = newRun.SubCategory != null;
                await UpdateCurrentVerifiedRun(false, hasSubcategory, newRun);
                newRun.Status = (int)Status.Verified;
            }
                
            _context.Runs.Add(newRun);
            await _context.SaveChangesAsync();

            return Ok("Run created.");
        }

        [HttpPut("verify"), Authorize]
        public async Task<ActionResult<Run>> VerifyRun(VerifyDto request)
        {
            var run = await _context.Runs.FindAsync(request.RunId);
            if (run == null)
                return NotFound();

            run.Status = (int)Status.Verified;
            run.VerifiedBy = request.ModName;
            run.VerifiedDate = DateTime.UtcNow;

            //makes current board run obsolete
            var hasSubcategory = run.SubCategory != null;

            await UpdateCurrentVerifiedRun(false, hasSubcategory, run);

            _context.Runs.Update(run);
            _context.SaveChanges();

            return Ok("Run verified");
        }

        [HttpPut("reject"), Authorize]
        public async Task<ActionResult<string>> RejectRun(RejectDto request)
        {
            var run = await _context.Runs.FindAsync(request.RunId);
            if (run == null)
                return NotFound();

            run.Status = (int)Status.Rejected;
            run.RejectedDate = DateTime.UtcNow;
            run.RejectedBy = request.ModName;
            run.RejectedReason = request.RejectedReason;

            run.VerifiedBy = null;
            run.VerifiedDate = null;

            var hasSubcategory = run.SubCategory != null;

            //make last obsolete run visible
            await UpdateCurrentVerifiedRun(true, hasSubcategory, run);

            _context.Runs.Update(run);
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
                .Include(rv => rv.RunVariables)
                .ThenInclude(v => v.AssociatedVariable)
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

            if (run.IsObsolete && run.Status == (int)Status.Verified)
                return string.Empty;

            if (run.IsObsolete && run.Status == (int)Status.Rejected)
                return string.Empty;

            if (run.Status == (int)Status.Pending)
                return await GetPendingRunPlace(run);

            runs = await _context.Runs
                .Where(x => x.IsObsolete == false)
                .Where(x => x.CategoryId == run.CategoryId)
                .Where(x => x.SubcategoryId == run.SubcategoryId)
                .ToListAsync();

            var orderedRuns = runs.OrderBy(x => x.Time).ToList();

            return (orderedRuns.FindIndex(x => x.Id == runId) + 1).ToString();
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

        [HttpGet("getall")]
        public async Task<ActionResult<List<Run>>> GetAllRuns()
        {
            var runs = await _context.Runs
                .Include(c => c.Category)
                .Include(sc => sc.SubCategory)
                .Include(g => g.Game)
                .Include(rv => rv.RunVariables)
                .ThenInclude(v => v.AssociatedVariable)
                .ToListAsync();

            return runs;
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

        [HttpGet("getpendingruns")]
        public async Task<ActionResult<List<Run>>> GetPendingRuns()
        {
            var runs = await _context.Runs.Where(x => x.Status == (int)Status.Pending).ToListAsync();

            return runs;
        }

        [HttpGet("getpendingrunscount")]
        public async Task<ActionResult<int>> GetPendingRunsCount()
        {
            var runs = await _context.Runs.Where(x => x.Status == (int)Status.Pending).ToListAsync();

            return runs.Count;
        }

        [HttpDelete("delete/{runId}"), Authorize]
        public async Task<ActionResult<Run>> DeleteRun(int runId)
        {
            var run = await _context.Runs.FindAsync(runId);

            if (run == null)
                return NotFound();

            _context.Runs.Remove(run);
            _context.SaveChanges();

            return Ok("Run deleted.");
        }

        [HttpPut("update"), Authorize]
        public async Task<ActionResult<Run>> UpdateRun(EditRunDto request)
        {
            var run = await _context.Runs
                .Include(x=> x.RunVariables)
                .FirstOrDefaultAsync(i => i.Id == request.RunId);

            if (run == null)
                return NotFound();

            if (request.SubcategoryId != run.SubcategoryId && request.SubcategoryId != 0)
                run.SubCategory = await _context.Subcategories.FindAsync(request.SubcategoryId);

            run.Date = request.Date;
            run.Platform = request.Platform;
            run.VideoLinks = request.VideoLinks;
            run.Time = request.Time;
            run.Status = (int)Status.Pending;
            run.Description = request.Description;

            if (request.Variables != null) //makes this spaghetti better
            {
                var variables = JsonConvert.DeserializeObject<List<Variable>>(request.Variables);

                foreach (var item in variables) //THIS CAN BE IMPROVED
                {
                    var variable = await _context.Variables
                        .FirstAsync(v => v.Name == item.Name && v.Id == item.Id);

                    var runVar = run.RunVariables.FirstOrDefault(x => x.VariableId == item.Id);
                    if (runVar != null)
                    {
                        run.RunVariables.Remove(runVar);
                    }

                    if (variable != null)
                    {
                        run.RunVariables.Add(new RunVariable
                        {
                            AssociatedRun = run,
                            AssociatedVariable = variable
                        });
                    }
                }
            }

            if (request.AutoVerify)
            {
                run.Status = (int)Status.Verified;
            }
            else
            {
                if(run.Status == (int)Status.Verified)
                {
                    var hasSubcategory = run.SubCategory != null;
                    await UpdateCurrentVerifiedRun(true, hasSubcategory, run);
                    run.Status = (int)Status.Pending;
                }
            }

            _context.Update(run);
            _context.SaveChanges();

            return Ok("Run updated");
        }

        private async Task UpdateCurrentVerifiedRun(bool isRejection, bool hasSubcategory, Run run) //make this spaghetti better
        {
            if (isRejection)
            {

                var lastVerifiedRun = new Run();

                if (hasSubcategory)
                {
                    lastVerifiedRun = await _context.Runs
                        .Where(p => p.PlayerName == run.PlayerName)
                        .Where(c => c.CategoryId == run.CategoryId)
                        .Where(sc => sc.SubcategoryId == run.SubcategoryId)
                        .Where(o => o.IsObsolete == true)
                        .OrderBy(x => x.Time).FirstOrDefaultAsync();
                }
                else
                {
                    lastVerifiedRun = await _context.Runs
                        .Where(p => p.PlayerName == run.PlayerName)
                        .Where(c => c.CategoryId == run.CategoryId)
                        .Where(o => o.IsObsolete == true)
                        .OrderBy(x => x.Time).FirstOrDefaultAsync();
                }

                if(lastVerifiedRun != null)
                {
                    lastVerifiedRun.IsObsolete = false;
                    _context.Runs.Update(lastVerifiedRun);
                }

                return;
            }

            var currentVerifiedRun = new Run();

            if (hasSubcategory)
            {
                currentVerifiedRun = await _context.Runs
                    .Where(p => p.PlayerName == run.PlayerName)
                    .Where(c => c.CategoryId == run.CategoryId)
                    .Where(sc => sc.SubcategoryId == run.SubcategoryId)
                    .Where(o => o.IsObsolete == false)
                    .OrderBy(x => x.Time).FirstOrDefaultAsync();
            }
            else
            {
                currentVerifiedRun = await _context.Runs
                    .Where(p => p.PlayerName == run.PlayerName)
                    .Where(c => c.CategoryId == run.CategoryId)
                    .Where(o => o.IsObsolete == false)
                    .OrderBy(x => x.Time).FirstOrDefaultAsync();
            }

            if (currentVerifiedRun != null)
            {
                if (currentVerifiedRun.Time < run.Time)
                {
                    run.IsObsolete = true;
                }
                else
                {
                    run.IsObsolete = false;
                    currentVerifiedRun.IsObsolete = true;
                    _context.Runs.Update(currentVerifiedRun);
                }
            }
            else
            {
                run.IsObsolete = false;
            }
        }

        private async Task GetRunVariables(RunDto request, Run newRun)
        {
            if (request.Variables != null) //makes this spaghetti better
            {
                var variables = JsonConvert.DeserializeObject<List<Variable>>(request.Variables);

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
        }
        private async Task<ActionResult<string>> GetPendingRunPlace(Run? pendingRun)
        {
            var currentVerifiedRun = new Run();

            if (pendingRun.SubcategoryId != null && pendingRun.SubcategoryId != 0)
            {
                currentVerifiedRun = await _context.Runs
                    .Where(x => x.IsObsolete == false)
                    .Where(x => x.PlayerName == pendingRun.PlayerName)
                    .Where(x => x.CategoryId == pendingRun.CategoryId)
                    .Where(x => x.SubcategoryId == pendingRun.SubcategoryId)
                    .OrderBy(x => x.Time)
                    .FirstOrDefaultAsync();

            }
            else
            {
                currentVerifiedRun = await _context.Runs
                    .Where(x => x.IsObsolete == false)
                    .Where(x => x.PlayerName == pendingRun.PlayerName)
                    .Where(x => x.CategoryId == pendingRun.CategoryId)
                    .OrderBy(x => x.Time)
                    .FirstOrDefaultAsync();
            }

            if (currentVerifiedRun != null && currentVerifiedRun.Time < pendingRun.Time)
                return "(Obsolete)";

            var runTimes = new List<double>();

            if (pendingRun.SubcategoryId != null && pendingRun.SubcategoryId != 0)
            {
                runTimes = await _context.Runs
                    .Where(x => x.IsObsolete == false)
                    .Where(x => x.CategoryId == pendingRun.CategoryId)
                    .Where(x => x.SubcategoryId == pendingRun.SubcategoryId)
                    .OrderBy(x => x.Time)
                    .Select(x => x.Time)
                    .ToListAsync();

            }
            else
            {
                runTimes = await _context.Runs
                    .Where(x => x.IsObsolete == false)
                    .Where(x => x.CategoryId == pendingRun.CategoryId)
                    .OrderBy(x => x.Time)
                    .Select(x => x.Time)
                    .ToListAsync();
            }

            runTimes.Add(pendingRun.Time);

            var orderedRuns = runTimes.OrderBy(x => x).ToList();

            return (orderedRuns.FindIndex(x => x == pendingRun.Time) + 1).ToString();
        }
    }
}