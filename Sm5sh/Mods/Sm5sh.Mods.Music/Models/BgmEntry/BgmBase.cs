using Sm5sh.Mods.Music.Interfaces;

namespace Sm5sh.Mods.Music.Models
{
    public abstract class BgmBase
    {
        public EntrySource Source { get { return MusicMod == null ? EntrySource.Core : EntrySource.Mod; } }
        public IMusicMod MusicMod { get; set; }
        public string ModId { get { return MusicMod?.Mod.Id; } }


        public BgmBase(IMusicMod musicMod = null)
        {
            MusicMod = musicMod;
        }
    }

}