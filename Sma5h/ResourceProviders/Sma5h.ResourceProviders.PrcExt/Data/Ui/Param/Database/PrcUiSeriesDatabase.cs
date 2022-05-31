using Sma5h.Interfaces;
using Sma5h.ResourceProviders.Prc.Helpers;
using System.Collections.Generic;

namespace Sma5h.Data.Ui.Param.Database
{
    public class PrcUiSeriesDatabase : IStateManagerDb
    {
        [PrcDictionary("ui_series_id")]
        [PrcHexMapping("db_root")]
        public Dictionary<string, PrcUiSeriesDatabaseModels.PrcSeriesDbRootEntry> DbRootEntries { get; set; }
    }

    namespace PrcUiSeriesDatabaseModels
    {
        public class PrcSeriesDbRootEntry
        {
            [PrcHexMapping("ui_series_id", true)]
            public string UiSeriesId { get; set; }

            [PrcHexMapping("name_id")]
            public string NameId { get; set; }

            [PrcHexMapping("disp_order")]
            public sbyte DispOrder { get; set; }

            [PrcHexMapping("disp_order_sound")]
            public sbyte DispOrderSound { get; set; }

            [PrcHexMapping("save_no")]
            public sbyte SaveNo { get; set; }

            [PrcHexMapping(0x1c38302364)]
            public bool Unk1 { get; set; }

            [PrcHexMapping("is_dlc")]
            public bool IsDlc { get; set; }

            [PrcHexMapping("is_patch")]
            public bool IsPatch { get; set; }

            [PrcHexMapping("dlc_chara_id", true)]
            public string DlcCharaId { get; set; }

            [PrcHexMapping("is_use_amiibo_bg")]
            public bool IsUseAmiiboBg { get; set; }

            public override string ToString()
            {
                return UiSeriesId.ToString();
            }
        }
    }
}
