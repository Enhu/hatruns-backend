using HatCommunityWebsite.DB;
using Microsoft.EntityFrameworkCore;

namespace HatCommunityWebsite.Repo
{
    public interface IRunRepository
    {
        Task<Run> GetRunByIdWithAllRelationships(int id);

        Task<Run> GetRunById(int id);

        Task SaveRun(Run run);

        Task SaveRuns(List<Run> runs);

        Task UpdateRun(Run run);

        Task<Run> GetCurrentVerifiedRun(int userId, int categoryId, int? subCategoryId = null);

        Task<Run> GetLastVerifiedRun(int userId, int categoryId, int? subCategoryId = null);

        Task<Run> GetRunByIdWithRunVariables(int id);

        Task<List<double>> GetLeaderboardTimes(int categoryId, int? subcategoryId = null);

        Task<List<Run>> GetLeaderboardRuns(int categoryId, int? subcategoryId = null);

        Task<List<Run>> GetUserPendingRuns(int userId);

        Task<List<Run>> GetVerifiedUserProfileRuns(string username);

        Task<List<Run>> GetAllLeaderboardRuns(int categoryId, int? subcategoryId = null, int? levelId = null);

        Task DeleteRun(Run run);
    }

    public class RunRepository : IRunRepository
    {
        private readonly AppDbContext _context;

        public RunRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Run> GetRunByIdWithAllRelationships(int id)
        {
            return await _context.Runs
                        .Include(c => c.Category)
                        .ThenInclude(g => g.Game)
                        .Include(sc => sc.SubCategory)
                        .Include(rv => rv.RunVariableValues)
                        .ThenInclude(v => v.AssociatedVariableValue)
                        .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Run> GetCurrentVerifiedRun(int userId, int categoryId, int? subCategoryId = null)
        {
            return await _context.Runs
                        .Where(p => p.RunUsers.First().AssociatedUser.Id == userId)
                        .Where(c => c.CategoryId == categoryId)
                        .Where(sc => sc.SubcategoryId == subCategoryId)
                        .Where(o => o.IsObsolete == false)
                        .Where(s => s.Status == 1)
                        .OrderBy(x => x.Time).FirstOrDefaultAsync();
        }

        public async Task<Run> GetLastVerifiedRun(int userId, int categoryId, int? subCategoryId = null)
        {
            return await _context.Runs
                        .Where(p => p.RunUsers.First().AssociatedUser.Id == userId)
                        .Where(c => c.CategoryId == categoryId)
                        .Where(sc => sc.SubcategoryId == subCategoryId)
                        .Where(o => o.IsObsolete == true)
                        .Where(s => s.Status == 1)
                        .OrderBy(x => x.Time).FirstOrDefaultAsync();
        }

        public async Task<List<Run>> GetUserPendingRuns(int userId)
        {
            return await _context.Runs
                .Where(p => p.RunUsers.First().AssociatedUser.Id == userId)
                .Where(s => s.Status == 0)
                .ToListAsync();
        }

        public async Task<Run> GetRunByIdWithRunVariables(int id)
        {
            return await _context.Runs
                .Include(x => x.RunVariableValues)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Run> GetRunById(int id)
        {
            return await _context.Runs
                .Include(x => x.RunUsers)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<List<double>> GetLeaderboardTimes(int categoryId, int? subcategoryId = null)
        {
            return await _context.Runs
                    .Where(x => x.IsObsolete == false)
                    .Where(x => x.CategoryId == categoryId)
                    .Where(x => x.SubcategoryId == subcategoryId)
                    .OrderBy(x => x.Time)
                    .Select(x => x.Time)
                    .ToListAsync();
        }

        public async Task<List<Run>> GetLeaderboardRuns(int categoryId, int? subcategoryId = null)
        {
            return await _context.Runs
                    .Where(x => x.IsObsolete == false)
                    .Where(x => x.CategoryId == categoryId)
                    .Where(x => x.SubcategoryId == subcategoryId)
                    .OrderBy(x => x.Time)
                    .ToListAsync();
        }

        public async Task<List<Run>> GetAllLeaderboardRuns(int categoryId, int? subcategoryId = null, int? levelId = null)
        {
            return await _context.Runs
                .Include(x => x.RunUsers)
                .Include(x => x.Category)
                .ThenInclude(x => x.Game)
                    .Where(x => x.CategoryId == categoryId)
                    .Where(x => x.SubcategoryId == subcategoryId)
                    .Where(x => x.Category.LevelId == levelId)
                    .OrderBy(x => x.Time)
                    .ToListAsync();
        }

        public async Task<List<Run>> GetVerifiedUserProfileRuns(string username)
        {
            return await _context.Runs
                .Include(x => x.Category)
                .ThenInclude(x => x.Game)
                .Include(x => x.SubCategory)
                .Include(x => x.Category)
                .ThenInclude(x => x.Level)
                .Where(x => x.RunUsers.First().AssociatedUser.Username == username && x.Status == 1)
                .OrderByDescending(x => x.Date)
                .ToListAsync();
        }

        public async Task SaveRun(Run run)
        {
            _context.Runs.Add(run);
            await _context.SaveChangesAsync();
        }

        public async Task SaveRuns(List<Run> runs)
        {
            foreach (var run in runs)
            {
                _context.Runs.Add(run);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateRun(Run run)
        {
            _context.Runs.Update(run);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRun(Run run)
        {
            _context.Runs.Remove(run);
            await _context.SaveChangesAsync();
        }
    }
}