using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatCommunityWebsite.DB
{
    public class VariableValue
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get;set; }

        //navigation property
        public ICollection<RunVariableValue> RunVariables { get; set; }
        public Variable Variable { get; set; }
        public int VariableId { get; set; }
    }
}
