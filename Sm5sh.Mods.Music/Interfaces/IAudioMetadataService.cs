using Sm5sh.Mods.Music.BgmEntryModels;

namespace Sm5sh.Mods.Music
{
    public interface IAudioMetadataService
    {
        AudioCuePoints GetCuePoints(string inputFile);
        bool ConvertAudio(string inputMediaFile, string outputMediaFile);
    }
}
