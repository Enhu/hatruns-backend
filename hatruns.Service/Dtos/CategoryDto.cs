using System.ComponentModel.DataAnnotations;

namespace HatCommunityWebsite.Service.Dtos
{
    public class CategoryDto
    {
        public int? Id { get; set; } = null;

        [Required]
        public int? GameId { get; set; }

        public string? Name { get; set; }
        public string? Rules { get; set; }
        [Required]
        public int? Index { get; set; }
        public bool? IsConsole { get; set; }
        public int? LevelId { get; set; } = null;
        public bool? IsCustom { get; set; } 
        public List<SubcategoryDto>? Subcategories { get; set; }
    }

    public class SubcategoryDto
    {
        public int? Id { get; set; } = null;
        public int? Index { get; set; } = null;
        public string? Name { get; set; }
        public string? Rules { get; set; }
    }
}