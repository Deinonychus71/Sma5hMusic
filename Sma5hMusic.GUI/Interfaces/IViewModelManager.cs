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
        void ReorderSongs(IEnumerable<string> bgmEntriesToReorder, short newPosition);

        IObservableCache<ModEntryViewModel, string> ObservableModsEntries { get; }
        IObservableCache<LocaleViewModel, string> ObservableLocales { get; }
        IObservableCache<SeriesEntryViewModel, string> ObservableSeries { get; }
        IObservableCache<GameTitleEntryViewModel, string> ObservableGameTitles { get; }
        IObservableCache<BgmDbRootEntryViewModel, string> ObservableDbRootEntries { get; }
        IObservableCache<BgmStreamSetEntryViewModel, string> ObservableStreamSetEntries { get; }
        IObservableCache<BgmAssignedInfoEntryViewModel, string> ObservableAssignedInfoEntries { get; }
        IObservableCache<BgmStreamPropertyEntryViewModel, string> ObservableStreamPropertyEntries { get; }
        IObservableCache<BgmPropertyEntryViewModel, string> ObservableBgmPropertyEntries { get; }
        IObservableCache<PlaylistEntryViewModel, string> ObservablePlaylistsEntries { get; }
        IObservableCache<StageEntryViewModel, string> ObservableStagesEntries { get; }

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
