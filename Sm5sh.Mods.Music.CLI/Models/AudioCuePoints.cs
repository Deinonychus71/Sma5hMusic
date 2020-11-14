namespace Sm5shMusic.Models
{
    public class AudioCuePoints
    {
        public ulong TotalSamples { get; set; }
        public ulong LoopStartSample { get; set; }
        public ulong LoopEndSample { get; set; }
        public uint Frequency { get; set; }
        
    }
}
