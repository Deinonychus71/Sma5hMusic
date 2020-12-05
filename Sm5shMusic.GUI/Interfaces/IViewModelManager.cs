using Sm5shMusic.GUI.ViewModels;
using System.Collections.Generic;

namespace Sm5shMusic.GUI.Interfaces
{
    public interface IViewModelManager
    {
        void Init();
        void ReorderSongs();

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
    }
}
