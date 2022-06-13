using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HatCommunityWebsite.DB
{
    public class Game
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Acronym { get; set; }
        public bool IsActive { get; set; }
        public string ReleaseDate { get; set; }

        //navigation properties
        public ICollection<Level> Levels { get; set; }
        public ICollection<Category> Categories { get; set; }

        public ICollection<Variable> Variables { get; set; }
    }
}