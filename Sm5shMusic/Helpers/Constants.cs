using System.IO;

namespace Sm5shMusic.Helpers
{
    public class Constants
    {
        public class MusicModFiles
        {
            public const string MusicModMetadataFile = "metadata_mod.json";
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
            

            public const string NusBankTemplatePath = "nus3bank";
            public const string NusBankTemplateFile = "template.nus3bank";

            public const string MsbtPath = "msbt";
            public const string MsbtBgmFile = "msg_bgm+{0}.msbt";
            public const string MsbtTitleFile = "msg_title+{0}.msbt";
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
            public const string WorkspaceBgmPropertyFile = "bgm_property.bin";
            public const string WorkspaceBgmPropertyTempFile = "bgm_property.yml";
            public const string WorkspaceMsgBgm = "msg_bgm+{0}.msbt";
            public const string WorkspaceMsgTitle = "msg_title+{0}.msbt";
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
            public const string BgmIdPrefix = "ui_bgm_";
            public const string SetIdPrefix = "set_";
            public const string InfoPrefix = "info_";
            public const string StreamPrefix = "stream_";

            public const string MsbtTitPrefix = "tit_";
            public const string MsbtBgmAuthorPrefix = "bgm_author_";
            public const string MsbtBgmCopyrightPrefix = "bgm_copyright_";
            public const string MsbtBgmTitlePrefix = "bgm_title_";

            public const string GameSeriesIdDefault = "ui_series_none";
        }

        public const string DefaultLocale = "en_US";
    }
}
