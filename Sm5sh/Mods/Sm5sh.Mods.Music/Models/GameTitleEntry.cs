using Sm5sh.Mods.Music.Helpers;
using System.Collections.Generic;

namespace Sm5sh.Mods.Music.Models
{
    public class GameTitleEntry
    {
        public string UiGameTitleId { get; }
        public string NameId { get; set; }
        public string UiSeriesId { get; set; }
        public bool Unk1 { get; set; }
        public int Release { get; set; }

        public Dictionary<string, string> MSBTTitle { get; set; }
        public string MSBTTitleKey { get { return !string.IsNullOrEmpty(NameId) ? string.Format(Constants.InternalIds.MSBT_GAME_TITLE, NameId) : null; } }

        public override string ToString()
        {
            return UiGameTitleId;
        }

        public GameTitleEntry(string uiGameTitleId)
        {
            UiGameTitleId = uiGameTitleId;
            MSBTTitle = new Dictionary<string, string>();
        }
    }
}
