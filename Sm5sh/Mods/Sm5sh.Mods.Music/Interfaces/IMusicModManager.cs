using Sm5sh.Mods.Music.Models;
using System.Collections.Generic;

namespace Sm5sh.Mods.Music.Interfaces
{
    public interface IMusicModManager
    {
        List<BgmEntry> LoadBgmEntriesFromMod();
        bool AddBgmToMod(string filename);
        bool SaveBgmChangesToMod(BgmEntry bgmEntry);
    }
}
