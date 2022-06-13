using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HatCommunityWebsite.DB
{
    public class RunVariableValue
    {
        [Key]
        public int Id { get; set; }

        public int RunId { get; set; }
        public Run AssociatedRun { get; set; }

        public int VariableValueId { get; set; }
        public VariableValue AssociatedVariableValue { get; set; }
    }
}