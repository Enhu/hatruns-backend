namespace HatCommunityWebsite.Service.Responses
{
    public class GameDashboardResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Acronym { get; set; }
        public bool IsActive { get; set; }
        public string ReleaseDate { get; set; }
        public ICollection<AdminLevelData> Levels { get; set; }
        public List<AdminVariableData> Variables { get; set; }
        public List<AdminCategoryData> Categories { get; set; }
    }

    public class AdminLevelData
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class AdminVariableData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<AdminValueData> Values { get; set; }
    }

    public class AdminCategoryData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Rules { get; set; }
        public int Index { get; set; }
        public int? LevelId { get; set; }
        public ICollection<AdminSubcategoryData> Subcategories { get; set; }
    }

    public class AdminSubcategoryData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Rules { get; set; }
        public int Index { get; set; }
    }

    public class AdminValueData
    {
        public string Value { get; set; }
        public bool IsDefault { get; set; }
    }
}