using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HatCommunityWebsite.DB
{
    public class Variable
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        //navigation properties
        public ICollection<VariableValue> Values { get; set; }

        public Game Game { get; set; }
        public int GameId { get; set; }
    }
}