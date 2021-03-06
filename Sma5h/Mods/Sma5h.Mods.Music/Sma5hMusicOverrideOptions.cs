namespace Sma5h.Mods.Music
{
    public class Sma5hMusicOverrideOptions : Sma5hOptions
    {
        public Sma5hMusicOverrideOptionsSection Sma5hMusicOverride { get; set; }

        public class Sma5hMusicOverrideOptionsSection
        {
            public string ModPath { get; set; }
        }
    }
}
