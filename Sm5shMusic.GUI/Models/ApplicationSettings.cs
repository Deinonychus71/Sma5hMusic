using static Sm5sh.Mods.Music.Sm5shMusicOverrideOptions;
using static Sm5shMusic.GUI.Helpers.StylesHelper;

namespace Sm5sh.Mods.Music
{
    public class ApplicationSettings : Sm5shMusicOptions
    {
        public Sm5shMusicOverrideOptionsSection Sm5shMusicOverride { get; set; }

        public Sm5shMusicGuiOptionsSection Sm5shMusicGUI { get; set; }

        public class Sm5shMusicGuiOptionsSection
        {
            public bool Advanced { get; set; }
            public UIScale UIScale { get; set; }
            public UITheme UITheme { get; set; }
            public string DefaultGUILocale { get; set; }
            public string DefaultMSBTLocale { get; set; }
            public bool CopyToEmptyLocales { get; set; }
        }
    }
}
