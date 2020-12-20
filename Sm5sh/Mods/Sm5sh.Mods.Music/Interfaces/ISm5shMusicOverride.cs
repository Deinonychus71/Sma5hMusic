using Sm5sh.Interfaces;
using Sm5sh.Mods.Music.Models;
using System.Collections.Generic;

namespace Sm5sh.Mods.Music.Interfaces
{
    public interface ISm5shMusicOverride : ISm5shMod
    {
        bool UpdateSoundTestOrderConfig(Dictionary<string, short> entries);
        bool UpdateCoreBgmEntries(MusicModEntries musicModEntries);
        bool UpdateGameTitleEntry(GameTitleEntry gameTitleEntry);
        bool DeleteGameTitleEntry(string gameTitleId);
        bool UpdatePlaylistConfig(Dictionary<string, PlaylistEntry> playlistEntries);
        bool UpdateMusicStageOverride(List<StageEntry> stageEntries);
    }
}
