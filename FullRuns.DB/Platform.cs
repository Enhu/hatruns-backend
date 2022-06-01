using System.ComponentModel.DataAnnotations;

namespace HatCommunityWebsite.DB
{
    public class Platform
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}