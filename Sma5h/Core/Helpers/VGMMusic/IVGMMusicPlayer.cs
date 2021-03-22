using System.Threading.Tasks;
using static VGMMusic.VGMMusicPlayer;

namespace VGMMusic
{
    public interface IVGMMusicPlayer
    {
        int TotalTime { get; }
        int CurrentTime { get; }
        bool Loaded { get; }
        bool Play();
        bool IsPlaying { get; }
        float InGameVolume { get; set; }
        float GlobalVolume { get; set; }
        Task<bool> Play(string filename);
        Task<VGMAudioCuePoints> GetAudioCuePoints(string filename);
        Task<bool> Stop();

        event PlaybackEventHandler PlaybackStarted;
        event PlaybackEventHandler PlaybackStopped;
        event PlaybackEventHandler PlaybackPaused;
        event PlaybackPositionEventHandler PlaybackPositionChanged;
    }
}
