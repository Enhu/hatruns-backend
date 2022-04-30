using FullRuns.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace HatCommunityWebsite.DB
{
    public class Variable
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public bool IsDefault { get; set; }

        //Navigation Properties
        [JsonIgnore]
        public List<RunVariable> RunVariables { get; set; }
        [JsonIgnore]
        public int GameId { get; set; }
        [JsonIgnore]
        public Game Game { get; set; }
    }
}
