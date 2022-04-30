using FullRuns.DB;
using HatCommunityWebsite.API.Dtos;
using HatCommunityWebsite.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HatCommunityWebsite.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameController : Controller
    {

        private readonly ILogger<RunController> _logger;
        private readonly AppDbContext _context;

        public GameController(AppDbContext context, ILogger<RunController> logger)
        {
            _logger = logger;
            _context = context;
        }

        [HttpPost("creategame")]
        public async Task<ActionResult<Game>> CreateGame(GameDto request)
        {
            var newGame = new Game
            {
                Categories = null,
                Runs = null,
                Variables = null,
                Name = request.Name,
                Acronym = request.Acronym,
            };

            _context.Games.Add(newGame);
            await _context.SaveChangesAsync();

            return Ok("Game created.");
        }

        [HttpGet("getgame/{gameId}")]
        public async Task<ActionResult<Game>> GetGame(int gameId)
        {
            var game = await _context.Games
                .Include(c => c.Categories)
                .Include(sc => sc.Subcategories)
                .FirstOrDefaultAsync(x => x.Id == gameId);

            if (game == null)
                return NotFound();

            return game;
        }

        [HttpGet("getgamebyacronym/{gameId}")]
        public async Task<ActionResult<Game>> GetGame(string gameId)
        {
            var game = await _context.Games
                .Include(c => c.Categories)
                .Include(sc => sc.Subcategories)
                .FirstOrDefaultAsync(x => x.Acronym == gameId);

            if (game == null)
                return NotFound();

            return game;
        }

        [HttpGet("getall")]
        public async Task<ActionResult<List<Game>>> GetGames()
        {
            var games = await _context.Games
                .Include(x => x.Categories)
                .ToListAsync();

            return games;
        }

        [HttpGet("getgamevariables/{gameId}")]
        public async Task<ActionResult<List<Variable>>> GetGameVariables(int gameId)
        {
            var variables = await _context.Variables
                .Where(x => x.GameId == gameId)
                .Include(g => g.Game)
                .ToListAsync();

            return variables;
        }

        [HttpGet("getdistinctvarnames/{gameId}")]
        public async Task<ActionResult<List<string>>> GetDistinctVarNames(string gameId)
        {
            var variables = await _context.Variables
                .Where(x => x.Game.Acronym == gameId)
                .Select(x => x.Name)
                .ToListAsync();

            var distinctList = variables.Distinct().ToList();

            return distinctList;
        }

        [HttpGet("getvariablesbygamename/{gameId}")]
        public async Task<ActionResult<List<Variable>>> GetVariablesByGameName(string gameId)
        {
            var variables = await _context.Variables
                .Where(x => x.Game.Acronym == gameId)
                .ToListAsync();

            return variables;
        }
    }
}
