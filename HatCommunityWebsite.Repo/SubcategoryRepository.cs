using HatCommunityWebsite.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatCommunityWebsite.Repo
{
    public interface ISubCategoryRepository
    {
        Task<Subcategory> GetSubcategoryById(int id);
    }
    public class SubcategoryRepository : ISubCategoryRepository
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
    }
}
