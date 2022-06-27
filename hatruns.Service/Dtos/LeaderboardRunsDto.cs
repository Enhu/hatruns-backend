using System.ComponentModel.DataAnnotations;

namespace HatCommunityWebsite.Service.Dtos
{
    public class LeaderboardRunsDto
    {
        [Required]
        public int? CategoryId { get; set; }
        public int? SubcategoryId { get; set; } = null;
        public int? LevelId { get; set; } = null;
    }
}
