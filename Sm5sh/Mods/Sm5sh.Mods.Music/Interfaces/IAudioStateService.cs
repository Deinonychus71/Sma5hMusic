using Sm5sh.Mods.Music.Models;
using System.Collections.Generic;

namespace Sm5sh.Mods.Music.Interfaces
{
    public interface IAudioStateService
    {
        IEnumerable<BgmEntry> GetBgmEntries();
        IEnumerable<BgmEntry> GetModBgmEntries();
        IEnumerable<GameTitleEntry> GetGameTitleEntries();
        IEnumerable<string> GetSeriesEntries();
        IEnumerable<string> GetLocales();
        BgmEntry GetBgmEntry(string toneId);
        bool AddBgmEntry(BgmEntry bgmEntry);
        void RemoveBgmEntry(string toneId);
        bool SaveBgmEntriesToStateManager();
    }
}
