using System.ComponentModel.DataAnnotations;

namespace HatCommunityWebsite.Service.Dtos
{
    public class GameLevelDto
    {
        public int? Id { get; set; } = null;
        [Required]
        public int? GameId { get; set; }
        public string? Rules { get; set; }
        public string? Name { get; set; }
    }
}
