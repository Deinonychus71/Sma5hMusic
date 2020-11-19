using System.Threading.Tasks;

namespace VGMMusic
{
    public interface IVGMMusicPlayer
    {
        int TotalTime { get; }
        int CurrentTime { get; }
        bool Loaded { get; }
        bool Play();
        Task<bool> Play(string filename);
        Task<bool> Stop();
    }
}
