using ReactiveUI;
using Sm5sh.Mods.Music.Interfaces;

namespace Sm5sh.GUI.ViewModels
{
    public class ModEntryViewModel : ReactiveObject
    {
        public bool AllFlag { get; set; }
        public bool CreateFlag { get; set; }
        public IMusicMod MusicMod { get; }
        public string ModId { get; }
        public string ModName { get; set; }
        public string ModPath { get { return MusicMod.ModPath; } }

        public ModEntryViewModel() { }

        public ModEntryViewModel(string modId, IMusicMod musicMod)
        {
            ModId = modId;
            MusicMod = musicMod;
            ModName = musicMod?.Name;
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
