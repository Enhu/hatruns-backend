using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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