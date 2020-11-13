using Sm5shMusic.Models;
using System.Collections.Generic;

namespace Sm5shMusic.Interfaces
{
    public interface IParacobService
    {
        bool GenerateGameTitlePrcFile(List<GameTitleDbNewEntry> gameTitleEntries, string outputFilePath);
        bool GenerateBgmPrcFile(List<BgmDbNewEntry> bgmEntries, string outputFilePath);
        bool UpdateStagePrcFile(List<StageDbEntry> stageEntries, string outputFilePath);
        string GetNewBgmId();
        List<GameTitleDbEntry> GetCoreDbRootGameTitleEntries();
        List<BgmDbRootEntry> GetCoreDbRootBgmEntries();
        List<StageDbEntry> GetCoreDbRootStageEntries();
        List<string> GetCoreDbRootPlaylists();
    }
}
