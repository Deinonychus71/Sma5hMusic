using DynamicData;
using DynamicData.Binding;
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
        private readonly ReadOnlyObservableCollection<BgmEntryViewModel> _items;

        public ReadOnlyObservableCollection<BgmEntryViewModel> Items { get { return _items; } }

        [Reactive]
        public BgmEntryViewModel SelectedBgmEntry { get; private set; }

        [Reactive]
        public int BgmEntriesCount { get; private set; }

        public ReactiveCommand<string, Unit> ActionPlaySong { get; }

        public BgmListViewModel(IObservable<IChangeSet<BgmEntryViewModel, string>> observableBgmEntries)
        {
            observableBgmEntries
                .Sort(SortExpressionComparer<BgmEntryViewModel>.Ascending(p => p.HiddenInSoundTest).ThenByAscending(p => p.SoundTestIndex), SortOptimisations.IgnoreEvaluates)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _items)
                .DisposeMany()
                .Subscribe((o) => BgmEntriesCount = o.Count);
        }
    }
}
