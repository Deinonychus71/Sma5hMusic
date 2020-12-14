using Sm5sh.Mods.Music.Models;
using System.Threading.Tasks;

namespace Sm5sh.Mods.Music.Interfaces
{
    public interface IMusicMod
    {
        string Id { get; }
        string Name { get; }
        string ModPath { get; }

        MusicModInformation Mod { get; }

        bool UpdateModInformation(MusicModInformation configBase);
        MusicModEntries GetMusicModEntries();
        Task<bool> AddOrUpdateMusicModEntries(MusicModEntries musicModEntries);
        bool RemoveMusicModEntries(MusicModDeleteEntries musicModDeleteEntries);
    }
}
