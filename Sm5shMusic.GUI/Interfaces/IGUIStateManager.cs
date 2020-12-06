using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Models;
using System.Threading.Tasks;

namespace Sm5shMusic.GUI.Interfaces
{
    public interface IGUIStateManager
    {
        Task UpdateMusicModEntries(MusicModEntries musicModEntries, IMusicMod musicMod = null);
        Task RemoveMusicModEntries(MusicModDeleteEntries musicModDeleteEntries, IMusicMod musicMod = null);

        Task<string> CreateNewGameTitleEntry(GameTitleEntry gameTitleEntry);
        Task UpdateGameTitleEntry(GameTitleEntry gameTitleEntry);

        Task<string> CreateNewModEntry(MusicModInformation musicModInformation, string modPath);
        Task UpdateModEntry(IMusicMod musicMod, MusicModInformation musicModInformation);

        Task<string> CreateNewPlaylists(PlaylistEntry playlistEntry);
        Task UpdatePlaylists();
        Task RemovePlaylist(string playlistId);
        Task UpdateStages();
    }
}
