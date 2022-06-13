using System.ComponentModel.DataAnnotations;

namespace HatCommunityWebsite.Service.Dtos
{
    public class UpdateGameInfoDto
    {
        [Required]
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Acronym { get; set; }
        public string? ReleasedDate { get; set; }
        public bool? IsActive { get; set; }
    }
}