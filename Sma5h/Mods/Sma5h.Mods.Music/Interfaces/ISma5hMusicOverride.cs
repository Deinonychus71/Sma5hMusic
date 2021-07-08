using Sma5h.Interfaces;
using Sma5h.Mods.Music.Models;
using System.Collections.Generic;

namespace Sma5h.Mods.Music.Interfaces
{
    public interface ISma5hMusicOverride : ISma5hMod
    {
        bool UpdateSoundTestOrderConfig(Dictionary<string, short> entries);
        bool UpdateCoreBgmEntries(MusicModEntries musicModEntries);
        bool UpdateGameTitleEntry(GameTitleEntry gameTitleEntry);
        bool DeleteGameTitleEntry(string gameTitleId);
        bool UpdatePlaylistConfig(Dictionary<string, PlaylistEntry> playlistEntries);
        bool UpdateMusicStageOverride(List<StageEntry> stageEntries);
        bool ResetOverrideFile(string file);
        void UpdateOldUnknownValues();
    }
}
