namespace Sm5sh.Mods.Music.Helpers
{
    public class MusicConstants
    {
        public const double VersionSm5shMusic = 1.0;
        public const double VersionSm5shMusicOverride = 1.0;

        public class MusicModFiles
        {
            public const string MUSIC_MOD_METADATA_JSON_FILE = "metadata_mod.json";
            public const string MUSIC_MOD_METADATA_CSV_FILE = "metadata_mod.csv";
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
    }
}
