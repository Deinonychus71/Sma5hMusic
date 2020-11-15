namespace Sm5sh.Mods.Music.Helpers
{
    internal class Constants
    {
        internal class MusicModFiles
        {
            public const string MUSIC_MOD_METADATA_JSON_FILE = "metadata_mod.json";
            public const string MUSIC_MOD_METADATA_CSV_FILE = "metadata_mod.csv";
        }

        public class Resources
        {
            public const string BGM_PROPERTY_EXE_FILE = "BgmProperty/bgm-property.exe";
            public const string BGM_PROPERTY_HASH_FILE = "BgmProperty/bgm_hashes.txt";
            public const string BGM_PROPERTY_TEMP_FILE = "temp.yml";

            public const string NUS3AUDIO_EXE_FILE = "Nus3Audio/nus3audio.exe";
            public const string NUS3BANK_TEMPLATE_FILE = "template.nus3bank";
            public const string NUS3BANK_IDS_FILE = "nusbank_ids.csv";
            public const string NUS3AUDIO_TEMP_FILE = "temp.{0}";
        }

        public class GameResources
        {
            public const string NUS3AUDIO_FILE = "bgm_{0}.nus3audio";
            public const string NUS3BANK_FILE = "bgm_{0}.nus3bank";

            public const string PRC_UI_BGM_DB_PATH = "ui/param/database/ui_bgm_db.prc";
            public const string PRC_UI_GAMETITLE_DB_PATH = "ui/param/database/ui_gametitle_db.prc";
            public const string PRC_BGM_PROPERTY_PATH = "sound/config/bgm_property.bin";
            public const string MSBT_BGM = "ui/message/msg_bgm+{0}.msbt";
            public const string MSBT_TITLE = "ui/message/msg_title+{0}.msbt";
        }

        internal class InternalIds
        {
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
    }
}
