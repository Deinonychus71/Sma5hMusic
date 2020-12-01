using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive.Linq;

namespace Sm5sh.GUI.ViewModels
{
    public class BgmPropertiesViewModel : ViewModelBase
    {
        [Reactive]
        public BgmEntryViewModel SelectedBgmEntry { get; set; }

        public BgmPropertiesViewModel(IObservable<BgmEntryViewModel> observableBgmEntry)
        {
            observableBgmEntry.Subscribe(o =>
            {
                if (o != null)
                    SelectedBgmEntry = o;
            });
        }
    }
}
