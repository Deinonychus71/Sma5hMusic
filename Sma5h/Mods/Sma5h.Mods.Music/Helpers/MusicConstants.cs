using System.Collections.Generic;

namespace Sma5h.Mods.Music.Helpers
{
    public class MusicConstants
    {
        public const double VersionSma5hMusic = 1.53;
        public const double VersionSma5hMusicOverride = 1.53;

        public class MusicModFiles
        {
            public const string MUSIC_MOD_METADATA_JSON_FILE = "metadata_mod.json";
            public const string MUSIC_OVERRIDE_ORDER_JSON_FILE = "order_override.json";
            public const string MUSIC_OVERRIDE_PLAYLIST_JSON_FILE = "playlist_override.json";
            public const string MUSIC_OVERRIDE_CORE_BGM_JSON_FILE = "core_bgm_override.json";
            public const string MUSIC_OVERRIDE_CORE_GAME_JSON_FILE = "core_game_override.json";
            public const string MUSIC_OVERRIDE_STAGE_JSON_FILE = "stage_override.json";
        }

        public class Resources
        {
            public const string NUS3AUDIO_EXE_FILE = "Nus3Audio/nus3audio.exe";
            public const string NUS3BANK_TEMPLATE_FILE = "template.nus3bank";
            public const string NUS3BANK_IDS_FILE = "nusbank_ids.csv";
            public const string NUS3AUDIO_TEMP_FILE = "temp.{0}";
            public const string AUDIO_FILE = "bgm_{0}{1}";
        }

        public class GameResources
        {
            public const string NUS3AUDIO_FILE = "bgm_{0}.nus3audio";
            public const string NUS3BANK_FILE = "bgm_{0}.nus3bank";

            public const int ToneIdMinimumSize = 1; //TOVERIFY
            public const int ToneIdMaximumSize = 47;

            public const int DbRootIdMinimumSize = ToneIdMinimumSize + 7;
            public const int DbRootIdMaximumSize = ToneIdMaximumSize + 7;

            public const int StreamSetIdMinimumSize = ToneIdMinimumSize + 4;
            public const int StreamSetIdMaximumSize = ToneIdMaximumSize + 4;

            public const int AssignedInfoIdMinimumSize = ToneIdMinimumSize + 5;
            public const int AssignedInfoIdMaximumSize = ToneIdMaximumSize + 5;

            public const int StreamIdMinimumSize = ToneIdMinimumSize + 7;
            public const int StreamIdMaximumSize = ToneIdMaximumSize + 7;

            public const int GameTitleMinimumSize = 14; //TO VERIFY
            public const int GameTitleMaximumSize = 62; //TO VERIFY
        }

        public class InternalIds
        {
            public const string NUS3AUDIO_FILE_PREFIX = "bgm_";

            public const string UI_BGM_ID_PREFIX = "ui_bgm_";
            public const string STREAM_SET_PREFIX = "set_";
            public const string INFO_ID_PREFIX = "info_";
            public const string STREAM_PREFIX = "stream_";

            public const string GAME_TITLE_ID_PREFIX = "ui_gametitle_";
            public const string GAME_SERIES_ID_PREFIX = "ui_series_";
            public const string RECORD_TYPE_PREFIX = "record_";

            public const string PLAYLIST_PREFIX = "bgm";

            public const string MSBT_GAME_TITLE_PREFIX = "tit_";
            public const string MSBT_GAME_TITLE = "tit_{0}";
            public const string MSBT_BGM_TITLE = "bgm_title_{0}";
            public const string MSBT_BGM_AUTHOR = "bgm_author_{0}";
            public const string MSBT_BGM_COPYRIGHT = "bgm_copyright_{0}";

            public const string GAME_SERIES_ID_DEFAULT = "ui_series_none";
            public const string GAME_TITLE_ID_DEFAULT = "ui_gametitle_none";
            public const string RECORD_TYPE_DEFAULT = "record_none";
            public const string RARITY_DEFAULT = "bgm_rarity_0";
            public const string SOUND_CONDITION = "sound_condition_none";
        }

        public static string[] VALID_RECORD_TYPES = new string[]
        {
            "record_arrange",
            "record_new_arrange",
            "record_original"
        };

        public static string[] VALID_MUSIC_EXTENSIONS = new string[]
        {
            ".idsp",
            ".lopus",
            ".brstm"
        };

        public static string[] EXTENSIONS_NEED_CONVERSION = new string[]
        {
            ".brstm"
        };

        public static Dictionary<string, string> SPECIAL_CATEGORY_LABELS = new Dictionary<string, string>()
        {
            {"0x16ff8d1375", "mario_3dland_scenelink" },
            {"0x15e9235a3d", "mario_paper_scenelink" },
            {"0x1b2d134791", "mario_pastusa_situationlink" },
            {"0x1ad6262d31", "mario_past64_situationlink" },
            {"0x16fcec16b2", "mario_odyssey_partlink" },
            {"0x1b2901643f", "mario_odyssey_kinopiotaicho" },
            {"0x1686642302", "yoshi_island_scenelink" },
            {"0x17b37fd636", "kirby_gameboy_scenelink" },
            {"0x16ea1970ce", "wario_madein_minigames" },
            {"0x0d009c712f", "totakeke_live" },
            {"0x105274ba4f", "sf_situationlink" },
            {"0x12111561a7", "0x12111561a7" }, //Moray Towers
            {"0x150281ae07", "mario_maker_scenelink" },
            {"0x11ff737d4d", "jack_mementoes_p3" },
            {"0x116117e8ee", "jack_mementoes_p4" },
            {"0x111610d878", "jack_mementoes_p5" },
            {"0x1609de57c3", "dolly_stadium_theme_01" },
            {"0x1690d70679", "dolly_stadium_theme_02" },
            {"0x16e7d036ef", "dolly_stadium_theme_03" },
            {"0x1679b4a34c", "dolly_stadium_theme_04" },
            {"0x160eb393da", "dolly_stadium_theme_05" },
            {"0x1697bac260", "dolly_stadium_theme_06" },
            {"0x16e0bdf2f6", "dolly_stadium_theme_07" },
            {"0x167002ef67", "dolly_stadium_theme_08" },
            {"0x160705dff1", "dolly_stadium_theme_09" },
            {"0x1667c25614", "dolly_stadium_theme_10" },
            {"0x1610c56682", "dolly_stadium_theme_11" },
            {"0x1689cc3738", "dolly_stadium_theme_12" },
            {"0x16fecb07ae", "dolly_stadium_theme_13" }
        };

        public static Dictionary<string, string> SOUND_CONDITION_LABELS = new Dictionary<string, string>()
        {
            {"0x147340113c", "sound_condition_none" },
            {"0x1d7feb1956", "sound_condition_xvillage_live" }
        };

        public static Dictionary<string, string> SOUND_CONDITION_PROCESS_LABELS = new Dictionary<string, string>()
        {
            {"0x1b9fe75d3f", "sound_condition_process_add" },
            {"0x21bee0c6ef", "sound_condition_process_exclusive" }
        };
    }
}
