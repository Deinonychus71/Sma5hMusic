using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using VGMMusic;

namespace Sm5sh.GUI.ViewModels
{
    public class BgmPropertiesViewModel : ViewModelBase
    {
        [Reactive]
        public bool IsEditMode { get; set; }

        [Reactive]
        public BgmEntryViewModel SelectedBgmEntry { get; set; }

        public BgmPropertiesViewModel(IObservable<BgmEntryViewModel> observableBgmEntry)
        {

            observableBgmEntry.Subscribe(o => {
                if(o != null)
                    SelectedBgmEntry = o;
            });
        }
    }
}
