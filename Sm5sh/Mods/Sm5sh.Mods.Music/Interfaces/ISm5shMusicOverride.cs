﻿using Sm5sh.Interfaces;
using Sm5sh.Mods.Music.Models;
using Sm5sh.Mods.Music.Models.PlaylistEntryModels;
using System.Collections.Generic;

namespace Sm5sh.Mods.Music.Interfaces
{
    public interface ISm5shMusicOverride : ISm5shMod
    {
        bool UpdateSoundTestOrderConfig(Dictionary<string, short> entries);

        bool UpdateCoreBgmEntry(BgmEntry bgmEntry);
        bool RemoveCoreBgmEntry(string toneId);
        bool UpdatePlaylistConfig(Dictionary<string, List<PlaylistValueEntry>> playlistEntries);
    }
}