using Sm5sh.PrcParser;
using System.Collections.Generic;

namespace Sm5shMusic.PrcModels
{
    public class GameTitleDatabase : IPrcParsable
    {
        [PrcHexMapping("db_root")]
        public List<GameTitleDbRootEntry> DbRootEntries { get; set; }
    }

    public class GameTitleDbRootEntry
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
