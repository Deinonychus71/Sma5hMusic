using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sm5sh.GUI.Helpers;
using Sm5sh.GUI.Models;
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
        private readonly ReadOnlyObservableCollection<BgmEntryListViewModel> _mods;
        private readonly List<ComboItem> _recordTypes;
        private IChangeSet<BgmEntryListViewModel, string> _allChangeSet;

        public ReadOnlyObservableCollection<BgmEntryListViewModel> Series { get { return _series; } }
        public ReadOnlyObservableCollection<BgmEntryListViewModel> Games { get { return _games; } }
        public ReadOnlyObservableCollection<BgmEntryListViewModel> Mods { get { return _mods; } }
        public IEnumerable<ComboItem> RecordTypes { get { return _recordTypes; } }

        [Reactive]
        public BgmEntryListViewModel SelectedMod { get; set; }
        [Reactive]
        public BgmEntryListViewModel SelectedSeries { get; set; }
        [Reactive]
        public BgmEntryListViewModel SelectedGame { get; set; }
        [Reactive]
        public ComboItem SelectedRecordType { get; set; }
        [Reactive]
        public bool SelectedShowInSoundTest { get; set; }
        [Reactive]
        public bool SelectedShowHiddenSongs { get; set; }
        [Reactive]
        public bool SelectedCharacterVictorySongs { get; set; }
        [Reactive]
        public bool SelectedPinchSongs { get; set; }
        [Reactive]
        public bool SelectedCoreSongs { get; set; }
        [Reactive]
        public bool SelectedModSongs { get; set; }

        public IObservable<IChangeSet<BgmEntryListViewModel, string>> WhenFiltersAreApplied { get; }


        public BgmFiltersViewModel(IObservable<IChangeSet<BgmEntryListViewModel, string>> observableBgmEntries)
        {
            _allChangeSet = GetAllChangeSet();
            _recordTypes = GetRecordTypes();

            var whenAnyPropertyChanged = this.WhenAnyPropertyChanged("SelectedSeries", "SelectedGame",
                "SelectedRecordType", "SelectedMod", "SelectedShowInSoundTest", "SelectedShowHiddenSongs", 
                "SelectedCharacterVictorySongs", "SelectedPinchSongs", "SelectedCoreSongs", "SelectedModSongs");
            WhenFiltersAreApplied = observableBgmEntries
                .AutoRefreshOnObservable(p => whenAnyPropertyChanged)
                .Filter(p =>
                    (SelectedShowHiddenSongs || (!SelectedShowHiddenSongs && !p.HiddenInSoundTest)) &&
                    (SelectedShowInSoundTest || (!SelectedShowInSoundTest && p.HiddenInSoundTest)) &&
                    (SelectedModSongs || (!SelectedModSongs && p.Source == Sm5sh.Mods.Music.Models.BgmEntryModels.EntrySource.Core)) &&
                    (SelectedCoreSongs || (!SelectedCoreSongs && p.Source == Sm5sh.Mods.Music.Models.BgmEntryModels.EntrySource.Mod)) &&
                    (SelectedMod == null || SelectedMod.AllFlag || p.ModName == SelectedMod.ModName) &&
                    (SelectedRecordType == null || SelectedRecordType.AllFlag || p.RecordTypeId == SelectedRecordType.Id) &&
                    (SelectedSeries == null || SelectedSeries.AllFlag || p.SeriesId == SelectedSeries.SeriesId) &&
                    (SelectedGame == null || SelectedGame.AllFlag || p.GameId == SelectedGame.GameId)
                );

            var modsChanged = observableBgmEntries.WhenValueChanged(species => species.ModName);
            observableBgmEntries
                .Filter(p => p.ModName != null)
                .Group(p => p.ModName, modsChanged.Select(_ => Unit.Default))
                .Transform(p => p.Cache.Items.First())
                .Prepend(_allChangeSet)
                .Sort(SortExpressionComparer<BgmEntryListViewModel>.Descending(p => p.AllFlag).ThenByAscending(p => p.ModName), SortOptimisations.IgnoreEvaluates)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _mods)
                .DisposeMany()
                .Subscribe((o) => SelectedMod = _allChangeSet.First().Current);

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
                .Subscribe((o) => SelectedSeries = _allChangeSet.First().Current);

            var gameChanged = observableBgmEntries.WhenValueChanged(species => species.SeriesId);
            observableBgmEntries
                .AutoRefreshOnObservable(p => this.WhenAnyValue(p => p.SelectedSeries))
                .Filter(p => p.SeriesId != null && p.GameId != null && (SelectedSeries == null || SelectedSeries.AllFlag || p.SeriesId == SelectedSeries.SeriesId))
                .Group(p => p.GameId, gameChanged.Select(_ => Unit.Default))
                .Transform(p => p.Cache.Items.First())
                .Prepend(_allChangeSet)
                .Sort(SortExpressionComparer<BgmEntryListViewModel>.Descending(p => p.AllFlag).ThenByAscending(p => p.GameTitle), SortOptimisations.IgnoreEvaluates)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _games)
                .DisposeMany()
                .Subscribe();

            SelectedShowInSoundTest = true;
            SelectedModSongs = true;
            SelectedCoreSongs = true;
            SelectedRecordType = _recordTypes[0];
            this.WhenAnyValue(p => p.SelectedSeries).Subscribe((o) => SelectedGame = _allChangeSet.First().Current);
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
                    GameTitle = "All",
                    ModName = "All"
                })
            });
        }

        private List<ComboItem> GetRecordTypes()
        {
            var recordTypes = new List<ComboItem>() { new ComboItem("All", "All", true) };
            recordTypes.AddRange(Constants.CONVERTER_RECORD_TYPE.Select(p => new ComboItem(p.Key, p.Value)));
            return recordTypes;
        }
    }
}
