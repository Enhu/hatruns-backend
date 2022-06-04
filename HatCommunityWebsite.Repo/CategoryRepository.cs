using HatCommunityWebsite.DB;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatCommunityWebsite.Repo
{
    public interface ICategoryRepository
    {
        Task<Category> GetCategoryById(int id);
        Task<Category> GetCategoryByIdIncludeSubcategories(int id);
        Task SaveCategory(Category category);
        Task UpdateCategory(Category category);
        Task<List<Category>> GetAllLevelCategories();
    }
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Category> GetCategoryById(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<Category> GetCategoryByIdIncludeSubcategories(int id)
        {
            return await _context.Categories
                .Include(x => x.Subcategories)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Category>> GetAllLevelCategories()
        {
            return await _context.Categories
                .Where(x => x.IsLevel && !x.IsCustom)
                .DistinctBy(x => x.Name)
                .ToListAsync();
        }

        public async Task SaveCategory(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCategory(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }
    }
}
