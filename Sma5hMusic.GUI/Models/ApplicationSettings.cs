using static Sma5h.Mods.Music.Sma5hMusicOverrideOptions;
using static Sma5hMusic.GUI.Helpers.StylesHelper;

namespace Sma5h.Mods.Music
{
    public class ApplicationSettings : Sma5hMusicOptions
    {
        public Sma5hMusicOverrideOptionsSection Sma5hMusicOverride { get; set; }

        public Sma5hMusicGuiOptionsSection Sma5hMusicGUI { get; set; }

        public class Sma5hMusicGuiOptionsSection
        {
            public bool Advanced { get; set; }
            public UIScale UIScale { get; set; }
            public UITheme UITheme { get; set; }
            public string DefaultGUILocale { get; set; }
            public string DefaultMSBTLocale { get; set; }
            public bool CopyToEmptyLocales { get; set; }
            public ushort PlaylistIncidenceDefault { get; set; }
            public bool SkipWarningGameVersion { get; set; }
            public bool InGameVolume { get; set; }
            public bool HideIndexColumn { get; set; }
            public bool HideSeriesColumn { get; set; }
            public bool HideRecordColumn { get; set; }
            public bool HideModColumn { get; set; }
            
        }
    }
}
