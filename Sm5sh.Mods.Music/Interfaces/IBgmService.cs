using System.Collections.Generic;

namespace Sm5sh.Mods.Music
{
    public interface IBgmService
    {
        IEnumerable<BgmEntry> GetBgmEntries();

        BgmEntry AddBgmEntry(string toneId);

        bool RemoveBgmEntry(string toneId);

        bool SaveChangesToStateService();
    }
}
