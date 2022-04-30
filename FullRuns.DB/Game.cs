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
    public class Game
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Acronym { get; set; }
        public string ReleaseDate { get; set; } 

        //Navigation Properties
        public List<Category> Categories { get; set; }
        public List<SubCategory> Subcategories { get; set; }
        public List<Variable>? Variables { get; set; }
        [JsonIgnore]
        public List<Run> Runs { get; set; }
    }
}
