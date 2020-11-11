namespace Sm5shMusic.Models
{
    public class MusicModAudioCuePoints
    {
        public ulong LoopStartMs { get; set; }
        public ulong LoopStartSample { get; set; }
        public ulong LoopEndMs { get; set; }
        public ulong LoopEndSample { get; set; }
        public ulong TotalTimeMs { get; set; }
        public ulong TotalSample { get; set; }

        public static MusicModAudioCuePoints FromAudioCuePoints(AudioCuePoints audioCuePoints)
        {
            return new MusicModAudioCuePoints()
            {
                TotalSample = audioCuePoints.TotalSamples,
                LoopStartSample = audioCuePoints.LoopStartSample,
                LoopEndSample = audioCuePoints.LoopEndSample,
                TotalTimeMs = audioCuePoints.Frequency > 0 ? audioCuePoints.TotalSamples * 1000 / audioCuePoints.Frequency : 0,
                LoopStartMs = audioCuePoints.Frequency > 0 ? audioCuePoints.LoopStartSample * 1000 / audioCuePoints.Frequency : 0,
                LoopEndMs = audioCuePoints.Frequency > 0 ? audioCuePoints.LoopEndSample * 1000 / audioCuePoints.Frequency : 0
            };
        }
    }
}
