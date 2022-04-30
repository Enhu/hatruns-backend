using FullRuns.DB;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatCommunityWebsite.DB
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string? Country { get; set; }
        public byte[]? Avatar { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string Email { get; set; }
        public DateTime Created { get; set; }
        public int Role { get; set; }
        public List<Run>? Runs { get; set; }
    }
}
