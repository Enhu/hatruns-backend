using HatCommunityWebsite.DB;

namespace HatCommunityWebsite.Repo
{
    public interface ISubcategoryRepository
    {
        Task<Subcategory> GetSubcategoryById(int id);

        Task SaveSubcategory(Subcategory subcategory);

        Task UpdateSubcategory(Subcategory subcategory);

        Task SaveSubcategories(List<Subcategory> subcategories);

        Task UpdateSubcategories(List<Subcategory> subcategories);
    }

    public class SubcategoryRepository : ISubcategoryRepository
    {
        private readonly AppDbContext _context;

        public SubcategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Subcategory> GetSubcategoryById(int id)
        {
            return await _context.Subcategories.FindAsync(id);
        }

        public async Task SaveSubcategory(Subcategory subcategory)
        {
            _context.Subcategories.Add(subcategory);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSubcategory(Subcategory subcategory)
        {
            _context.Subcategories.Update(subcategory);
            await _context.SaveChangesAsync();
        }

        public async Task SaveSubcategories(List<Subcategory> subcategories)
        {
            foreach (var subcategory in subcategories)
                _context.Subcategories.Add(subcategory);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateSubcategories(List<Subcategory> subcategories)
        {
            foreach (var subcategory in subcategories)
                _context.Subcategories.Update(subcategory);

            await _context.SaveChangesAsync();
        }
    }
}