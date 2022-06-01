namespace HatCommunityWebsite.Service.Dtos
{
    public class LeaderboardDto
    {
        public int CategoryId { get; set; }
        public int? SubcategoryId { get; set; } = null;
        public int? LevelId { get; set; } = null;
    }
}
