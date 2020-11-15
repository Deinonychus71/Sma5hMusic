using Sm5sh.Mods.Music.Models;
using System.Collections.Generic;

namespace Sm5sh.Mods.Music.Interfaces
{
    public interface IBgmService
    {
        IEnumerable<BgmEntry> GetBgmEntries();

        BgmEntry AddBgmEntry(string toneId);

        bool RemoveBgmEntry(string toneId);
    }
}
