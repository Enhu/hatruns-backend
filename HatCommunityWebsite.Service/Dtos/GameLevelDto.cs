namespace HatCommunityWebsite.Service.Dtos
{
    public class GameLevelDto
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public List<int>? CategoriesIds { get; set; }
    }
}
