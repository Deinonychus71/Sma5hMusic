﻿using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sma5h.Mods.Music.Helpers;
using Sma5h.Mods.Music.Models;
using Sma5hMusic.GUI.Helpers;
using Sma5hMusic.GUI.Interfaces;
using Sma5hMusic.GUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Sma5hMusic.GUI.ViewModels
{
    public class BgmFiltersViewModel : ViewModelBase
    {
        private readonly ReadOnlyObservableCollection<SeriesEntryViewModel> _series;
        private readonly ReadOnlyObservableCollection<GameTitleEntryViewModel> _games;
        private readonly ReadOnlyObservableCollection<ModEntryViewModel> _mods;
        private readonly List<ComboItem> _recordTypes;
        private readonly IChangeSet<ModEntryViewModel, string> _allModsChangeSet;
        private readonly IChangeSet<SeriesEntryViewModel, string> _allSeriesChangeSet;
        private readonly IChangeSet<GameTitleEntryViewModel, string> _allGameTitleChangeSet;

        public ReadOnlyObservableCollection<SeriesEntryViewModel> Series { get { return _series; } }
        public ReadOnlyObservableCollection<GameTitleEntryViewModel> Games { get { return _games; } }
        public ReadOnlyObservableCollection<ModEntryViewModel> Mods { get { return _mods; } }
        public IEnumerable<ComboItem> RecordTypes { get { return _recordTypes; } }

        [Reactive]
        public string SearchText { get; set; }
        [Reactive]
        public ModEntryViewModel SelectedMod { get; set; }
        [Reactive]
        public SeriesEntryViewModel SelectedSeries { get; set; }
        [Reactive]
        public GameTitleEntryViewModel SelectedGame { get; set; }
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

        public IObservable<IChangeSet<BgmDbRootEntryViewModel, string>> WhenFiltersAreApplied { get; }


        public BgmFiltersViewModel(IViewModelManager viewModelManager)
        {
            _allModsChangeSet = GetAllModsChangeSet();
            _allSeriesChangeSet = GetAllSeriesChangeSet();
            _allGameTitleChangeSet = GetAllGameTitleChangeSet();
            _recordTypes = GetRecordTypes();

            var observableBgmEntries = viewModelManager.ObservableDbRootEntries.Connect()
                .DeferUntilLoaded()
                .Filter(p => p.UiBgmId != MusicConstants.InternalIds.BGM_ID_RANDOM);

            var whenAnyPropertyChanged = this.WhenAnyPropertyChanged("SelectedSeries", "SelectedGame",
                "SelectedRecordType", "SelectedMod", "SelectedShowInSoundTest", "SelectedShowHiddenSongs",
                "SelectedCharacterVictorySongs", "SelectedPinchSongs", "SelectedCoreSongs", "SelectedModSongs", "SearchText");
            WhenFiltersAreApplied = observableBgmEntries
                .AutoRefresh(p => p.TestDispOrder, TimeSpan.FromMilliseconds(50))
                .AutoRefreshOnObservable(p => whenAnyPropertyChanged, changeSetBuffer: TimeSpan.FromMilliseconds(50), scheduler: RxApp.TaskpoolScheduler)
                .Filter(p =>
                    (SelectedShowHiddenSongs || (!SelectedShowHiddenSongs && !p.HiddenInSoundTest)) &&
                    (SelectedShowInSoundTest || (!SelectedShowInSoundTest && p.HiddenInSoundTest)) &&
                    (SelectedModSongs || (!SelectedModSongs && p.Source == EntrySource.Core)) &&
                    (SelectedCoreSongs || (!SelectedCoreSongs && p.Source == EntrySource.Mod)) &&
                    (SelectedMod == null || SelectedMod.DefaultFlag || p.ModId == SelectedMod.Id) &&
                    (SelectedRecordType == null || SelectedRecordType.DefaultFlag || p.RecordType == SelectedRecordType.Id) &&
                    (SelectedSeries == null || SelectedSeries.AllFlag || p.SeriesId == SelectedSeries.UiSeriesId) &&
                    (SelectedGame == null || SelectedGame.AllFlag || p.UiGameTitleId == SelectedGame.UiGameTitleId) &&
                    (string.IsNullOrEmpty(SearchText) || p.ToneId.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || p.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                );

            var modsChanged = observableBgmEntries.WhenValueChanged(mod => mod.MusicModViewModel.Name);
            observableBgmEntries
                .Throttle(TimeSpan.FromMilliseconds(50))
                .Filter(p => p.MusicMod != null)
                .Group(p => p.ModId, modsChanged.Select(_ => Unit.Default))
                .Transform(p => p.Cache.Items.First().MusicModViewModel)
                .Prepend(_allModsChangeSet)
                .Sort(SortExpressionComparer<ModEntryViewModel>.Descending(p => p.DefaultFlag).ThenByAscending(p => p.Name), SortOptimisations.IgnoreEvaluates)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _mods)
                .DisposeMany()
                .Subscribe((o) => SelectedMod = _allModsChangeSet.First().Current);

            var seriesChanged = observableBgmEntries.WhenValueChanged(series => series.SeriesId);
            observableBgmEntries
                .Throttle(TimeSpan.FromMilliseconds(50))
                .Filter(p => p.SeriesId != null)
                .AutoRefresh(p => p.GameTitleViewModel.UiSeriesId, TimeSpan.FromMilliseconds(50))
                .Group(p => p.SeriesId, seriesChanged.Select(_ => Unit.Default))
                .Transform(p => p.Cache.Items.First().SeriesViewModel)
                .AutoRefresh(p => p.Title, TimeSpan.FromMilliseconds(50))
                .Prepend(_allSeriesChangeSet)
                .Sort(SortExpressionComparer<SeriesEntryViewModel>.Descending(p => p.AllFlag).ThenByAscending(p => p.Title), SortOptimisations.ComparesImmutableValuesOnly)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _series)
                .DisposeMany()
                .Subscribe((o) => SelectedSeries = _allSeriesChangeSet.First().Current);

            var gameChanged = observableBgmEntries.WhenValueChanged(game => game.UiGameTitleId);
            observableBgmEntries
                .Throttle(TimeSpan.FromMilliseconds(50))
                .AutoRefreshOnObservable(p => this.WhenAnyValue(p => p.SelectedSeries))
                .Filter(p => p.SeriesId != null && p.UiGameTitleId != null && (SelectedSeries == null || SelectedSeries.AllFlag || p.SeriesId == SelectedSeries.UiSeriesId))
                .Group(p => p.UiGameTitleId, gameChanged.Select(_ => Unit.Default))
                .Transform(p => p.Cache.Items.First().GameTitleViewModel)
                .AutoRefresh(p => p.Title, TimeSpan.FromMilliseconds(50))
                .Prepend(_allGameTitleChangeSet)
                .Sort(SortExpressionComparer<GameTitleEntryViewModel>.Descending(p => p.AllFlag).ThenByAscending(p => p.Title), SortOptimisations.ComparesImmutableValuesOnly)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _games)
                .DisposeMany()
                .Subscribe();

            SelectedShowInSoundTest = true;
            SelectedModSongs = true;
            SelectedCoreSongs = false;
            SelectedRecordType = _recordTypes[0];
            this.WhenAnyValue(p => p.SelectedSeries).Subscribe((o) => SelectedGame = _allGameTitleChangeSet.First().Current);
        }

        private IChangeSet<SeriesEntryViewModel, string> GetAllSeriesChangeSet()
        {
            return new ChangeSet<SeriesEntryViewModel, string>(new List<Change<SeriesEntryViewModel, string>>()
            {
                new Change<SeriesEntryViewModel, string>(ChangeReason.Add, "-1", new SeriesEntryViewModel()
                {
                    AllFlag = true,
                    Title = "All"
                })
            });
        }

        private IChangeSet<GameTitleEntryViewModel, string> GetAllGameTitleChangeSet()
        {
            return new ChangeSet<GameTitleEntryViewModel, string>(new List<Change<GameTitleEntryViewModel, string>>()
            {
                new Change<GameTitleEntryViewModel, string>(ChangeReason.Add, "-1", new GameTitleEntryViewModel()
                {
                    AllFlag = true,
                    Title = "All"
                })
            });
        }

        private IChangeSet<ModEntryViewModel, string> GetAllModsChangeSet()
        {
            return new ChangeSet<ModEntryViewModel, string>(new List<Change<ModEntryViewModel, string>>()
            {
                new Change<ModEntryViewModel, string>(ChangeReason.Add, "-1", new ModEntryViewModel()
                {
                    DefaultFlag = true,
                    Name = "All"
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
