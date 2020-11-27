using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive.Linq;

namespace Sm5sh.GUI.ViewModels
{
    public class BgmPropertiesViewModel : ViewModelBase
    {
        private readonly BgmEntryViewModel _empty;

        [Reactive]
        public BgmEntryViewModel SelectedBgmEntry { get; set; }

        public BgmPropertiesViewModel(IObservable<BgmEntryViewModel> observableBgmEntry)
        {
            _empty = new BgmEntryViewModel();

            observableBgmEntry.Subscribe(o => {
                if (o != null)
                    SelectedBgmEntry = o;
                else
                    SelectedBgmEntry = _empty;
            });
        }
    }
}
