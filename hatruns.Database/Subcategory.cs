using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HatCommunityWebsite.DB
{
    public class Subcategory
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int Index { get; set; }
        public string? Rules { get; set; }

        //navigation properties
        public ICollection<Run> Runs { get; set; }
        public Category Category { get; set; }
        public int CategoryId { get; set; }
    }
}