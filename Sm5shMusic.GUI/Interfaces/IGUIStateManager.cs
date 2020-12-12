using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Models;
using System.Threading.Tasks;

namespace Sm5shMusic.GUI.Interfaces
{
    public interface IGUIStateManager
    {
        Task<string> CreateNewMusicModFromToneId(string toneId, string filename, IMusicMod musicMod);
        Task<string> CreateNewMusicMod(MusicModEntries musicModEntries, IMusicMod musicMod);
        Task<bool> CanAddMusicModEntries(MusicModEntries musicModEntries);
        Task<bool> PersistMusicModEntryChanges(MusicModEntries musicModEntries, IMusicMod musicMod = null);
        Task<bool> RemoveMusicModEntries(MusicModDeleteEntries musicModDeleteEntries, IMusicMod musicMod = null);

        Task<string> CreateNewGameTitleEntry(GameTitleEntry gameTitleEntry);
        Task<bool> PersistGameTitleEntryChange(GameTitleEntry gameTitleEntry);

        Task<string> CreateNewModEntry(MusicModInformation musicModInformation, string modPath);
        Task<bool> PersistModInformationChange(IMusicMod musicMod, MusicModInformation musicModInformation);

        Task<string> CreateNewPlaylist(PlaylistEntry playlistEntry);
        Task<bool> PersistPlaylistChanges();
        Task<bool> RemovePlaylist(string playlistId);
        Task<bool> PersistStageChanges();
        void ReorderSongs();
    }
}
