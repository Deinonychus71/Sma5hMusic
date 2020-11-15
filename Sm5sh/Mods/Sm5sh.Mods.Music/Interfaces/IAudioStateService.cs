using Sm5sh.Mods.Music.Models;
using System.Collections.Generic;

namespace Sm5sh.Mods.Music.Interfaces
{
    public interface IAudioStateService
    {
        IEnumerable<BgmEntry> GetBgmEntries();
        IEnumerable<BgmEntry> GetModBgmEntries();
        BgmEntry AddBgmEntry(string toneId, BgmEntry bgmEntry);
        bool RemoveBgmEntry(string toneId);
        bool SaveChanges();
    }
}
