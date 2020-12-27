using Sm5sh.Mods.Music.Models;
using System.Collections.Generic;

namespace Sm5sh.Mods.Music.Interfaces
{
    public interface IAudioStateService
    {
        IEnumerable<BgmDbRootEntry> GetBgmDbRootEntries();
        IEnumerable<BgmDbRootEntry> GetModBgmDbRootEntries();
        IEnumerable<BgmStreamSetEntry> GetBgmStreamSetEntries();
        IEnumerable<BgmStreamSetEntry> GetModBgmStreamSetEntries();
        IEnumerable<BgmAssignedInfoEntry> GetBgmAssignedInfoEntries();
        IEnumerable<BgmAssignedInfoEntry> GetModBgmAssignedInfoEntries();
        IEnumerable<BgmStreamPropertyEntry> GetBgmStreamPropertyEntries();
        IEnumerable<BgmStreamPropertyEntry> GetModBgmStreamPropertyEntries();
        IEnumerable<BgmPropertyEntry> GetBgmPropertyEntries();
        IEnumerable<BgmPropertyEntry> GetModBgmPropertyEntries();
        IEnumerable<GameTitleEntry> GetGameTitleEntries();
        IEnumerable<string> GetSeriesEntries();
        IEnumerable<StageEntry> GetStagesEntries();
        IEnumerable<string> GetLocales();
        IEnumerable<PlaylistEntry> GetPlaylists();

        double GameVersion { get; }

        bool CanAddBgmDbRootEntry(string uiBgmId);
        bool CanAddBgmStreamSetEntry(string streamSetId);
        bool CanAddBgmAssignedInfoEntry(string infoId);
        bool CanAddBgmStreamPropertyEntry(string streamId);
        bool CanAddBgmPropertyEntry(string nameId);
        bool CanAddGameTitleEntry(string uiGameTitleId);
        bool AddBgmDbRootEntry(BgmDbRootEntry bgmDbRootEntry);
        bool AddBgmStreamSetEntry(BgmStreamSetEntry bgmStreamSetEntry);
        bool AddBgmAssignedInfoEntry(BgmAssignedInfoEntry bgmAssignedInfoEntry);
        bool AddBgmStreamPropertyEntry(BgmStreamPropertyEntry bgmStreamPropertyEntry);
        bool AddBgmPropertyEntry(BgmPropertyEntry bgmPropertyEntry);
        bool AddGameTitleEntry(GameTitleEntry gameTitleEntry);
        bool AddPlaylistEntry(PlaylistEntry playlistEntry);

        bool RemoveBgmDbRootEntry(string uiBgmId);
        bool RemoveBgmStreamSetEntry(string streamSetId);
        bool RemoveBgmAssignedInfoEntry(string infoId);
        bool RemoveBgmStreamPropertyEntry(string streamId);
        bool RemoveBgmPropertyEntry(string nameId);
        bool RemoveGameTitleEntry(string uiGameTitleId);
        bool RemovePlaylistEntry(string playlistId);

        void InitBgmEntriesFromStateManager();
        bool SaveBgmEntriesToStateManager();
    }
}
