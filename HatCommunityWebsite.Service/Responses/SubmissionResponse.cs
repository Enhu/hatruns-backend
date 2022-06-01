using HatCommunityWebsite.DB;
using HatCommunityWebsite.Service.Responses.Data;

namespace HatCommunityWebsite.Service.Responses
{
    public class SubmissionResponse
    {
        public string PlayerName { get; set; }
        public string Platform { get; set; }
        public string? Description { get; set; }
        public double Time { get; set; }
        public string VideoLinks { get; set; }
        public DateTime Date { get; set; }
        public string SubmittedBy { get; set; }
        public DateTime SubmittedDate { get; set; }
        public string Place { get; set; }

        //verification status
        public int Status { get; set; }
        public string? VerifiedBy { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public DateTime? RejectedDate { get; set; }
        public string? RejectedBy { get; set; }
        public string? RejectedReason { get; set; }
        public bool IsObsolete { get; set; }

        //relationships properties
        public List<VariablesData> Variables { get; set; }
        public GameData Game { get; set; }
        public CategoryData Category { get; set; }
        public SubCategoryData SubCategory { get; set; }
    }
}