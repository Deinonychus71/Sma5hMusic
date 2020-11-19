using System;
using System.Collections.Generic;
using System.Text;

namespace Sm5sh.GUI.Helpers
{
    public class Constants
    {
        public const string DEFAULT_LOCALE = "us_en";

        public readonly static Dictionary<string,string> CONVERTER_SERIES = new Dictionary<string, string>()
        {
            { "ui_series_none", "" },
            { "ui_series_mario", "Mario" },
            { "ui_series_mariokart", "Mario Kart" },
            { "ui_series_wreckingcrew", "Wrecking Crew" },
            { "ui_series_etc", "etc" }
            //TODO
        };

        public readonly static Dictionary<string, string> CONVERTER_RECORD_TYPE = new Dictionary<string, string>()
        {
            { "record_none", "" },
            { "record_arrange", "Remix" },
            { "record_original", "Original" },
            { "record_new_arrange", "New Remix" }
        };
    }
}
