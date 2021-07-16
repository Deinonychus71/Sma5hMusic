using System.Collections.Generic;

namespace Sma5hMusic.GUI.Helpers
{
    public static class Constants
    {
        public const double GUIVersion = 1.53;
        public const bool IsStable = false;

        public const float DefaultVolume = 0.8f;
        public const float MinimumGameVolume = -20.0f;
        public const float MaximumGameVolume = 20.0f;

        public class DragAndDropDataFormats
        {
            public const string DATAOBJECT_FORMAT_BGM = "BGM";
            public const string DATAOBJECT_FORMAT_TREEVIEW = "TREEVIEW";
            public const string DATAOBJECT_FORMAT_PLAYLIST = "PLAYLIST";
        }
        public class SpecialCategories
        {
            public const string SPECIAL_CATEGORY_PERSONA_STAGE_3_VALUE = "jack_mementoes_p3";
            public const string SPECIAL_CATEGORY_PERSONA_STAGE_4_VALUE = "jack_mementoes_p4";
            public const string SPECIAL_CATEGORY_PERSONA_STAGE_5_VALUE = "jack_mementoes_p5";
            public const string SPECIAL_CATEGORY_PINCH_VALUE = "sf_situationlink"; //"0x105274ba4f"

            public readonly static Dictionary<string, string> UI_SPECIAL_CATEGORY = new Dictionary<string, string>()
            {
                { SPECIAL_CATEGORY_PINCH_VALUE, "Pinch Songs" },
                { SPECIAL_CATEGORY_PERSONA_STAGE_3_VALUE, "Persona 3 Stage" },
                { SPECIAL_CATEGORY_PERSONA_STAGE_4_VALUE, "Persona 4 Stage" },
                { SPECIAL_CATEGORY_PERSONA_STAGE_5_VALUE, "Persona 5 Stage" }
            };
        }

        public readonly static Dictionary<string, string> CONVERTER_SERIES = new Dictionary<string, string>()
        {
            { "ui_series_none", "None" },
            { "ui_series_mario", "Mario" },
            { "ui_series_mariokart", "Mario Kart" },
            { "ui_series_wreckingcrew", "Wrecking Crew" },
            { "ui_series_etc", "etc" },
            { "ui_series_donkeykong", "Donkey Kong" },
            { "ui_series_zelda", "The Legend of Zelda" },
            { "ui_series_metroid", "Metroid" },
            { "ui_series_yoshi", "Yoshi" },
            { "ui_series_kirby", "Kirby" },
            { "ui_series_starfox", "Starfox" },
            { "ui_series_pokemon", "Pokémon" },
            { "ui_series_fzero", "F-Zero" },
            { "ui_series_mother", "Mother" },
            { "ui_series_fireemblem", "Fire Emblem" },
            { "ui_series_gamewatch", "Game & Watch" },
            { "ui_series_palutena", "Kid Icarus" },
            { "ui_series_wario", "Wario" },
            { "ui_series_pikmin", "Pikmin" },
            { "ui_series_famicomrobot", "Famicon Robot" },
            { "ui_series_doubutsu", "Animal Crossing" },
            { "ui_series_wiifit", "Wii Fit" },
            { "ui_series_punchout", "Punch-Out!!" },
            { "ui_series_xenoblade", "Xenoblade" },
            { "ui_series_metalgear", "Metal Gear" },
            { "ui_series_sonic", "Sonic" },
            { "ui_series_rockman", "Megaman" },
            { "ui_series_pacman", "Pacman" },
            { "ui_series_streetfighter", "Street Fighter" },
            { "ui_series_finalfantasy", "Final Fantasy" },
            { "ui_series_bayonetta", "Bayonetta" },
            { "ui_series_splatoon", "Splatoon" },
            { "ui_series_castlevania", "Castlevania" },
            { "ui_series_smashbros", "Smash Bros" },
            { "ui_series_arms", "Arms" },
            { "ui_series_persona", "Persona" },
            { "ui_series_dragonquest", "Dragon Quest" },
            { "ui_series_banjokazooie", "Banjo-Kazooie" },
            { "ui_series_fatalfury", "Fatal Fury" },
            { "ui_series_minecraft", "Minecraft" },
            { "ui_series_tekken", "Tekken" }
        };

        public readonly static Dictionary<string, string> CONVERTER_RECORD_TYPE = new Dictionary<string, string>()
        {
            { "record_none", "None" },
            { "record_arrange", "Remix" },
            { "record_original", "Original" },
            { "record_new_arrange", "New Remix" }
        };

        public readonly static Dictionary<string, string> CONVERTER_LOCALE = new Dictionary<string, string>()
        {
            { "eu_de", "German" },
            { "eu_en", "English (UK)" },
            { "eu_es", "Spanish (Spain)" },
            { "eu_fr", "French (France)" },
            { "eu_it", "Italian" },
            { "eu_nl", "Dutch" },
            { "eu_ru", "Russian" },
            { "jp_ja", "Japanese" },
            { "kr_ko", "Korean" },
            { "us_en", "English (US)" },
            { "us_es", "Spanish (US)" },
            { "us_fr", "French (Canada)" },
            { "zh_cn", "Chinese (Simplified)" },
            { "zh_tw", "Chinese (Traditional)" }
        };

        public readonly static Dictionary<string, string> CONVERTER_CORE_PLAYLISTS = new Dictionary<string, string>()
        {
            {"bgmsmashbtl", "Smash Battle" },
            {"bgmsmashmenu", "Smash Menu" },
            {"bgmsmashmode", "Smash Mode" },
            {"bgmstageedit", "Stage Edit" },
            {"bgmboss", "Boss" },
            {"bgmadventure", "Adventure" },
            {"bgmmario", "Mario" },
            {"bgmmkart", "Mario Kart" },
            {"bgmdk", "Donkey Kong" },
            {"bgmkirby", "Kirby" },
            {"bgmzelda", "The Legend of Zelda" },
            {"bgmmetroid", "Metroid" },
            {"bgmfzero", "F-Zero" },
            {"bgmyoshi", "Yoshi" },
            {"bgmfox", "Starfox" },
            {"bgmpokemon", "Pokémon" },
            {"bgmmother", "Mother" },
            {"bgmfe", "Fire Emblem" },
            {"bgmgamewatch", "Game & Watch" },
            {"bgmicaros", "Kid Icarus" },
            {"bgmwario", "Wario" },
            {"bgmpikmin", "Pikmin" },
            {"bgmanimal", "Animal Crossing" },
            {"bgmwiifit", "Wii-Fit" },
            {"bgmpunchout", "Punch-Out!!" },
            {"bgmxenoblade", "Xenoblade" },
            {"bgmspla", "Splatoon" },
            {"bgmmetalgear", "Metal Gear" },
            {"bgmsonic", "Sonic" },
            {"bgmrockman", "Megaman" },
            {"bgmpacman", "Pacman" },
            {"bgmsf", "Street Fighter" },
            {"bgmff", "Final Fantasy" },
            {"bgmbeyo", "Bayonetta" },
            {"bgmdracula", "Castlevania" },
            {"bgmother", "Other" },
            {"bgmjack", "Persona" },
            {"bgmbrave", "Dragon Quest" },
            {"bgmbuddy", "Banjo-Kazooie" },
            {"bgmdolly", "Fatal Fury" },
            {"bgmmaster", "Fire Emblem Three Houses" },
            {"bgmtantan", "Arms" },
            {"bgmpickel", "Minecraft" },
            {"bgmedge", "Final Fantasy (Sephiroth)" },
            {"bgmelement", "Xenoblade 2 (Pyra & Mythra)" },
            {"bgmdemon", "Tekken" },
            {"bgmplaylist", "Playlist" },
        };

        public readonly static Dictionary<string, string> CONVERTER_CORE_STAGES = new Dictionary<string, string>()
        {
            {"ui_stage_random","(H) Random"},
            {"ui_stage_random_normal","(H) Random Normal"},
            {"ui_stage_random_battle_field","(H) Random Battlefield"},
            {"ui_stage_random_end","(H) Random Ω Form"},
            {"ui_stage_battle_field","Battlefield"},
            {"ui_stage_battle_field_l","Big Battlefield"},
            {"ui_stage_end","Final Destination"},
            {"ui_stage_mario_castle64","Peach's Castle"},
            {"ui_stage_dk_jungle","Kongo Jungle"},
            {"ui_stage_zelda_hyrule","Hyrule Castle"},
            {"ui_stage_yoshi_story","Super Happy Tree"},
            {"ui_stage_kirby_pupupu64","Dream Land"},
            {"ui_stage_poke_yamabuki","Saffron City"},
            {"ui_stage_mario_past64","Mushroom Kingdom"},
            {"ui_stage_mario_castledx","Princess Peach's Castle"},
            {"ui_stage_mario_rainbow","Rainbow Cruise"},
            {"ui_stage_dk_waterfall","Kongo Falls"},
            {"ui_stage_dk_lodge","Jungle Japes"},
            {"ui_stage_zelda_greatbay","Great Bay"},
            {"ui_stage_zelda_temple","Temple"},
            {"ui_stage_metroid_zebesdx","Brinstar"},
            {"ui_stage_yoshi_yoster","Yoshi's Island (Melee)"},
            {"ui_stage_yoshi_cartboard","Yoshi's Story"},
            {"ui_stage_kirby_fountain","Fountain of Dreams"},
            {"ui_stage_kirby_greens","Greens Greens"},
            {"ui_stage_fox_corneria","Corneria"},
            {"ui_stage_fox_venom","Venom"},
            {"ui_stage_poke_stadium","Pokémon Stadium"},
            {"ui_stage_mother_onett","Onett"},
            {"ui_stage_mario_pastusa","Mushroom Kingdom II"},
            {"ui_stage_metroid_kraid","Brinstar Depths"},
            {"ui_stage_fzero_bigblue","Big Blue"},
            {"ui_stage_mother_fourside","Fourside"},
            {"ui_stage_mario_dolpic","Delfino Plaza"},
            {"ui_stage_mario_pastx","Mushroomy Kingdom"},
            {"ui_stage_kart_circuitx","Figure-8 Circuit"},
            {"ui_stage_wario_madein","WarioWare, Inc"},
            {"ui_stage_zelda_oldin","Bridge of Eldin"},
            {"ui_stage_metroid_norfair","Norfair"},
            {"ui_stage_metroid_orpheon","Frigate Orpheon"},
            {"ui_stage_yoshi_island","Yoshi's Island"},
            {"ui_stage_kirby_halberd","Halberd"},
            {"ui_stage_fox_lylatcruise","Lylat Cruise"},
            {"ui_stage_poke_stadium2","Pokémon Stadium 2"},
            {"ui_stage_fzero_porttown","Port Town Aero Dive"},
            {"ui_stage_fe_siege","Castle Siege"},
            {"ui_stage_pikmin_planet","Distant Planet"},
            {"ui_stage_animal_village","Smashville"},
            {"ui_stage_mother_newpork","New Pork City"},
            {"ui_stage_ice_top","Summit"},
            {"ui_stage_icarus_skyworld","Skyworld"},
            {"ui_stage_mg_shadowmoses","Shadow Moses Island"},
            {"ui_stage_luigimansion","Luigi's Mansion"},
            {"ui_stage_zelda_pirates","Pirate Ship"},
            {"ui_stage_poke_tengam","Spear Pillar"},
            {"ui_stage_75m","75 m"},
            {"ui_stage_mariobros","Mario Bros."},
            {"ui_stage_plankton","Hanenbow"},
            {"ui_stage_sonic_greenhill","Green Hill Zone"},
            {"ui_stage_mario_3dland","3D Land"},
            {"ui_stage_mario_newbros2","Golden Plains"},
            {"ui_stage_mario_paper","Paper Mario"},
            {"ui_stage_zelda_gerudo","Gerudo Valley"},
            {"ui_stage_zelda_train","Spirit Train"},
            {"ui_stage_kirby_gameboy","Dream Land GB"},
            {"ui_stage_poke_unova","Unova Pokémon League"},
            {"ui_stage_poke_tower","Prism Tower"},
            {"ui_stage_fzero_mutecity3ds","Mute City SNES"},
            {"ui_stage_mother_magicant","Magicant"},
            {"ui_stage_fe_arena","Arena Ferox"},
            {"ui_stage_icarus_uprising","Reset Bomb Forest"},
            {"ui_stage_animal_island","Tortimer Island"},
            {"ui_stage_balloonfight","Balloon Fight"},
            {"ui_stage_nintendogs","Living Room"},
            {"ui_stage_streetpass","Find Mii"},
            {"ui_stage_tomodachi","Tomodachi Life"},
            {"ui_stage_pictochat2","PictoChat 2"},
            {"ui_stage_mario_uworld","Mushroom Kingdom U"},
            {"ui_stage_mario_galaxy","Mario Galaxy"},
            {"ui_stage_kart_circuitfor","Mario Circuit"},
            {"ui_stage_zelda_skyward","Skyloft"},
            {"ui_stage_kirby_cave","The Great Cave Offensive"},
            {"ui_stage_poke_kalos","Kalos Pokémon League"},
            {"ui_stage_fe_colloseum","Coliseum"},
            {"ui_stage_flatzonex","Flat Zone X"},
            {"ui_stage_icarus_angeland","Palutena's Temple"},
            {"ui_stage_wario_gamer","Gamer"},
            {"ui_stage_pikmin_garden","Garden of Hope"},
            {"ui_stage_animal_city","Town and City"},
            {"ui_stage_wiifit","Wii Fit Studio"},
            {"ui_stage_punchoutsb","Boxing Ring"},
            {"ui_stage_xeno_gaur","Gaur Plain"},
            {"ui_stage_duckhunt","Duck Hunt"},
            {"ui_stage_wreckingcrew","Wrecking Crew"},
            {"ui_stage_pilotwings","Pilotwings"},
            {"ui_stage_wufuisland","Wuhu Island"},
            {"ui_stage_sonic_windyhill","Windy Hill Zone"},
            {"ui_stage_rock_wily","Wily Castle"},
            {"ui_stage_pac_land","PAC-LAND"},
            {"ui_stage_mario_maker","Super Mario Maker"},
            {"ui_stage_sf_suzaku","Suzaku Castle"},
            {"ui_stage_ff_midgar","Midgar"},
            {"ui_stage_bayo_clock","Umbra Clock Tower"},
            {"ui_stage_mario_odyssey","New Donk City Hall"},
            {"ui_stage_zelda_tower","Great Plateau Tower"},
            {"ui_stage_spla_parking","Moray Towers"},
            {"ui_stage_dracula_castle","Dracula's Castle"},
            {"ui_stage_bonus_game","(H) Bonus Game"},
            {"ui_stage_training","(H) Stage Training"},
            {"ui_stage_general_all","(H) General All"},
            {"ui_stage_setting_stage","(H) Setting Stage"},
            {"ui_stage_sham_fight","(H) Sham Fight"},
            {"ui_stage_campaign_map","(H) Campaign Map"},
            {"ui_stage_menu_music","(H) Menu Music"},
            {"ui_stage_boss_ganon","(H) Boss Ganon"},
            {"ui_stage_boss_rathalos","(H) Boss Rathalos"},
            {"ui_stage_boss_marx","(H) Boss Marx"},
            {"ui_stage_boss_dracula","(H) Boss Dracula"},
            {"ui_stage_boss_galleom","(H) Boss Galleom"},
            {"ui_stage_boss_final","(H) Boss Final"},
            {"ui_stage_boss_final2","(H) Boss Final 2"},
            {"ui_stage_boss_final3","(H) Boss Final 3"},
            {"ui_stage_punchoutw","(H) Boxing Ring"},
            {"ui_stage_edit","(H) Stage Edit"},
            {"ui_stage_homerun","(H) Home Run"},
            {"ui_stage_jack_mementoes","Mementos"},
            {"ui_stage_brave_altar","Yggdrasil's Altar"},
            {"ui_stage_buddy_spiral","Spiral Mountain"},
            {"ui_stage_dolly_stadium","King of Fighter Stadium"},
            {"ui_stage_fe_shrine","Garreg Mach Monastery"},
            {"ui_stage_tantan_spring","Spring Stadium"},
            {"ui_stage_pickel_world","Minecraft World"},
            {"ui_stage_ff_cave","Northern Cave"},
            {"ui_stage_xeno_alst","Cloud Sea of Alrest"},
            {"ui_stage_demon_dojo","Mishima Dojo"},
            {"ui_stage_battle_field_s", "Small Battlefield"}
        };

        public static string GetSeriesDisplayName(string key)
        {
            if (string.IsNullOrEmpty(key))
                return key;
            return CONVERTER_SERIES.ContainsKey(key) ? CONVERTER_SERIES[key] : key;
        }

        public static string GetRecordTypeDisplayName(string key)
        {
            if (string.IsNullOrEmpty(key))
                return key;
            return CONVERTER_RECORD_TYPE.ContainsKey(key) ? CONVERTER_RECORD_TYPE[key] : key;
        }

        public static string GetStageDisplayName(string key)
        {
            if (string.IsNullOrEmpty(key))
                return key;
            return CONVERTER_CORE_STAGES.ContainsKey(key) ? CONVERTER_CORE_STAGES[key] : key;
        }

        public static string GetBgmPlaylistName(string key)
        {
            if (string.IsNullOrEmpty(key))
                return key;
            return CONVERTER_CORE_PLAYLISTS.ContainsKey(key) ? CONVERTER_CORE_PLAYLISTS[key] : key;
        }

        public static string GetLocaleDisplayName(string key)
        {
            if (string.IsNullOrEmpty(key))
                return key;
            return CONVERTER_LOCALE.ContainsKey(key) ? CONVERTER_LOCALE[key] : key;
        }
    }
}
