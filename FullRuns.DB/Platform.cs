using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HatCommunityWebsite.DB
{
    public class Platform
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}