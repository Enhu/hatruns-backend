using System.ComponentModel.DataAnnotations;

namespace HatCommunityWebsite.DB
{
    public class Variable
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Value { get; set; }
        public bool IsDefault { get; set; }

        //navigation properties
        public ICollection<RunVariable> RunVariables { get; set; }

        public Game Game { get; set; }
        public int GameId { get; set; }
    }
}