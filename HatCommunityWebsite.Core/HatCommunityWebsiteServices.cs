using FullRuns.DB;
using HatCommunityWebsite.DB;

namespace HatCommunityWebsite.Core
{
    public class HatCommunityWebsiteServices : IHatCommunityWebsiteServices
    {
        private AppDbContext _context;
        public HatCommunityWebsiteServices(AppDbContext context)
        {
            _context = context;
        }

        public Run CreateFullGameRun(Run run)
        {
            _context.Runs.Add(run);
            _context.SaveChanges();

            return run;
        }            
        
        public void CreateMultipleRuns(List<Run> runs)
        {
            foreach (var run in runs)
            {
                _context.Runs.Add(run);
            }
            _context.SaveChanges();
        }        
        
        public Run GetFullGameRun(int id)
        {
            return _context.Runs.FirstOrDefault(x => x.Id == id);
        }

        public Game CreateGame(Game game)
        {
            _context.Games.Add(game);
            _context.SaveChanges();

            return game;
        }

        public List<Game> GetFullGameList()
        {
            return _context.Games.ToList();
        }

        public List<Category> GetCategoriesByGame(int gameId)
        {
            return _context.Categories.Where(x => x.Game.Id == gameId).ToList();
        }
        public List<Category> GetCategoriesByGameAcronym(string gameName)
        {
            return _context.Categories.Where(x => x.Game.Acronym == gameName).ToList();
        }

        public List<Run> GetAllFullGameRuns()
        {
            return _context.Runs.ToList();
        }

        public List<Run> GetFullGameRunsByGameName(string gameName)
        {
            return _context.Runs.Where(x => x.Game.Name == gameName).ToList();
        }

        public void DeleteFullGameRun(int id)
        {
            var run = _context.Runs.FirstOrDefault(x => x.Id == id);
            _context.Remove(run);
            _context.SaveChanges();
        }        
        
        public void EditFullGameRun(Run run)
        {
            var editedRun = _context.Runs.FirstOrDefault(x => x.Id == run.Id);
            editedRun.PlayerName = run.PlayerName; 
            editedRun.Time = run.Time; 
            editedRun.Description = run.Description; 
            _context.SaveChanges();
        }
    }
}