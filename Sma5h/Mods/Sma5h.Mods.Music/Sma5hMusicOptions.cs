using System.Collections.Generic;

namespace Sma5h.Mods.Music
{
    public class Sma5hMusicOptions : Sma5hOptions
    {
        public Sma5hMusicOptionsSection Sma5hMusic { get; set; }

        public class Sma5hMusicOptionsSection
        {
            public Sma5hMusicOptionsAutoPlaylistsSection PlaylistMapping { get; set; }
            public bool EnableAudioCaching { get; set; }
            public string AudioConversionFormat { get; set; }
            public string AudioConversionFormatFallBack { get; set; }
            public string DefaultLocale { get; set; }
            public string ModPath { get; set; }
            public string CachePath { get; set; }
        }

        public class Sma5hMusicOptionsAutoPlaylistsSection
        {
            public bool Enabled { get; set; }
            public ushort Incidence { get; set; }
            public Dictionary<string, string> Mapping { get; set; }
        }
    }
}
