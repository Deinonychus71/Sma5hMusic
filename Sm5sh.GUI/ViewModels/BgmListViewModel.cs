using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using VGMMusic;

namespace Sm5sh.GUI.ViewModels
{
    public class BgmListViewModel : ViewModelBase
    {
        private readonly ReadOnlyObservableCollection<BgmEntryListViewModel> _items;

        public ReadOnlyObservableCollection<BgmEntryListViewModel> Items { get { return _items; } }

        [Reactive]
        public BgmEntryListViewModel SelectedBgmEntry { get; private set; }

        [Reactive]
        public int BgmEntriesCount { get; private set; }

        public ReactiveCommand<string, Unit> ActionPlaySong { get; }

        public BgmListViewModel(IVGMMusicPlayer musicPlayer, IObservable<IChangeSet<BgmEntryListViewModel, string>> observableBgmEntries)
        {
            observableBgmEntries
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _items)
                .DisposeMany()
                .Subscribe((o) => BgmEntriesCount = o.Count);
        }
    }
}
