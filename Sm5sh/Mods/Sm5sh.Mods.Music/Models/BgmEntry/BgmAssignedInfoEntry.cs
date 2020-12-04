using Sm5sh.Mods.Music.Helpers;
using Sm5sh.Mods.Music.Interfaces;

namespace Sm5sh.Mods.Music.Models
{
    public class BgmAssignedInfoEntry : BgmBase
    {
        public string InfoId { get; }
        public string StreamId { get; set; }
        public string Condition { get; set; }
        public string ConditionProcess { get; set; }
        public int StartFrame { get; set; }
        public int ChangeFadeInFrame { get; set; }
        public int ChangeStartDelayFrame { get; set; }
        public int ChangeFadoutFrame { get; set; }
        public int ChangeStopDelayFrame { get; set; }
        public int MenuChangeFadeInFrame { get; set; }
        public int MenuChangeStartDelayFrame { get; set; }
        public int MenuChangeFadeOutFrame { get; set; }
        public int Unk1 { get; set; }


        public BgmAssignedInfoEntry(string infoId, IMusicMod musicMod = null)
            : base(musicMod)
        {
            InfoId = infoId;
            Condition = Constants.InternalIds.SOUND_CONDITION;
            ConditionProcess = "0x1b9fe75d3f";
            ChangeFadoutFrame = 55;
            MenuChangeFadeOutFrame = 55;
        }


        public override string ToString()
        {
            return InfoId;
        }
    }
}
