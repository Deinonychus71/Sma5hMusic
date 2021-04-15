using Sma5h.Interfaces;
using Sma5h.ResourceProviders.Prc.Helpers;
using System.Collections.Generic;

namespace Sma5h.Data.Ui.Param.Database
{
    public class PrcUiStageDatabase : IStateManagerDb
    {
        [PrcDictionary("ui_stage_id")]
        [PrcHexMapping("db_root")]
        public Dictionary<string, PrcUiStageDatabaseModels.StageDbRootEntry> DbRootEntries { get; set; }
    }

    namespace PrcUiStageDatabaseModels
    {
        public class StageDbRootEntry
        {
            [PrcHexMapping("ui_stage_id", true)]
            public string UiStageId { get; set; }

            [PrcHexMapping("name_id")]
            public string NameId { get; set; }

            [PrcHexMapping("save_no")]
            public short SaveNo { get; set; }

            [PrcHexMapping("ui_series_id", true)]
            public string UiSeriesId { get; set; }

            [PrcHexMapping("can_select")]
            public bool CanSelect { get; set; }

            [PrcHexMapping("disp_order")]
            public sbyte DispOrder { get; set; }

            [PrcHexMapping("stage_place_id", true)]
            public string StagePlaceId { get; set; }

            [PrcHexMapping("secret_stage_place_id", true)]
            public string SecretStagePlaceId { get; set; }

            [PrcHexMapping("can_demo")]
            public bool CanDemo { get; set; }

            [PrcHexMapping(0x10359e17b0)]
            public bool Unk1 { get; set; }

            [PrcHexMapping("is_usable_flag")]
            public bool IsUsableFlag { get; set; }

            [PrcHexMapping("is_usable_amiibo")]
            public bool IsUsableAmiibo { get; set; }

            [PrcHexMapping("secret_command_id", true)]
            public string SecretCommandId { get; set; }

            [PrcHexMapping("secret_command_id_joycon", true)]
            public string SecretCommandIdJoycon { get; set; }

            [PrcHexMapping("bgm_set_id", true)]
            public string BgmSetId { get; set; }

            [PrcHexMapping("bgm_setting_no")]
            public byte BgmSettingNo { get; set; }

            [PrcHexMapping("bgm_selector")]
            public bool BgmSelector { get; set; }

            [PrcHexMapping("is_dlc")]
            public bool IsDlc { get; set; }

            [PrcHexMapping("is_patch")]
            public bool IsPatch { get; set; }

            [PrcHexMapping("dlc_chara_id", true)]
            public string DlcCharaId { get; set; }

            public override string ToString()
            {
                return UiStageId.ToString();
            }
        }
    }
}
