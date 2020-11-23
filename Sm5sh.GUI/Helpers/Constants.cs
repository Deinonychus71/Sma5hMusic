using System;
using System.Collections.Generic;
using System.Text;

namespace Sm5sh.GUI.Helpers
{
    public static class Constants
    {
        public const string DEFAULT_LOCALE = "us_en";

        public readonly static Dictionary<string, string> CONVERTER_SERIES = new Dictionary<string, string>()
        {
            { "ui_series_none", "" },
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
            { "ui_series_punchout", "Punch Out!" },
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
            { "ui_series_minecraft", "Minecraft" }
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
            { "eu_sp", "Spanish (Spain)" },
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

        public readonly static Dictionary<ulong, string> CONVERTER_SPECIAL_CATEGORY = new Dictionary<ulong, string>()
        {
            { 0x105274ba4f, "Pinch Songs" },
            { 0x11ff737d4d, "Persona 3 Stage" },
            { 0x116117e8ee, "Persona 4 Stage" },
            { 0x111610d878, "Persona 5 Stage" },
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

        public static string GetLocaleDisplayName(string key)
        {
            if (string.IsNullOrEmpty(key))
                return key;
            return CONVERTER_LOCALE.ContainsKey(key) ? CONVERTER_LOCALE[key] : key;
        }

        public static string GetSpecialCategoryDisplayName(ulong key)
        {
            return CONVERTER_SPECIAL_CATEGORY.ContainsKey(key) ? CONVERTER_SPECIAL_CATEGORY[key] : $"0x{key:x}";
        }
    }
}
