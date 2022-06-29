using System.ComponentModel.DataAnnotations;

namespace HatCommunityWebsite.Service.Dtos;

public class BanUserDto
{
    [Required]
    public int? UserId { get; set; }
}