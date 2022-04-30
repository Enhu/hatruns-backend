using HatCommunityWebsite.DB;

namespace FullRuns.DB
{
    public class RunVariable
    {
        public int Id { get; set; }

        public int RunId { get; set; }
        public Run AssociatedRun { get; set; }

        public int VariableId { get; set; }
        public Variable AssociatedVariable { get; set; }
    }
}