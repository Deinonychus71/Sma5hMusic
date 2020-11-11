using Sm5shMusic.Models;
using System.Collections.Generic;

namespace Sm5shMusic.Interfaces
{
    public interface IParacobService
    {
        bool GenerateGameTitlePrcFile(List<GameTitleDbNewEntry> gameTitleEntries, string outputFilePath);
        bool GenerateBgmPrcFile(List<BgmDbNewEntry> bgmEntries, string outputFilePath);
        string GetNewBgmId();
        List<GameTitleDbEntry> GetCoreDbRootGameTitleEntries();
        List<BgmDbRootEntry> GetCoreDbRootBgmEntries();
    }
}
