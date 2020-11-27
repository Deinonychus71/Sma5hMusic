using Sm5sh.Mods.Music.Interfaces;

namespace Sm5sh.GUI.ViewModels
{
    public class ModEntryViewModel : ViewModelBase
    {
        public bool AllFlag { get; set; }
        public IMusicMod MusicMod { get; }
        public string ModId { get;  }
        public string ModName { get; set; }

        public ModEntryViewModel() { }

        public ModEntryViewModel(string modId, IMusicMod musicMod)
        {
            ModId = modId;
            MusicMod = musicMod;
            ModName = musicMod.Name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            ModEntryViewModel p = obj as ModEntryViewModel;
            if (p == null)
                return false;

            return p.ModId == this.ModId;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
