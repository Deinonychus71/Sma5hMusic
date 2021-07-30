using Sma5h.ResourceProviders.Constants;
using System.Collections.Generic;

namespace Sma5h.Mods.Music.Helpers
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
            new GameCrcSet()
            {
                Version = 11.0,
                CrcResources = new Dictionary<string, uint>()
                {
                    { BgmPropertyFileConstants.BGM_PROPERTY_PATH, 0xBA5AA485 },
                    { PrcExtConstants.PRC_UI_BGM_DB_PATH, 0x8C9C8CFA },
                    { PrcExtConstants.PRC_UI_GAMETITLE_DB_PATH, 0xD8DF6BB7 },
                    { PrcExtConstants.PRC_UI_STAGE_DB_PATH, 0x166F5A24 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "eu_de"), 0x2B7F6EFC },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "eu_en"), 0x0523E68A },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "eu_es"), 0x0952278B },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "eu_fr"), 0x1633E38E },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "eu_it"), 0x35C08CC7 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "eu_nl"), 0x66B153D0 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "eu_ru"), 0x5A5FDC42 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "jp_ja"), 0x91A3C495 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "kr_ko"), 0xABA748D1 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "us_en"), 0xA928A9A2 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "us_es"), 0x34B84DCB },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "us_fr"), 0xB52AB6BA },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "zh_cn"), 0x8DB138DC },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "zh_tw"), 0x68E1C851 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "eu_de"), 0x9691CD0B },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "eu_en"), 0xE92A0719 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "eu_es"), 0x9A71A59C },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "eu_fr"), 0xB4A176A9 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "eu_it"), 0xF2E9436F },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "eu_nl"), 0xB3079C50 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "eu_ru"), 0xC3CF90AC },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "jp_ja"), 0x942D6AD1 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "kr_ko"), 0x3C424CE3 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "us_en"), 0xB2F1BD5E },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "us_es"), 0x42EC88D6 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "us_fr"), 0xA9C3A0FC },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "zh_cn"), 0xB4E54C6C },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "zh_tw"), 0xEFEABC47 }
                }
            },
            new GameCrcSet()
            {
                Version = 12.0,
                CrcResources = new Dictionary<string, uint>()
                {
                    { BgmPropertyFileConstants.BGM_PROPERTY_PATH, 0xC785C580 },
                    { PrcExtConstants.PRC_UI_BGM_DB_PATH, 0x3320B289 },
                    { PrcExtConstants.PRC_UI_GAMETITLE_DB_PATH, 0x533664E1 },
                    { PrcExtConstants.PRC_UI_STAGE_DB_PATH, 0x7084D2D8 },
                    { PrcExtConstants.PRC_UI_SERIES_DB_PATH, 0x1E6D7052 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "eu_de"), 0x84016BE6 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "eu_en"), 0xF1571085 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "eu_es"), 0xDED83EC6 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "eu_fr"), 0x766048A9 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "eu_it"), 0xA4BA3D7F },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "eu_nl"), 0x9A371627 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "eu_ru"), 0x75C1E6B5 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "jp_ja"), 0xFB7BA083 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "kr_ko"), 0x1C36486B },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "us_en"), 0x3E9F6E4B },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "us_es"), 0xC0C48C07 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "us_fr"), 0x4901DF63 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "zh_cn"), 0x1DDB42E7 },
                    { string.Format(MsbtExtConstants.MSBT_BGM, "zh_tw"), 0xDDFE32FE },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "eu_de"), 0x7C355694 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "eu_en"), 0x0084328C },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "eu_es"), 0x0D716895 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "eu_fr"), 0x84795FD1 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "eu_it"), 0xA7891AC9 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "eu_nl"), 0x744FBDEB },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "eu_ru"), 0x057D7B2E },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "jp_ja"), 0x62B96EBA },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "kr_ko"), 0x10DB3A17 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "us_en"), 0x1235AB87 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "us_es"), 0x118E70A3 },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "us_fr"), 0x791364CF },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "zh_cn"), 0xEC7F7A1D },
                    { string.Format(MsbtExtConstants.MSBT_TITLE, "zh_tw"), 0x87F06A90 }
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
