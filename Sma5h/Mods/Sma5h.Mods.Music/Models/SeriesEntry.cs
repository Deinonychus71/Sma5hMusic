using Sma5h.Mods.Music.Helpers;
using System.Collections.Generic;

namespace Sma5h.Mods.Music.Models
{
    public class SeriesEntry : BgmBase
    {
        public string UiSeriesId { get; set; }
        public string NameId { get; set; }
        public sbyte DispOrder { get; set; }
        public sbyte DispOrderSound { get; set; }
        public short SaveNo { get; set; }
        public bool Unk1 { get; set; }
        public bool IsDlc { get; set; }
        public bool IsPatch { get; set; }
        public string DlcCharaId { get; set; }
        public bool IsUseAmiiboBg { get; set; }

        public Dictionary<string, string> MSBTTitle { get; set; }
        public string MSBTTitleKey { get { return !string.IsNullOrEmpty(NameId) ? string.Format(MusicConstants.InternalIds.MSBT_SERIES_TITLE, NameId) : null; } }

        public override string ToString()
        {
            return UiSeriesId;
        }

        public SeriesEntry(string uiSeriesId, EntrySource source = EntrySource.Core)
            : base(source)
        {
            UiSeriesId = uiSeriesId;
            SaveNo = -1;
            MSBTTitle = new Dictionary<string, string>();
        }
    }
}
