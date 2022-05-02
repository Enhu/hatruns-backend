using HatCommunityWebsite.DB;

namespace HatCommunityWebsite.API.Dtos
{
    public class RunDto
    {
        public string PlayerName { get; set; }
        public string Platform { get; set; }
        public string? Description { get; set; }
        public double Time { get; set; }
        public string VideoLinks { get; set; }
        public DateTime Date { get; set; }
        public string SubmittedBy { get; set; }
        public bool? IsObsolete { get; set; }
        public string? Variables { get; set; }
        public int GameId { get; set; }
        public int CategoryId { get; set; }
        public int? SubcategoryId { get; set; }
        public bool AutoVerify { get; set; }
    }
}
