using HatCommunityWebsite.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatCommunityWebsite.Repo
{
    public interface IRunVariableRepository
    {
        void SetRunVariables(RunVariableValue runVariable);
    }
    public class RunVariableRepository : IRunVariableRepository
    {
        private readonly AppDbContext _context;
        public RunVariableRepository(AppDbContext context)
        {
            _context = context;
        }

        public void SetRunVariables(RunVariableValue runVariable)
        {
            _context.RunVariables.Add(runVariable);
        }
    }
}
