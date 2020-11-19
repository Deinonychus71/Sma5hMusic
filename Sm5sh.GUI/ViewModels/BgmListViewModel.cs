using Sm5sh.Mods.Music.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Sm5sh.GUI.ViewModels
{
    public class BgmListViewModel : ViewModelBase
    {
        public ObservableCollection<BgmEntry> Items { get; set; }

        public string Locale { get; set; }

        public BgmListViewModel(IEnumerable<BgmEntry> items)
        {
            Locale = "us_en";
            Items = new ObservableCollection<BgmEntry>(items);
        }
    }
}
