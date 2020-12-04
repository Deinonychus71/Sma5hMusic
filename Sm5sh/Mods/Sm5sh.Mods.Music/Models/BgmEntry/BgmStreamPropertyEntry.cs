using Sm5sh.Mods.Music.Interfaces;

namespace Sm5sh.Mods.Music.Models
{
    public class BgmStreamPropertyEntry : BgmBase
    {
        public string StreamId { get; }
        public string DataName0 { get; set; }
        public string DataName1 { get; set; }
        public string DataName2 { get; set; }
        public string DataName3 { get; set; }
        public string DataName4 { get; set; }
        public byte Loop { get; set; }
        public string EndPoint { get; set; }
        public ushort FadeOutFrame { get; set; }
        public string StartPointSuddenDeath { get; set; }
        public string StartPointTransition { get; set; }
        public string StartPoint0 { get; set; }
        public string StartPoint1 { get; set; }
        public string StartPoint2 { get; set; }
        public string StartPoint3 { get; set; }
        public string StartPoint4 { get; set; }


        public BgmStreamPropertyEntry(string streamId, IMusicMod musicMod = null)
            : base(musicMod)
        {
            StreamId = streamId;
        }


        public override string ToString()
        {
            return StreamId;
        }
    }
}
