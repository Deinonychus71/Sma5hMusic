using Sm5shMusic.Models;
using System.Collections.Generic;

namespace Sm5shMusic.Interfaces
{
    public interface IArcModGeneratorService
    {
        bool GenerateArcMusicMod(List<MusicModBgmEntry> bgmEntries);

        bool GenerateArcStagePlaylistMod(List<MusicModBgmEntry> bgmEntries, List<StagePlaylistModConfig> stageEntries);
    }
}
