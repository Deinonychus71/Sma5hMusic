using System.Collections.Generic;

namespace Sm5sh.Mods.Music
{
    public interface IBgmService
    {
        Dictionary<string, BgmEntry> BgmEntries { get; }
    }
}
