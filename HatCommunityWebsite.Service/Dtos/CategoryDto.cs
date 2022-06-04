namespace HatCommunityWebsite.Service.Dtos
{
    public class CategoryDto
    {
        public int? Id { get; set; } 
        public string? Name { get; set; }
        public string? Rules { get; set; }
        public bool? IsDefault { get; set; }
        public bool? IsConsole { get; set; }
        public List<SubcategoryDto>? Subcategories { get; set; }
    }

    public class SubcategoryDto
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Rules { get; set; }
        public bool? IsDefault { get; set; }
    }
}
