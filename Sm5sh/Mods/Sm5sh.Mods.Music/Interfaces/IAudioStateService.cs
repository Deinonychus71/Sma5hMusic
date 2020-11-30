using Sm5sh.Mods.Music.Models;
using System.Collections.Generic;

namespace Sm5sh.Mods.Music.Interfaces
{
    public interface IAudioStateService
    {
        IEnumerable<BgmEntry> GetBgmEntries();
        IEnumerable<BgmEntry> GetModBgmEntries();
        IEnumerable<GameTitleEntry> GetGameTitleEntries();
        IEnumerable<string> GetSeriesEntries();
        IEnumerable<StageEntry> GetStagesEntries();
        IEnumerable<string> GetLocales();
        IEnumerable<PlaylistEntry> GetPlaylists();
        BgmEntry GetBgmEntry(string toneId);
        bool AddBgmEntry(BgmEntry bgmEntry);
        bool RemoveBgmEntry(string toneId);
        bool AddPlaylistEntry(PlaylistEntry playlistEntry);
        bool RemovePlaylistEntry(string playlistId);
        void InitBgmEntriesFromStateManager();
        bool SaveBgmEntriesToStateManager();
    }
}
