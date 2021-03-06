using Sma5h.Mods.Music.Models;
using System.Threading.Tasks;

namespace Sma5h.Mods.Music.Interfaces
{
    public interface IAudioMetadataService
    {
        Task<AudioCuePoints> GetCuePoints(string inputFile);
        bool ConvertAudio(string inputMediaFile, string outputMediaFile);
    }
}
