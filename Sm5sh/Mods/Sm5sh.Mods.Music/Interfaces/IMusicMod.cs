using Sm5sh.Mods.Music.Models;

namespace Sm5sh.Mods.Music.Interfaces
{
    public interface IMusicMod
    {
        string Id { get; }
        string Name { get; }
        string ModPath { get; }

        MusicModInformation Mod { get; }

        void UpdateModInformation(MusicModInformation configBase);
        MusicModEntries GetMusicModEntries();
        bool AddOrUpdateMusicModEntries(MusicModEntries musicModEntries);
        bool RemoveBgm(string toneId);
    }
}
