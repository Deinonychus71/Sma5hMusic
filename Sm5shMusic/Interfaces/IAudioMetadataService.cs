using Sm5shMusic.Models;

namespace Sm5shMusic.Interfaces
{
    public interface IAudioMetadataService
    {
        AudioCuePoints GetCuePoints(string filePath);
    }
}
