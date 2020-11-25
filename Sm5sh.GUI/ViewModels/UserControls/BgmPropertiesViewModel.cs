using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive.Linq;
using VGMMusic;

namespace Sm5sh.GUI.ViewModels
{
    public class BgmPropertiesViewModel : ViewModelBase
    {
        [Reactive]
        public bool IsEditMode { get; set; }

        [Reactive]
        public BgmEntryListViewModel SelectedBgmEntry { get; set; }

        public BgmPropertiesViewModel(IObservable<BgmEntryListViewModel> observableBgmEntry)
        {
            observableBgmEntry.Subscribe(o => SelectedBgmEntry = o);
        }
    }
}
