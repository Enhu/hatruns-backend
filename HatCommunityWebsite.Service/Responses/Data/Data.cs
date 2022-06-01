using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatCommunityWebsite.Service.Responses.Data
{
    public class VariablesData
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class GameData
    {
        public string Name { get; set; }
        public string Acronym { get; set; }
    }

    public class CategoryData
    {
        public string Name { set; get; }
    }

    public class SubCategoryData
    {
        public string? Name { set; get; }
    }

    public class RunData
    {
        public int Id { get; set; }
        public string PlayerName { set; get; } = string.Empty;
        public string Place { get; set; }
        public string CategoryName { get; set; }
        public string SubcategoryName { get; set; } = string.Empty;
        public double Time { get; set; }
        public DateTime Date { get; set; }
        public bool IsObsolete { get; set; }
        public string LevelName { get; set; } = string.Empty;
    }
}
