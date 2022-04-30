using FullRuns.DB;
using HatCommunityWebsite.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HatCommunityWebsite.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SubcategoryController : Controller
    {
        private readonly ILogger<RunController> _logger;
        private readonly AppDbContext _context;
        public SubcategoryController(AppDbContext context, ILogger<RunController> logger)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet("getsubcategoriesbygame/{gameId}")]
        public async Task<ActionResult<List<SubCategory>>> GetSubcategoriesByGame(int gameId)
        {
            var subcats = await _context.Subcategories
                .Where(x => x.GameId == gameId)
                .ToListAsync();

            return subcats;
        }

        [HttpGet("getsubcategoriesbygameacronym/{gameId}")]
        public async Task<ActionResult<List<SubCategory>>> GetSubcategoriesByGame(string gameId)
        {
            var subcats = await _context.Subcategories
                .Where(x => x.Game.Acronym == gameId)
                .ToListAsync();

            return subcats;
        }

        [HttpGet("getsubcategoriesbycategory/{categoryId}")]
        public async Task<ActionResult<List<SubCategory>>> GetSubcategoriesByCategory(int categoryId)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null)
                return NotFound("Category not found");

            var subcats = await _context.Subcategories
                .Where(x => x.Category.FirstOrDefault(c => c.Id == categoryId).Id == categoryId)
                .ToListAsync();

            return subcats;
        }
        [HttpGet("getall")]
        public async Task<ActionResult<List<SubCategory>>> GetCategories()
        {
            var subcategories = await _context.Subcategories
                .ToListAsync();

            return subcategories;
        }
    }
}
