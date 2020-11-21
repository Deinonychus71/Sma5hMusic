using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Sm5sh.GUI.ViewModels
{
    public class BgmFiltersViewModel : ViewModelBase
    {
        private readonly ReadOnlyObservableCollection<BgmEntryListViewModel> _series;
        private readonly ReadOnlyObservableCollection<BgmEntryListViewModel> _games;
        private IChangeSet<BgmEntryListViewModel, string> _allChangeSet;

        public ReadOnlyObservableCollection<BgmEntryListViewModel> Series { get { return _series; } }
        public ReadOnlyObservableCollection<BgmEntryListViewModel> Games { get { return _games; } }
        [Reactive]
        public BgmEntryListViewModel SelectedSeries { get; set; }
        [Reactive]
        public BgmEntryListViewModel SelectedGame { get; set; }
        public IObservable<IChangeSet<BgmEntryListViewModel, string>> WhenFiltersAreApplied { get; }

        public BgmFiltersViewModel(IObservable<IChangeSet<BgmEntryListViewModel, string>> observableBgmEntries)
        {
            _allChangeSet = GetAllChangeSet();

            var whenAnyPropertyChanged = this.WhenAnyPropertyChanged("SelectedSeries", "SelectedGame");
            WhenFiltersAreApplied = observableBgmEntries
                .AutoRefreshOnObservable(p => whenAnyPropertyChanged)
                .Filter(p =>
                    (SelectedSeries == null || SelectedSeries.AllFlag || p.SeriesId == SelectedSeries.SeriesId) &&
                    (SelectedGame == null || SelectedGame.AllFlag || p.GameId == SelectedGame.GameId)
                );


            var seriesChanged = observableBgmEntries.WhenValueChanged(species => species.SeriesId);
            observableBgmEntries
                .Filter(p => p.SeriesId != null)
                .Group(p => p.SeriesId, seriesChanged.Select(_ => Unit.Default))
                .Transform(p => p.Cache.Items.First())
                .Prepend(_allChangeSet)
                .Sort(SortExpressionComparer<BgmEntryListViewModel>.Descending(p => p.AllFlag).ThenByAscending(p => p.SeriesId), SortOptimisations.IgnoreEvaluates)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _series)
                .DisposeMany()
                .Subscribe();

            var gameChanged = observableBgmEntries.WhenValueChanged(species => species.SeriesId);
            observableBgmEntries
                .AutoRefreshOnObservable(p => this.WhenAnyValue(p => p.SelectedSeries))
                .Filter(p => p.SeriesId != null && p.GameId != null && (SelectedSeries == null || SelectedSeries.AllFlag || p.SeriesId == SelectedSeries.SeriesId))
                .Group(p => p.GameId, gameChanged.Select(_ => Unit.Default))
                .Transform(p => p.Cache.Items.First())
                .Prepend(_allChangeSet)
                .Sort(SortExpressionComparer<BgmEntryListViewModel>.Ascending(p => p.GameTitle), SortOptimisations.IgnoreEvaluates)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _games)
                .DisposeMany()
                .Subscribe();

            this.WhenAnyValue(p => p.SelectedSeries).Subscribe((o) => SelectedGame = null);
        }

        private IChangeSet<BgmEntryListViewModel, string> GetAllChangeSet()
        {
            return new ChangeSet<BgmEntryListViewModel, string>(new List<Change<BgmEntryListViewModel, string>>()
            {
                new Change<BgmEntryListViewModel, string>(ChangeReason.Add, "-1", new BgmEntryListViewModel()
                {
                    AllFlag = true, 
                    SeriesId = "All",
                    SeriesTitle = "All", 
                    GameId = "All",
                    GameTitle = "All"
                })
            });
        }
    }
}
