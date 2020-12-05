using Sm5sh.Mods.Music.Helpers;
using Sm5sh.Mods.Music.Interfaces;
using System.Collections.Generic;

namespace Sm5sh.Mods.Music.Models
{
    public class GameTitleEntry : BgmBase
    {
        public string UiGameTitleId { get; }
        public string NameId { get; set; }
        public string UiSeriesId { get; set; }
        public bool Unk1 { get; set; }
        public int Release { get; set; }

        public Dictionary<string, string> MSBTTitle { get; set; }
        public string MSBTTitleKey { get { return !string.IsNullOrEmpty(NameId) ? string.Format(MusicConstants.InternalIds.MSBT_GAME_TITLE, NameId) : null; } }

        public override string ToString()
        {
            return UiGameTitleId;
        }

        public GameTitleEntry(string uiGameTitleId, IMusicMod musicMod = null)
            :base(musicMod)
        {
            UiGameTitleId = uiGameTitleId;
            MSBTTitle = new Dictionary<string, string>();
        }
    }
}
