using FullRuns.DB;
using HatCommunityWebsite.DB;

namespace HatCommunityWebsite.Core
{
    public interface IHatCommunityWebsiteServices
    {
        Run CreateFullGameRun(Run run);
        void CreateMultipleRuns(List<Run> runs);
        Game CreateGame(Game game);
        Run GetFullGameRun(int id);
        List<Game> GetFullGameList();
        List<Category> GetCategoriesByGame(int gameId);
        List<Category> GetCategoriesByGameAcronym(string gameName);
        List<Run> GetFullGameRunsByGameName(string gameName);
        List<Run> GetAllFullGameRuns();
        void DeleteFullGameRun(int id);
        void EditFullGameRun(Run run);
    }
}