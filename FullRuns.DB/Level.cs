using System.ComponentModel.DataAnnotations;

namespace HatCommunityWebsite.DB
{
    public class Level
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        //navigation properties

        public ICollection<Category> Categories { get; set; }
        public Game Game { get; set; }
        public int GameId { get; set; }
    }
}