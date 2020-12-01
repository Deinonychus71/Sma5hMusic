using Sm5sh.Mods.Music.Models;
using System;
using System.Collections.Generic;

namespace Sm5sh.Mods.Music.Interfaces
{
    public interface IMusicMod
    {
        string Id { get; }
        string Name { get; }
        string ModPath { get; }

        MusicModInformation Mod { get; }

        void UpdateModInformation(MusicModInformation configBase);
        List<BgmEntry> GetBgms();
        BgmEntry AddBgm(string toneId, string filename);
        bool UpdateBgm(BgmEntry bgmEntry);
        bool RemoveBgm(string toneId);
    }
}
