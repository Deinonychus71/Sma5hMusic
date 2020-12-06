using Sm5sh.Mods.Music.Interfaces;

namespace Sm5sh.Mods.Music.Models
{
    public class BgmStreamSetEntry : BgmBase
    {
        public string StreamSetId { get; }
        public string SpecialCategory { get; set; }
        public string Info0 { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string Info4 { get; set; }
        public string Info5 { get; set; }
        public string Info6 { get; set; }
        public string Info7 { get; set; }
        public string Info8 { get; set; }
        public string Info9 { get; set; }
        public string Info10 { get; set; }
        public string Info11 { get; set; }
        public string Info12 { get; set; }
        public string Info13 { get; set; }
        public string Info14 { get; set; }
        public string Info15 { get; set; }


        public BgmStreamSetEntry(string streamSetId, IMusicMod musicMod = null)
            : base(musicMod)
        {
            StreamSetId = streamSetId;
        }


        public override string ToString()
        {
            return StreamSetId;
        }
    }
}
