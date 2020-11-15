namespace Sm5sh.Mods.StagePlaylist
{
    public class Sm5shStagePlaylistOptions : Sm5shOptions
    {
        public Sm5shStagePlaylistOptionsSection Sm5shStagePlaylist { get; set; }

        public class Sm5shStagePlaylistOptionsSection
        {
            public string ModFile { get; set; }
        }
    }
}
