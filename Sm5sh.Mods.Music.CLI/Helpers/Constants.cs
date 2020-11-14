using System.IO;

namespace Sm5shMusic.Helpers
{
    public class Constants
    {
        public class MusicModFiles
        {
            public const string MusicModMetadataJsonFile = "metadata_mod.json";
            public const string MusicModMetadataCsvFile = "metadata_mod.csv";
            public const string StageModMetadataJsonFile = "metadata_stage_playlists.json";
        }

        public class ResourcesFiles
        {
            public const string BgmPropertyYmlFile = "bgm_property.yml";
            public const string NusBankIdsCsvFile = "nusbank_ids.csv";
            public const string UiBgmDbLabelsCsvFile = "param_labels.csv";

            public const string BgmPropertyFile = "bgm_property.bin";

            public const string ParamsPath = "params";
            public const string UiBgmDbFile = "ui_bgm_db.prc";
            public const string UiGameTitleDbFile = "ui_gametitle_db.prc";
            public const string UiStageDbFile = "ui_stage_db.prc";


            public const string NusBankTemplatePath = "nus3bank";
            public const string NusBankTemplateFile = "template.nus3bank";

            public const string MsbtPath = "msbt";
            public const string MsbtBgmFile = "msg_bgm+{0}.msbt";
            public const string MsbtTitleFile = "msg_title+{0}.msbt";

            public const string TemporaryAudioFile = "temp.{0}";
            public const string TemporaryBgmPropertyFile = "bgm_property.yml";
        }

        public class WorkspacePaths
        {
            public const string WorkspaceBgmStream = "stream;";
            public const string WorkspaceBgmStreamSound = "sound";
            public const string WorkspaceBgmStreamSoundBgm = "bgm";
            public const string WorkspaceUi = "ui";
            public const string WorkspaceUiParam = "param";
            public const string WorkspaceUiMessage = "message";
            public const string WorkspaceUiParamDatabase = "database";
            public const string WorkspaceSound = "sound";
            public const string WorkspaceSoundConfig = "config";
            public const string WorkspaceNus3AudioFile = "bgm_{0}.nus3audio";
            public const string WorkspaceNus3BankFile = "bgm_{0}.nus3bank";
            public const string WorkspaceUiGameTitleDb = "ui_gametitle_db.prc";
            public const string WorkspaceUiBgmDb = "ui_bgm_db.prc";
            public const string WorkspaceUiStageDb = "ui_stage_db.prc";
            public const string WorkspaceBgmPropertyFile = "bgm_property.bin";
            public const string WorkspaceMsgBgm = "msg_bgm+{0}.msbt";
            public const string WorkspaceMsgTitle = "msg_title+{0}.msbt";

            public const string AudioCachePath = "nus3audio";
        }

        public class ExternalTools
        {
            public const string Nus3AudioPath = "Nus3Audio";
            public const string BgmPropertyPath = "BgmProperty";
            public const string Nus3AudioExe = "nus3audio.exe";
            public const string BgmPropertyExe = "bgm-property.exe";
            public const string BgmPropertyHashes = "bgm_hashes.txt";
        }

        public class InternalIds
        {
            public const string GameTitleIdPrefix = "ui_gametitle_";
            public const string GameSeriesIdPrefix = "ui_series_";
            public const string StageIdPrefix = "ui_stage_";
            public const string BgmIdPrefix = "ui_bgm_";
            public const string SetIdPrefix = "set_";
            public const string InfoPrefix = "info_";
            public const string StreamPrefix = "stream_";

            public const string MsbtTitPrefix = "tit_";
            public const string MsbtBgmAuthorPrefix = "bgm_author_";
            public const string MsbtBgmCopyrightPrefix = "bgm_copyright_";
            public const string MsbtBgmTitlePrefix = "bgm_title_";

            public const string GameSeriesIdDefault = "ui_series_none";

            public const string RecordTypePrefix = "record_";
            public const string PlaylistPrefix = "bgm";
            public const string RarityDefault = "bgm_rarity_0";
        }

        public static string[] ValidSeries = new string[]
        {
            "none",
            "mario",
            "mariokart",
            "wreckingcrew",
            "etc",
            "donkeykong",
            "zelda",
            "metroid",
            "yoshi",
            "kirby",
            "starfox",
            "pokemon",
            "fzero",
            "mother",
            "fireemblem",
            "gamewatch",
            "palutena",
            "wario",
            "pikmin",
            "famicomrobot",
            "doubutsu",
            "wiifit",
            "punchout",
            "xenoblade",
            "metalgear",
            "sonic",
            "rockman",
            "pacman",
            "streetfighter",
            "finalfantasy",
            "bayonetta",
            "splatoon",
            "castlevania",
            "smashbros",
            "arms",
            "persona",
            "dragonquest",
            "banjokazooie",
            "fatalfury",
            "minecraft"
        };

        public static string[] DLCStages = new string[]
        {
            "brave",
            "buddy",
            "dolly",
            "master",
            "tantan",
            "pickel"
        };

        public static string[] ValidStages = new string[]
        {
            "random",
            "random_normal",
            "random_battle_field",
            "random_end",
            "battle_field",
            "battle_field_l",
            "end",
            "mario_castle64",
            "dk_jungle",
            "zelda_hyrule",
            "yoshi_story",
            "kirby_pupupu64",
            "poke_yamabuki",
            "mario_past64",
            "mario_castledx",
            "mario_rainbow",
            "dk_waterfall",
            "dk_lodge",
            "zelda_greatbay",
            "zelda_temple",
            "metroid_zebesdx",
            "yoshi_yoster",
            "yoshi_cartboard",
            "kirby_fountain",
            "kirby_greens",
            "fox_corneria",
            "fox_venom",
            "poke_stadium",
            "mother_onett",
            "mario_pastusa",
            "metroid_kraid",
            "fzero_bigblue",
            "mother_fourside",
            "mario_dolpic",
            "mario_pastx",
            "kart_circuitx",
            "wario_madein",
            "zelda_oldin",
            "metroid_norfair",
            "metroid_orpheon",
            "yoshi_island",
            "kirby_halberd",
            "fox_lylatcruise",
            "poke_stadium2",
            "fzero_porttown",
            "fe_siege",
            "pikmin_planet",
            "animal_village",
            "mother_newpork",
            "ice_top",
            "icarus_skyworld",
            "mg_shadowmoses",
            "luigimansion",
            "zelda_pirates",
            "poke_tengam",
            "75m",
            "mariobros",
            "plankton",
            "sonic_greenhill",
            "mario_3dland",
            "mario_newbros2",
            "mario_paper",
            "zelda_gerudo",
            "zelda_train",
            "kirby_gameboy",
            "poke_unova",
            "poke_tower",
            "fzero_mutecity3ds",
            "mother_magicant",
            "fe_arena",
            "icarus_uprising",
            "animal_island",
            "balloonfight",
            "nintendogs",
            "streetpass",
            "tomodachi",
            "pictochat2",
            "mario_uworld",
            "mario_galaxy",
            "kart_circuitfor",
            "zelda_skyward",
            "kirby_cave",
            "poke_kalos",
            "fe_colloseum",
            "flatzonex",
            "icarus_angeland",
            "wario_gamer",
            "pikmin_garden",
            "animal_city",
            "wiifit",
            "punchoutsb",
            "punchoutw",
            "xeno_gaur",
            "duckhunt",
            "wreckingcrew",
            "pilotwings",
            "wufuisland",
            "sonic_windyhill",
            "rock_wily",
            "pac_land",
            "mario_maker",
            "sf_suzaku",
            "ff_midgar",
            "bayo_clock",
            "mario_odyssey",
            "zelda_tower",
            "spla_parking",
            "dracula_castle",
            "bonus_game",
            "training",
            "general_all",
            "setting_stage",
            "sham_fight",
            "campaign_map",
            "menu_music",
            "boss_ganon",
            "boss_rathalos",
            "boss_marx",
            "boss_dracula",
            "boss_galleom",
            "boss_final",
            "boss_final2",
            "boss_final3",
            "edit",
            "homerun",
            "jack_mementoes",
            "brave_altar",
            "buddy_spiral",
            "dolly_stadium",
            "fe_shrine",
            "tantan_spring",
            "pickel_world",
            "battle_field_s"
        };

        public static string[] ValidRecordTypes = new string[]
        {
            "arrange",
            "new_arrange",
            "original"
        };

        public static string[] ValidExtensions = new string[]
        {
            ".idsp",
            ".lopus",
            ".brstm"
        };

        public static string[] ExtensionsNeedConversion = new string[]
        {
            ".brstm"
        };

        public const string DefaultLocale = "en_US";
    }
}
