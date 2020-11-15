using Sm5sh.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Sm5sh.Mods.Music.Models
{
    public class BgmEntry
    {
        public string ToneId { get; set; }

        public GameTitleEntry GameTitle { get; set; }

        public Dictionary<string, string> Title { get; set; }

        public Dictionary<string, string> Copyright { get; set; }

        public Dictionary<string, string> Author { get; set; }

        public string RecordType { get; set; }

        public BgmEntryModels.AudioCuePoints AudioCuePoints { get; set; }

        public float AudioVolume { get; set; }

        public bool IsDlc { get; set; }

        public bool IsPatch { get; set; }

        public List<BgmEntryModels.PlaylistEntry> Playlists { get; set; }

        public BgmEntryModels.BgmAudioSource Source { get; set; }

        public string FileName { get; set; }

        public override string ToString()
        {
            return ToneId;
        }
    }

    namespace BgmEntryModels
    {
        public enum BgmAudioSource
        {
            CoreGame = 0,
            Mod = 1
        }

        public class AudioCuePoints
        {
            public ulong LoopStartMs { get; set; }
            public ulong LoopStartSample { get; set; }
            public ulong LoopEndMs { get; set; }
            public ulong LoopEndSample { get; set; }
            public ulong TotalTimeMs { get; set; }
            public ulong TotalSamples { get; set; }
            public ulong Frequency { get { return TotalTimeMs / 1000 * TotalSamples; } }
        }

        public class PlaylistEntry
        {
            public string Id { get; set; }

            public override string ToString()
            {
                return Id;
            }
        }
    }
}
