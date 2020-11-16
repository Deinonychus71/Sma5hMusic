using Sm5sh.Mods.Music.Models;
using System.Collections.Generic;

namespace Sm5sh.Mods.Music.Interfaces
{
    public interface IAudioStateService
    {
        IEnumerable<BgmEntry> GetBgmEntries();
        IEnumerable<BgmEntry> GetModBgmEntries();
        BgmEntry GetBgmEntry(string toneId);
        BgmEntry AddOrUpdateBgmEntry(BgmEntry bgmEntry);
        void RemoveBgmEntry(string toneId);
    }
}
