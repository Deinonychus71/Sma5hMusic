using DynamicData;
using Sma5h.Mods.Music.Interfaces;
using Sma5h.Mods.Music.Models;
using Sma5hMusic.GUI.ViewModels;
using System;
using System.Collections.Generic;

namespace Sma5hMusic.GUI.Interfaces
{
    public interface IViewModelManager
    {
        void Init();
        void ReorderSongs();

        IObservable<IChangeSet<ModEntryViewModel, string>> ObservableModsEntries { get; }
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

        ModEntryViewModel GetModEntryViewModel(string modId);
        GameTitleEntryViewModel GetGameTitleViewModel(string uiGameTitleId);
        SeriesEntryViewModel GetSeriesViewModel(string uiSeriesId);
        BgmDbRootEntryViewModel GetBgmDbRootViewModel(string uiBgmId);
        BgmStreamSetEntryViewModel GetBgmStreamSetViewModel(string streamSetId);
        BgmAssignedInfoEntryViewModel GetBgmAssignedInfoViewModel(string assignedInfoId);
        BgmStreamPropertyEntryViewModel GetBgmStreamPropertyViewModel(string streamId);
        BgmPropertyEntryViewModel GetBgmPropertyViewModel(string nameId);
        PlaylistEntryViewModel GetPlaylistViewModel(string playlistId);

        void RemoveGameTitleView(string uiGameTitleId);
        void RemoveBgmDbRootView(string uiBgmId);
        void RemoveBgmStreamSetView(string streamSetId);
        void RemoveBgmAssignedInfoView(string infoId);
        void RemoveBgmStreamPropertyView(string streamId);
        void RemoveBgmPropertyView(string nameId);
        void RemovePlaylist(string playlistId);
        void RemoveBgmInAllPlaylists(string uiBgmId);

        bool AddNewModEntryViewModel(IMusicMod musicMod);
        bool AddNewGameTitleEntryViewModel(GameTitleEntry gameTitleEntry);
        bool AddNewBgmDbRootEntryViewModel(BgmDbRootEntry bgmDbRootEntry);
        bool AddNewBgmStreamSetEntryViewModel(BgmStreamSetEntry bgmStreamSetEntry);
        bool AddNewBgmAssignedInfoEntryViewModel(BgmAssignedInfoEntry bgmAssignedInfoEntry);
        bool AddNewBgmStreamPropertyEntryViewModel(BgmStreamPropertyEntry bgmStreamPropertyEntry);
        bool AddNewBgmPropertyEntryViewModel(BgmPropertyEntry bgmPropertyEntry);
        bool AddNewPlaylistEntryViewModel(PlaylistEntry playlistEntry);
    }
}
