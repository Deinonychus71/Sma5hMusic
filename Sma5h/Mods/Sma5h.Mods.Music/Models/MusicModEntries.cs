using System.Collections.Generic;

namespace Sma5h.Mods.Music.Models
{
    public class MusicModEntries
    {
        public List<BgmDbRootEntry> BgmDbRootEntries { get; }
        public List<BgmAssignedInfoEntry> BgmAssignedInfoEntries { get; }
        public List<BgmStreamSetEntry> BgmStreamSetEntries { get; }
        public List<BgmStreamPropertyEntry> BgmStreamPropertyEntries { get; }
        public List<BgmPropertyEntry> BgmPropertyEntries { get; }
        public List<GameTitleEntry> GameTitleEntries { get; }

        public MusicModEntries()
        {
            BgmDbRootEntries = new List<BgmDbRootEntry>();
            BgmAssignedInfoEntries = new List<BgmAssignedInfoEntry>();
            BgmStreamSetEntries = new List<BgmStreamSetEntry>();
            BgmStreamPropertyEntries = new List<BgmStreamPropertyEntry>();
            BgmPropertyEntries = new List<BgmPropertyEntry>();
            GameTitleEntries = new List<GameTitleEntry>();
        }
    }
}
