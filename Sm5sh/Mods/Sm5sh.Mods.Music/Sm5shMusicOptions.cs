namespace Sm5sh.Mods.Music
{
    public class Sm5shMusicOptions : Sm5shOptions
    {
        public Sm5shMusicOptionsSection Sm5shMusic { get; set; }

        public class Sm5shMusicOptionsSection
        {
            public bool EnableAudioCaching { get; set; }
            public string AudioConversionFormat { get; set; }
            public string AudioConversionFormatFallBack { get; set; }
            public string DefaultLocale { get; set; }
            public string ModPath { get; set; }
            public string CachePath { get; set; }
        }
    }
}
