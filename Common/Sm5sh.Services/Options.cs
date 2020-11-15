using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sm5sh.Services
{
    public class Options
    {
        //Game Path (Input)
        public string GameResourcesPath { get; set; }
        public string GameUiParamDatabasePath { get { return Path.Combine(GameResourcesPath, "ui", "param", "database"); } }
        public string GameSoundConfigPath { get { return Path.Combine(GameResourcesPath, "ui", "param", "database"); } }

        public string PrcUiBgmDbResourceFile { get { return Path.Combine(GameUiParamDatabasePath, "ui_bgm_db.prc"); } }
        public string PrcUiGameTitleDbResourceFile { get { return Path.Combine(GameUiParamDatabasePath, "ui_gametitle_db.prc"); } }
        public string PrcUiStageDbResourceFile { get { return Path.Combine(GameUiParamDatabasePath, "ui_stage_db.prc"); } }

        public string BinBgmPropertyResourceFile { get { return Path.Combine(GameSoundConfigPath, "bgm_property.bin"); } }


        //Output Path
        public string OutputPath { get; set; }
        public string OutputUiParamDatabasePath { get { return Path.Combine(OutputPath, "ui", "param", "database"); } }
        public string OutputSoundConfigPath { get { return Path.Combine(OutputPath, "ui", "param", "database"); } }

        public string OutputPrcUiBgmDbResourceFile { get { return Path.Combine(OutputUiParamDatabasePath, "ui_bgm_db.prc"); } }
        public string OutputPrcUiGameTitleDbResourceFile { get { return Path.Combine(OutputUiParamDatabasePath, "ui_gametitle_db.prc"); } }
        public string OutputPrcUiStageDbResourceFile { get { return Path.Combine(OutputUiParamDatabasePath, "ui_stage_db.prc"); } }

        public string OutputBinBgmPropertyResourceFile { get { return Path.Combine(OutputSoundConfigPath, "bgm_property.bin"); } }

        //Tools Path
        public string ToolsPath { get; set; }
        public string ToolsBgmPropertyExeFile { get { return Path.Combine(ToolsPath, "BgmProperty", "bgm-property.exe" ); } }
        public string ToolsBgmPropertyHashesFile { get { return Path.Combine(ToolsPath, "BgmProperty", "bgm_hashes.txt"); } }

        //Temp Path
        public string TempPath { get; set; }
    }
}
