using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Models;
using System.Threading.Tasks;

namespace Sm5shMusic.GUI.Interfaces
{
    public interface IGUIStateManager
    {
        Task UpdateMusicModEntries(MusicModEntries musicModEntries, IMusicMod musicMod = null);
        Task RemoveMusicModEntries(MusicModDeleteEntries musicModDeleteEntries, IMusicMod musicMod = null);

        Task CreateNewGameTitleEntry(GameTitleEntry gameTitleEntry);
        Task UpdateGameTitleEntry(GameTitleEntry gameTitleEntry);
    }
}
