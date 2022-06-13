using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatCommunityWebsite.DB
{
    public class RunUser
    {
        [Key]
        public int Id { get; set; }

        public int RunId { get; set; }
        public Run AssociatedRun { get; set; }

        public int UserId { get; set; }
        public User AssociatedUser { get; set; }
    }
}
