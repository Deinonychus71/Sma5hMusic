namespace Sm5sh.Mods.Music.Models
{
    public class AudioCuePoints
    {
        public ulong LoopStartMs { get; set; }
        public ulong LoopStartSample { get; set; }
        public ulong LoopEndMs { get; set; }
        public ulong LoopEndSample { get; set; }
        public ulong TotalTimeMs { get; set; }
        public ulong TotalSamples { get; set; }
        public ulong Frequency { get { return TotalTimeMs / 1000 * TotalSamples; } }
    }
}
