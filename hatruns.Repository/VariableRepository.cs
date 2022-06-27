using HatCommunityWebsite.DB;
using Microsoft.EntityFrameworkCore;

namespace HatCommunityWebsite.Repo
{
    public interface IVariableRepository
    {
        Task<Variable> GetVariableByNameAndId(int id, string name);

        Task<Variable> GetVariableByIdIncludeValues(int id);

        Task<Variable> GetVariableById(int id);

        Task UpdateVariable(Variable variable);

        Task SaveVariable(Variable variable);

        Task UpdateVariables(List<Variable> variables);

        Task SaveVariables(List<Variable> variables);

        Task<bool> VariableExistsByName(string name);
    }

    public class VariableRepository : IVariableRepository
    {
        private readonly AppDbContext _context;

        public VariableRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Variable> GetVariableById(int id)
        {
            return await _context.Variables.FindAsync(id);
        }

        public async Task<Variable> GetVariableByIdIncludeValues(int id)
        {
            return await _context.Variables
                .Include(x => x.Values)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> VariableExistsByName(string name)
        {
            return await _context.Variables.AnyAsync(x => x.Name == name);
        }

        public async Task<Variable> GetVariableByNameAndId(int id, string name)
        {
            return await _context.Variables
                    .FirstAsync(v => v.Name == name && v.Id == id);
        }

        public async Task SaveVariable(Variable variable)
        {
            _context.Variables.Add(variable);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateVariable(Variable variable)
        {
            _context.Variables.Add(variable);
            await _context.SaveChangesAsync();
        }

        public async Task SaveVariables(List<Variable> variables)
        {
            foreach (var variable in variables)
                _context.Variables.Add(variable);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateVariables(List<Variable> variables)
        {
            foreach (var variable in variables)
                _context.Variables.Update(variable);

            await _context.SaveChangesAsync();
        }
    }
}