using HatCommunityWebsite.DB;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatCommunityWebsite.Repo
{
    public interface IVariableValueRepository
    {
        Task<VariableValue> GetValueById(int id);
        Task SaveValue(VariableValue value);
        Task UpdateValue(VariableValue value);
        Task<VariableValue> GetValueByNameAndVaribleId(int id, string value);
    }
    public class VariableValueRepository : IVariableValueRepository
    {
        private readonly AppDbContext _context;

        public VariableValueRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<VariableValue> GetValueById(int id)
        {
            return await _context.VariableValues.FindAsync(id);
        }

        public async Task<VariableValue> GetValueByNameAndVaribleId(int id, string value)
        {
            return await _context.VariableValues
                .Where(x => x.Name == value && x.VariableId == id)
                .FirstOrDefaultAsync();
        }


        public async Task UpdateValue(VariableValue value)
        {
            _context.VariableValues.Update(value);
            await _context.SaveChangesAsync();
        }

        public async Task SaveValue(VariableValue value)
        {
            _context.VariableValues.Add(value);
            await _context.SaveChangesAsync();
        }
    }
}
