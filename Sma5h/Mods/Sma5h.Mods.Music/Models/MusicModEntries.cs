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
        public List<SeriesEntry> SeriesEntries { get; }

        public MusicModEntries()
        {
            BgmDbRootEntries = new List<BgmDbRootEntry>();
            BgmAssignedInfoEntries = new List<BgmAssignedInfoEntry>();
            BgmStreamSetEntries = new List<BgmStreamSetEntry>();
            BgmStreamPropertyEntries = new List<BgmStreamPropertyEntry>();
            BgmPropertyEntries = new List<BgmPropertyEntry>();
            GameTitleEntries = new List<GameTitleEntry>();
            SeriesEntries = new List<SeriesEntry>();
        }

        public MusicModDeleteEntries GetMusicModDeleteEntries()
        {
            var output = new MusicModDeleteEntries();
            if (BgmDbRootEntries != null)
            {
                foreach (var entry in BgmDbRootEntries)
                {
                    output.BgmDbRootEntries.Add(entry.UiBgmId);
                }
            }
            if (BgmAssignedInfoEntries != null)
            {
                foreach (var entry in BgmAssignedInfoEntries)
                {
                    output.BgmAssignedInfoEntries.Add(entry.InfoId);
                }
            }
            if (BgmStreamSetEntries != null)
            {
                foreach (var entry in BgmStreamSetEntries)
                {
                    output.BgmStreamSetEntries.Add(entry.StreamSetId);
                }
            }
            if (BgmStreamPropertyEntries != null)
            {
                foreach (var entry in BgmStreamPropertyEntries)
                {
                    output.BgmStreamPropertyEntries.Add(entry.StreamId);
                }
            }
            if (BgmPropertyEntries != null)
            {
                foreach (var entry in BgmPropertyEntries)
                {
                    output.BgmPropertyEntries.Add(entry.NameId);
                }
            }
            return output;
        }
    }
}
