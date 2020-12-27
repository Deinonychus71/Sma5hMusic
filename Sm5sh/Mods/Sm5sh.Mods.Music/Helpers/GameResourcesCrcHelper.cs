using Sm5sh.ResourceProviders.Constants;
using System.Collections.Generic;

namespace Sm5sh.Mods.Music.Helpers
{
    public class GameResourcesCrcHelper
    {
        public static List<GameCrcSet> VersionCrcSets = new List<GameCrcSet>()
        {
            new GameCrcSet()
            {
                Version = 10.0,
                CrcResources = new Dictionary<string, uint>()
                {
                    { BgmPropertyFileConstants.BGM_PROPERTY_PATH, 0x5DA473DC },
                    { PrcExtConstants.PRC_UI_BGM_DB_PATH, 0x0B40201B },
                    { PrcExtConstants.PRC_UI_GAMETITLE_DB_PATH, 0x64A6B3F2 },
                    { PrcExtConstants.PRC_UI_STAGE_DB_PATH, 0xBEC70A05 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "eu_de"), 0x5ACB4CD9 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "eu_en"), 0xBB7D6832 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "eu_es"), 0xBC2DFC42 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "eu_fr"), 0xCA1466A3 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "eu_it"), 0x6929D1A6 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "eu_nl"), 0xB7BE4024 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "eu_ru"), 0x17FE6AA8 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "jp_ja"), 0x4E871500 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "kr_ko"), 0xAAA93540 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "us_en"), 0x62BBB2A9 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "us_es"), 0x86AA1535 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "us_fr"), 0xACFFA15E },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "zh_cn"), 0x109189D2 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "zh_tw"), 0x47BD1B25 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "eu_de"), 0x7671A994 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "eu_en"), 0x8819E6D4 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "eu_es"), 0x42F6A770 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "eu_fr"), 0xA3373FD7 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "eu_it"), 0xDA270DB7 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "eu_nl"), 0x1D7F72C3 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "eu_ru"), 0x2A8F96FC },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "jp_ja"), 0xA3AD4A34 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "kr_ko"), 0x64252510 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "us_en"), 0x773E88AC },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "us_es"), 0x73F11E85 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "us_fr"), 0x07C29D63 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "zh_cn"), 0x17BCC77D },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "zh_tw"), 0x6E8482F9 }
                }
            },
            new GameCrcSet()
            {
                Version = 10.1,
                CrcResources = new Dictionary<string, uint>()
                {
                    { BgmPropertyFileConstants.BGM_PROPERTY_PATH, 0x5DA473DC },
                    { PrcExtConstants.PRC_UI_BGM_DB_PATH, 0x83C7244A },
                    { PrcExtConstants.PRC_UI_GAMETITLE_DB_PATH, 0x64A6B3F2 },
                    { PrcExtConstants.PRC_UI_STAGE_DB_PATH, 0xBEC70A05 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "eu_de"), 0x5ACB4CD9 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "eu_en"), 0xBB7D6832 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "eu_es"), 0xBC2DFC42 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "eu_fr"), 0xCA1466A3 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "eu_it"), 0x6929D1A6 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "eu_nl"), 0xB7BE4024 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "eu_ru"), 0x17FE6AA8 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "jp_ja"), 0x4E871500 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "kr_ko"), 0xAAA93540 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "us_en"), 0x62BBB2A9 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "us_es"), 0x86AA1535 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "us_fr"), 0xACFFA15E },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "zh_cn"), 0x109189D2 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "zh_tw"), 0x47BD1B25 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "eu_de"), 0x88394D04 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "eu_en"), 0x481B25B8 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "eu_es"), 0x6431BB7F },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "eu_fr"), 0x090AD9CB },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "eu_it"), 0xFE5BC0B2 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "eu_nl"), 0x350A6DD3 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "eu_ru"), 0x51E6AC28 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "jp_ja"), 0x7185EFAE },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "kr_ko"), 0x85E1CC5C },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "us_en"), 0xFC6E27E0 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "us_es"), 0x02EECC25 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "us_fr"), 0xFDF911CC },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "zh_cn"), 0xC086A18E },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "zh_tw"), 0xA817D5A3 }
                }
            },
        };
    }

    public class GameCrcSet
    {
        public double Version { get; set; }
        public Dictionary<string, uint> CrcResources { get; set; }
    }
}
