using Sma5h.Mods.Music;
using Sma5h.Mods.Music.Interfaces;
using Sma5h.Mods.Music.Models;
using System.Threading.Tasks;

namespace Sma5hMusic.GUI.Interfaces
{
    public interface IGUIStateManager
    {
        #region Music Mod Entries (Hackish)
        Task<string> CreateNewMusicModFromToneId(string toneId, string filename, IMusicMod musicMod);
        Task<bool> RenameMusicModToneId(MusicModEntries musicModEntries, IMusicMod musicMod, string newToneId);
        Task<bool> MoveMusicModEntrySetToAnotherMod(MusicModEntries musicModEntries, IMusicMod fromMusicMod, IMusicMod toMusicMod);
        #endregion

        Task<string> CreateNewMusicMod(MusicModEntries musicModEntries, IMusicMod musicMod);
        Task<bool> CanAddMusicModEntries(MusicModEntries musicModEntries);
        Task<bool> PersistMusicModEntryChanges(MusicModEntries musicModEntries, IMusicMod musicMod = null);
        Task<bool> RemoveMusicModEntries(MusicModDeleteEntries musicModDeleteEntries, IMusicMod musicMod = null);
        
        Task<string> CreateNewGameTitleEntry(GameTitleEntry gameTitleEntry);
        Task<bool> PersistGameTitleEntryChange(GameTitleEntry gameTitleEntry);
        Task<bool> RemoveGameTitleEntry(string gameTitleId);

        Task<string> CreateNewModEntry(MusicModInformation musicModInformation, string modPath);
        Task<bool> PersistModInformationChange(IMusicMod musicMod, MusicModInformation musicModInformation);

        Task<string> CreateNewPlaylist(PlaylistEntry playlistEntry);
        Task<bool> PersistPlaylistChanges();
        Task<bool> RemovePlaylist(string playlistId);
        Task<bool> PersistStageChanges();

        Task<bool> UpdateGlobalSettings(ApplicationSettings appSettings);
        Task<bool> WipeAudioCache();
        Task<bool> ReorderSongs();

        string GameVersion { get; }

        bool IsGameVersionFound { get; }
    }
}
