namespace HatCommunityWebsite.Service.Dtos
{
    public class UpdateSubmissionDto
    {
        public int RunId { get; set; }
        public string Platform { get; set; }
        public string? Description { get; set; }
        public double Time { get; set; }
        public DateTime Date { get; set; }
        public string? Variables { get; set; }
        public int? SubcategoryId { get; set; }
        public string VideoLinks { get; set; }
        public bool AutoVerify { get; set; }
    }
}
