using System.Collections.Generic;

namespace Sm5sh.Mods.Music.Models
{
    public class MusicModDeleteEntries
    {
        public List<string> BgmDbRootEntries { get; }
        public List<string> BgmAssignedInfoEntries { get; }
        public List<string> BgmStreamSetEntries  { get; }
        public List<string> BgmStreamPropertyEntries { get; }
        public List<string> BgmPropertyEntries { get; }
        public List<string> GameTitleEntries { get; }

        public MusicModDeleteEntries()
        {
            BgmDbRootEntries = new List<string>();
            BgmAssignedInfoEntries = new List<string>();
            BgmStreamSetEntries = new List<string>();
            BgmStreamPropertyEntries = new List<string>();
            BgmPropertyEntries = new List<string>();
            GameTitleEntries = new List<string>();
        }
    }
}
