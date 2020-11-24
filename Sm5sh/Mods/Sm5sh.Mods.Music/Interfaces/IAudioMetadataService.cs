using Sm5sh.Mods.Music.Models;

namespace Sm5sh.Mods.Music.Interfaces
{
    public interface IAudioMetadataService
    {
        AudioCuePoints GetCuePoints(string inputFile);
        bool ConvertAudio(string inputMediaFile, string outputMediaFile);
    }
}
