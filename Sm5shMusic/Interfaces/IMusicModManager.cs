using Sm5shMusic.Models;
using System.Collections.Generic;

namespace Sm5shMusic.Interfaces
{
    public interface IMusicModManager
    {
        Dictionary<string, MusicModBgmEntry> BgmEntries { get; }

        bool Init();

        string GetMusicModMetadataFile();

        string GetMusicModAudioFile(string songFileName);
    }
}
