using DynamicData;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace Sm5sh.GUI.ViewModels
{
    public class BgmListViewModel : ViewModelBase
    {
        private readonly ReadOnlyObservableCollection<BgmEntryListViewModel> _items;

        public ReadOnlyObservableCollection<BgmEntryListViewModel> Items { get { return _items; } }

        public BgmListViewModel(IObservable<IChangeSet<BgmEntryListViewModel, string>> observableBgmEntries)
        {
            observableBgmEntries
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _items)
                .DisposeMany()
                .Subscribe();
        }
    }
}
