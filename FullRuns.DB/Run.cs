using HatCommunityWebsite.DB;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace FullRuns.DB
{
    public class Run
    {
        [Key]
        public int Id { get; set; }
        public string PlayerName { get; set; }    
        public string Platform { get; set; }
        public string? Description { get; set; }    
        public double Time { get; set; }
        public string VideoLinks { get; set; }
        public DateTime Date { get; set; }
        public string SubmittedBy { get; set; }
        public DateTime SubmittedDate { get; set; }

        //
        public int Status { get; set; }
        public string? VerifiedBy { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public DateTime? RejectedDate { get; set; }
        public string? RejectedBy { get; set; }
        public string? RejectedReason { get; set; }
        public bool IsObsolete { get; set; }

        //Navigation Properties
        public List<RunVariable> RunVariables { get; set; }
        [JsonIgnore]
        public int GameId { get; set; }
        public Game Game { get; set; }
        [JsonIgnore]
        public int? UserId { get; set; }
        public User? User { get; set; }
        [JsonIgnore]
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        [JsonIgnore]
        public int? SubcategoryId { get; set; }
        public SubCategory? SubCategory { get; set; }
    }
}