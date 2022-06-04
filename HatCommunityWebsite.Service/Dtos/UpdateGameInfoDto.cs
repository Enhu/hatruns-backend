namespace HatCommunityWebsite.Service.Dtos
{
    public class UpdateGameInfoDto
    {
        public int Id { get; set; }
        public string? Acronym { get; set; }
        public string? ReleasedDate { get; set; }
        public bool? IsActive { get; set; }
    }
}
