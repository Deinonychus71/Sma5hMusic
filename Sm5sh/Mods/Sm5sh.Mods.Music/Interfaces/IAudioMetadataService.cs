using Sm5sh.Mods.Music.Models;
using System.Threading.Tasks;

namespace Sm5sh.Mods.Music.Interfaces
{
    public interface IAudioMetadataService
    {
        Task<AudioCuePoints> GetCuePoints(string inputFile);
        bool ConvertAudio(string inputMediaFile, string outputMediaFile);
    }
}
