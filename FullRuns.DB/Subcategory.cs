using FullRuns.DB;
using Newtonsoft.Json;

namespace HatCommunityWebsite.DB
{
    public class SubCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public string Rules { get; set; }

        //Navigation Properties
        [JsonIgnore]
        public List<Category> Category { get; set; }
        [JsonIgnore]
        public List<Run> Runs { get; set; }
        public int GameId { get; set; }
        [JsonIgnore]
        public Game Game { get; set; }

    }
}
