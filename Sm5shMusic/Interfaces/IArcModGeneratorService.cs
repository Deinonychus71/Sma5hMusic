using Sm5shMusic.Models;
using System.Collections.Generic;

namespace Sm5shMusic.Interfaces
{
    public interface IArcModGeneratorService
    {
        bool GenerateArcMod(List<MusicModBgmEntry> bgmEntries);
    }
}
