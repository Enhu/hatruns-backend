using HatCommunityWebsite.Repo;
using HatCommunityWebsite.Service.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatCommunityWebsite.Service
{
    public interface IAdminService
    {
        
    }
    public class AdminService : IAdminService
    {
        private readonly IVariableRepository _variableRepo;

        public AdminService(IVariableRepository variableRepository)
        {
            _variableRepo = variableRepository;
        }

        public DashboardResponse GetDashboardData()
        {
            var request = new DashboardResponse
            {

            };

            return request;
        }
    }
}
