namespace HatCommunityWebsite.Service.Dtos
{
    public class ImportDto
    {
        public List<string> PlayerNames { get; set; }
        public string Platform { get; set; }
        public string? Description { get; set; }
        public double Time { get; set; }
        public List<string> Videos { get; set; }
        public DateTime Date { get; set; }
        public bool IsObsolete { get; set; }
        public string SubmittedBy { get; set; }
        public string? Variables { get; set; }
        public int Status { get; set; }
        public int GameId { get; set; }
        public int CategoryId { get; set; }
        public int? SubcategoryId { get; set; }

    }
}