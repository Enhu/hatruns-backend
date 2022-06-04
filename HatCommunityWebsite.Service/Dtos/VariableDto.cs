namespace HatCommunityWebsite.Service.Dtos
{
    public class VariableDto
    {
        public int? Id { get; set; }
        public string? Name { get; set; }

        public List<VariableValuesDto>? Values { get; set; }
    }

    public class VariableValuesDto
    {
        public int? Id { get; set; }
        public string? Value { get; set; }
        public bool? IsDefault { get; set; } = false;
    }
}
