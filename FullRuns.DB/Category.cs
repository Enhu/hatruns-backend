using FullRuns.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatCommunityWebsite.DB
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Rules { get; set; }
        public bool? IsLevel { get; set; }
        public bool? IsConsole { get; set; }

        //Navigation Properties
        [JsonIgnore]
        public List<Run> Runs { get; set; }
        public List<SubCategory>? SubCategories { get; set; }
        public int GameId { get; set; }
        [JsonIgnore]
        public Game Game { get; set; }
    }
}
