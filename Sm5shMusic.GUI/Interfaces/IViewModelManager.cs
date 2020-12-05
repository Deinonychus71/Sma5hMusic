using DynamicData;
using Sm5sh.Mods.Music.Models;
using Sm5shMusic.GUI.ViewModels;
using System;
using System.Collections.Generic;

namespace Sm5shMusic.GUI.Interfaces
{
    public interface IViewModelManager
    {
        void Init();
        void ReorderSongs();

        IObservable<IChangeSet<LocaleViewModel, string>> ObservableLocales { get; }
        IObservable<IChangeSet<SeriesEntryViewModel, string>> ObservableSeries { get; }
        IObservable<IChangeSet<GameTitleEntryViewModel, string>> ObservableGameTitles { get; }
        IObservable<IChangeSet<BgmDbRootEntryViewModel, string>> ObservableDbRootEntries { get; }
        IObservable<IChangeSet<BgmStreamSetEntryViewModel, string>> ObservableStreamSetEntries { get; }
        IObservable<IChangeSet<BgmAssignedInfoEntryViewModel, string>> ObservableAssignedInfoEntries { get; }
        IObservable<IChangeSet<BgmStreamPropertyEntryViewModel, string>> ObservableStreamPropertyEntries { get; }
        IObservable<IChangeSet<BgmPropertyEntryViewModel, string>> ObservableBgmPropertyEntries { get; }
        IObservable<IChangeSet<PlaylistEntryViewModel, string>> ObservablePlaylistsEntries { get; }
        IObservable<IChangeSet<StageEntryViewModel, string>> ObservableStagesEntries { get; }
        IObservable<IChangeSet<ModEntryViewModel, string>> ObservableModsEntries { get; }

        IEnumerable<LocaleViewModel> GetLocalesViewModels();
        IEnumerable<SeriesEntryViewModel> GetSeriesViewModels();
        IEnumerable<GameTitleEntryViewModel> GetGameTitlesViewModels();
        IEnumerable<BgmDbRootEntryViewModel> GetBgmDbRootEntriesViewModels();
        IEnumerable<BgmStreamSetEntryViewModel> GetBgmStreamSetEntriesViewModels();
        IEnumerable<BgmAssignedInfoEntryViewModel> GetBgmAssignedInfoEntriesViewModels();
        IEnumerable<BgmStreamPropertyEntryViewModel> GetBgmStreamPropertyEntriesViewModels();
        IEnumerable<BgmPropertyEntryViewModel> GetBgmPropertyEntriesViewModels();
        IEnumerable<PlaylistEntryViewModel> GetPlaylistsEntriesViewModels();
        IEnumerable<StageEntryViewModel> GetStagesEntriesViewModels();

        GameTitleEntryViewModel GetGameTitleViewModel(string uiGameTitleId);
        SeriesEntryViewModel GetSeriesViewModel(string uiSeriesId);
        BgmDbRootEntryViewModel GetBgmDbRootViewModel(string uiBgmId);
        BgmStreamSetEntryViewModel GetBgmStreamSetViewModel(string streamSetId);
        BgmAssignedInfoEntryViewModel GetBgmAssignedInfoViewModel(string assignedInfoId);
        BgmStreamPropertyEntryViewModel GetBgmStreamPropertyViewModel(string streamId);
        BgmPropertyEntryViewModel GetBgmPropertyViewModel(string nameId);

        void RemoveGameTitleView(string uiGameTitleId);
        void RemoveBgmDbRootView(string uiBgmId);
        void RemoveBgmStreamSetView(string streamSetId);
        void RemoveBgmAssignedInfoView(string infoId);
        void RemoveBgmStreamPropertyView(string streamId);
        void RemoveBgmPropertyView(string nameId);
        void RemoveBgmInAllPlaylists(string uiBgmId);

        bool AddNewGameTitleEntryViewModel(GameTitleEntry gameTitleEntry);
    }
}
