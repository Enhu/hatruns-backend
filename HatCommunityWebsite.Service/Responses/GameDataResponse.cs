namespace HatCommunityWebsite.Service.Responses
{
    public class GameDataResponse
    {
        public LbGameData Game { get; set; }
        public List<LbLevelData> Levels { get; set; }
        public List<LbVariableData> Variables { get; set; }
        public List<LbCategoryData> Categories { get; set; }
    }

    public class LbGameData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Portrait { get; set; }
        public string ReleasedDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class LbLevelData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Rules { get; set; }
        public List<LbCategoryData> Categories { get; set; }
    }

    public class LbVariableData
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class LbCategoryData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Rules { get; set; }
        public List<LbSubcategoryData>? Subcategories { get; set; }
    }

    public class LbSubcategoryData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Rules { get; set; }
    }
}