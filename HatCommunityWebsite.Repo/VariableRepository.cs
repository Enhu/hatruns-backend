using HatCommunityWebsite.DB;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatCommunityWebsite.Repo
{
    public interface IVariableRepository
    {
        Task<Variable> GetByNameAndId(int id, string name);
    }
    public class VariableRepository : IVariableRepository
    {
        private readonly AppDbContext _context;
        public VariableRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Variable> GetByNameAndId(int id, string name)
        {
            return await _context.Variables
                    .FirstAsync(v => v.Name == name && v.Id == id);
        }
    }
}
