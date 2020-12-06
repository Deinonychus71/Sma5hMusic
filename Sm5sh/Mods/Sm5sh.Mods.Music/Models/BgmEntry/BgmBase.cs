using Sm5sh.Mods.Music.Interfaces;

namespace Sm5sh.Mods.Music.Models
{
    public abstract class BgmBase
    {
        public EntrySource Source { get; }
        public IMusicMod MusicMod { get; set; }
        public string ModId { get { return MusicMod?.Mod.Id; } }


        public BgmBase(IMusicMod musicMod = null)
        {
            MusicMod = musicMod;
            Source = musicMod != null ? EntrySource.Mod : EntrySource.Core;
        }

        public BgmBase(EntrySource source)
        {
            Source = source;
        }
    }

}