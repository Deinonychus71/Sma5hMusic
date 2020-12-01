namespace Sm5sh.Mods.Music.Models
{
    public class AudioCuePoints
    {
        public uint LoopStartMs { get; set; }
        public uint LoopStartSample { get; set; }
        public uint LoopEndMs { get; set; }
        public uint LoopEndSample { get; set; }
        public uint TotalTimeMs { get; set; }
        public uint TotalSamples { get; set; }
        public uint Frequency { get { return TotalTimeMs == 0 ? 0 : (uint)((double)TotalSamples / (double)TotalTimeMs * 1000.0); } }
    }
}
