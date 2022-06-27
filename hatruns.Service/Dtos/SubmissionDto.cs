using HatCommunityWebsite.Service.Responses.Data;
using System.ComponentModel.DataAnnotations;

namespace HatCommunityWebsite.Service.Dtos
{
    public class SubmissionDto
    {
        public int? RunId { get; set; } = null;
        public List<int>? ExtraUserIds { get; set; } = null;
        public string Platform { get; set; }
        public string? Description { get; set; }
        [Required]
        public double? Time { get; set; }
        [Required]
        public List<string>? Videos { get; set; }
        public DateTime Date { get; set; }
        [Required]
        public int? CategoryId { get; set; }
        public int? SubcategoryId { get; set; } = null;
        public bool AutoVerify { get; set; }
        public List<VariableData>? Variables { get; set; }
    }
}