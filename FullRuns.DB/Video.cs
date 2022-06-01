using System.ComponentModel.DataAnnotations;

namespace HatCommunityWebsite.DB
{
    public class Video
    {
        [Key]
        public int Id { get; set; }

        public string Link { get; set; }

        //navigation properties

        public Run Run { get; set; }

        public int RunId { get; set; }
    }
}