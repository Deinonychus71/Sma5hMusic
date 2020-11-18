using Sm5sh.Mods.Music.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Sm5sh.GUI.ViewModels
{
    public class BgmListViewModel : ViewModelBase
    {
        public ObservableCollection<BgmEntry> Items { get; set; }

        public BgmListViewModel(IEnumerable<BgmEntry> items)
        {
            Items = new ObservableCollection<BgmEntry>(items);
        } 
    }
}
