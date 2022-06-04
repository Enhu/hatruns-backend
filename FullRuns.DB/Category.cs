using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace HatCommunityWebsite.DB
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Rules { get; set; }
        public bool IsDefault { get; set; }
        public bool IsConsole { get; set; }
        public bool HasSubcategories => Subcategories.Count > 0;

        //properties for level categories
        public bool IsCustom { get; set; } = false;
        public bool IsLevel => LevelId.HasValue;

        //navigation properties
        public ICollection<Run> Runs { get; set; }

        public ICollection<Subcategory> Subcategories { get; set; }
        public Game Game { get; set; }
        public int GameId { get; set; }
        public Level? Level { get; set; }
        public int? LevelId { get; set; }
    }
}