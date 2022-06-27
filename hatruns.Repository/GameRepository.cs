using HatCommunityWebsite.DB;
using Microsoft.EntityFrameworkCore;

namespace HatCommunityWebsite.Repo
{
    public interface IGameRepository
    {
        Task<Game> GetGameById(int id);

        Task<List<Game>> GetAllGames();

        Task<Game> GetGameByIdIncludeAll(int id);

        Task<Game> GetGameByAcronymIncludeAll(string id);

        Task UpdateGame(Game game);
    }

    public class GameRepository : IGameRepository
    {
        private readonly AppDbContext _context;

        public GameRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Game> GetGameById(int id)
        {
            return await _context.Games.FindAsync(id);
        }

        public async Task<Game> GetGameByIdIncludeAll(int id)
        {
            return await _context.Games
                .Include(x => x.Categories)
                .ThenInclude(x => x.Subcategories)
                .Include(x => x.Variables)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Game> GetGameByAcronymIncludeAll(string id)
        {
            return await _context.Games
                .Include(x => x.Categories)
                .ThenInclude(x => x.Subcategories)
                .Include(x => x.Variables)
                .Include(x => x.Levels)
                .ThenInclude(x => x.Categories)
                .FirstOrDefaultAsync(x => x.Acronym == id);
        }

        public async Task<List<Game>> GetAllGames()
        {
            return await _context.Games.ToListAsync();
        }

        public async Task UpdateGame(Game game)
        {
            _context.Games.Update(game);
            await _context.SaveChangesAsync();
        }
    }
}