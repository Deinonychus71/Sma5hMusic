using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Sm5sh.GUI.Views;
using Splat;
using System;
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
            var win = new BgmPropertiesWindow()
            {
                DataContext = Locator.Current.GetService<BgmPropertiesWindowViewModel>()
            };
            win.ShowDialog<BgmPropertiesWindow>(parent);
        }
    }
}
