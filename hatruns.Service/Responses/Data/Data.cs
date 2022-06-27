using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatCommunityWebsite.Service.Responses.Data
{
    public class VariableData
    {
        public int VariableId { get; set; }
        public string VariableName { get; set; }
        public int? ValueId { get; set; }
        public string Value { get; set; }
    }

    public class GameData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Acronym { get; set; }
    }

    public class LevelData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Rules { get; set; }
        public List<CategoryData> Categories { get; set; }
    }

    public class CategoryData
    {
        public int Id { get; set; }
        public string Name { set; get; }
        public string Rules { set; get; }
        public List<SubcategoryData>? Subcategories { get; set; }
    }

    public class SubcategoryData
    {
        public int? Id { get; set; }
        public string? Name { set; get; }
        public string Rules { set; get; }
    }

    public class RunData
    {
        public int Id { get; set; }
        public List<string>? PlayerNames { set; get; } = null;
        public string Place { get; set; }
        public string CategoryName { get; set; }
        public string SubcategoryName { get; set; } = string.Empty;
        public double Time { get; set; }
        public DateTime Date { get; set; }
        public bool IsObsolete { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public List<string>? VariablesValues { get; set; }
    }
}
