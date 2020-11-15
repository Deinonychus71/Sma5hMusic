using Sm5sh.Interfaces;
using Sm5sh.ResourceProviders.Prc.Helpers;
using System.Collections.Generic;

namespace Sm5sh.Data.Ui.Param.Database
{
    public class PrcUiGameTitleDatabase : IStateManagerDb
    {
        [PrcHexMapping("db_root")]
        public List<PrcUiGameTitleDatabaseModels.PrcGameTitleDbRootEntry> DbRootEntries { get; set; }
    }

    namespace PrcUiGameTitleDatabaseModels
    {
        public class PrcGameTitleDbRootEntry
        {
            [PrcHexMapping("ui_gametitle_id")]
            public PrcHash40 UiGameTitleId { get; set; }

            [PrcHexMapping("name_id")]
            public string NameId { get; set; }

            [PrcHexMapping("ui_series_id")]
            public PrcHash40 UiSeriesId { get; set; }

            [PrcHexMapping(0x1c38302364)]
            public bool Unk1 { get; set; }

            [PrcHexMapping("release")]
            public int Release { get; set; }

            public override string ToString()
            {
                return UiGameTitleId.ToString();
            }
        }
    }
}
