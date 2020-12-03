namespace Sm5sh.Mods.Music
{
    public class Sm5shMusicOverrideOptions : Sm5shOptions
    {
        public Sm5shMusicOverrideOptionsSection Sm5shMusicOverride { get; set; }

        public class Sm5shMusicOverrideOptionsSection
        {
            public string ModPath { get; set; }
        }
    }
}
