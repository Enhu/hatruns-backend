using System.ComponentModel.DataAnnotations;

namespace HatCommunityWebsite.DB
{
    public class Run
    {
        [Key]
        public int Id { get; set; }

        public string Platform { get; set; }
        public string? Description { get; set; }
        public double Time { get; set; }
        public List<Video> Videos { get; set; }
        public DateTime Date { get; set; }
        public string SubmittedBy { get; set; }
        public DateTime SubmittedDate { get; set; }

        //verification status
        public int Status { get; set; }

        public string? VerifiedBy { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public DateTime? RejectedDate { get; set; }
        public string? RejectedBy { get; set; }
        public string? RejectedReason { get; set; }
        public bool IsObsolete { get; set; }

        //navigation properties
        public ICollection<RunVariableValue>? RunVariableValues { get; set; }
        public ICollection<RunUser> RunUsers { get; set; }

        public Category? Category { get; set; }
        public int? CategoryId { get; set; }
        public Subcategory? SubCategory { get; set; }
        public int? SubcategoryId { get; set; }
    }
}