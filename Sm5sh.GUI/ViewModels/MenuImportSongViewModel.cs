using Avalonia.Controls;
using ReactiveUI;
using System.Reactive;

namespace Sm5sh.GUI.ViewModels
{
    public class MenuImportSongViewModel : ViewModelBase
    {
        public string ModName { get; private set; }
        public string ModPath { get; private set; }

        public MenuImportSongViewModel(string modPath, string modName)
        {
            ModName = modPath;
            ModName = modName;
        }

        public void AddNewBgmEntry(Window parent)
        {
        }
    }
}
