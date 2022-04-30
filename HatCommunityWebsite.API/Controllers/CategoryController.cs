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
    public class CategoryController : Controller
    {
        private readonly ILogger<RunController> _logger;
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context, ILogger<RunController> logger)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet("getcategories/{gameId}")]
        public async Task<ActionResult<List<Category>>> GetCategoriesByGame(int gameId)
        {
            var categories = await _context.Categories
                .Where(x => x.GameId == gameId)
                .Include(x => x.Game)
                .ToListAsync();

            return categories;
        }

        [HttpPost("createcategories")]
        public async Task<ActionResult<List<Category>>> CreateCategories(List<CategoryDto> request)
        {
            foreach (var category in request)
            {
                var game = await _context.Games.FindAsync(category.GameId);
                if (game == null)
                    return NotFound();

                var newCategory = new Category
                {
                    Name = category.Name,
                    GameId = game.Id,
                    IsLevel = category.IsLevel,
                };

                _context.Categories.Add(newCategory);
            }

            await _context.SaveChangesAsync();
            return Ok("Categories created.");
        }

        [HttpGet("getlevels/{gameAcronym}")]
        public async Task<ActionResult<List<Category>>> GetIndividualLevels(string gameAcronym)
        {
            var categories = await _context.Categories
                .Where(x => x.Game.Acronym == gameAcronym)
                .Where(l => l.IsLevel == true)
                .ToListAsync();

            return categories;
        }

        [HttpGet("getcategoriesbyacronym/{gameAcronym}")]
        public async Task<ActionResult<List<Category>>> GetCategoriesByGameAcronym(string gameAcronym)
        {
            var categories = await _context.Categories
                .Where(x => x.Game.Acronym == gameAcronym)
                .Include(x => x.Game)
                .Include(sc => sc.SubCategories)
                .ToListAsync();

            return categories;
        }
        [HttpGet("getall")]
        public async Task<ActionResult<List<Category>>> GetCategories()
        {
            var categories = await _context.Categories
                .ToListAsync();

            return categories;
        }
    }
}
