using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace HatCommunityWebsite.DB
{
    public class Subcategory
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public string Rules { get; set; }

        //navigation properties
        public ICollection<Run> Runs { get; set; }
        public Category Category { get; set; }
        public int CategoryId { get; set; }
    }
}