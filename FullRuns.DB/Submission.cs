namespace FullRuns.DB
{
    public class Submission
    {
        public int Id { get; set; }
        public int Status { get; set; }
        public string PlayerName { get; set; }
        public string Platform { get; set; }
        public string? Description { get; set; }
        public double Time { get; set; }
        public string VideoLinks { get; set; }
        public DateTime Date { get; set; }
        public DateTime SubmittedDate { get; set; }
        public string SubmittedBy { get; set; }
        public string? Variables { get; set; }
        public int? UserId { get; set; }
        public string CategoryName { get; set; }
        public int CategoryId { get; set; }
        public int? SubcategoryId { get; set; }
        public string? SubcategoryName { get; set; }
        public string GameName { get; set; }
        public int GameId { get; set; }
    }
}