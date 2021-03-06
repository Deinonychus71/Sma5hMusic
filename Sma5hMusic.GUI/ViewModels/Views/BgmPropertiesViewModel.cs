using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive.Linq;

namespace Sma5hMusic.GUI.ViewModels
{
    public class BgmPropertiesViewModel : ViewModelBase
    {
        [Reactive]
        public BgmDbRootEntryViewModel SelectedBgmEntry { get; set; }

        public BgmPropertiesViewModel(IObservable<BgmDbRootEntryViewModel> observableBgmEntry)
        {
            observableBgmEntry.Subscribe(o =>
            {
                if (o != null)
                    SelectedBgmEntry = o;
            });
        }
    }
}
