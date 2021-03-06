using Sma5h.Mods.Music.Interfaces;

namespace Sma5h.Mods.Music.Models
{
    public class BgmPropertyEntry : BgmBase
    {
        public string NameId { get; }
        public float AudioVolume { get; set; }
        public string Filename { get; private set; }
        public uint LoopStartMs { get; set; }
        public uint LoopStartSample { get; set; }
        public uint LoopEndMs { get; set; }
        public uint LoopEndSample { get; set; }
        public uint TotalTimeMs { get; set; }
        public uint TotalSamples { get; set; }
        public uint Frequency { get { return TotalTimeMs == 0 ? 0 : (uint)((double)TotalSamples / (double)TotalTimeMs * 1000.0); } }

        public BgmPropertyEntry(string nameId, string filename, IMusicMod musicMod = null) :
            base(musicMod)
        {
            NameId = nameId;
            Filename = filename;
            AudioVolume = 2.7f;
        }

        public void ChangeFilename(string filename)
        {
            //TODO - Try to figure out a better way for mod to override filename
            Filename = filename;
        }
    }
}
