using Sm5sh.Helpers.PrcHelper;
using System.Collections.Generic;

namespace Sm5sh.Data.Ui.Param.Database
{
    public class PrcUiBgmDatabase : IPrcParsable
    {
        [PrcHexMapping("db_root")]
        public List<PrcUiBgmDatabaseModels.PrcBgmDbRootEntry> DbRootEntries { get; set; }

        [PrcHexMapping("stage_bgm")]
        public List<PrcUiBgmDatabaseModels.PrcBgmStageBgmEntry> StageBgmEntries { get; set; }

        [PrcHexMapping("stream_set")]
        public List<PrcUiBgmDatabaseModels.PrcBgmStreamSetEntry> StreamSetEntries { get; set; }

        [PrcHexMapping("assigned_info")]
        public List<PrcUiBgmDatabaseModels.PrcBgmAssignedInfoEntry> AssignedInfoEntries { get; set; }

        [PrcHexMapping("stream_property")]
        public List<PrcUiBgmDatabaseModels.PrcBgmStreamPropertyEntry> StreamPropertyEntries { get; set; }

        [PrcHexMapping("fighter_jingle")]
        public List<PrcUiBgmDatabaseModels.PrcBgmFighterJingleBgmEntry> FighterJingleEntries { get; set; }

        [PrcFilterMatch("^(bgm)")]
        public List<PcrFilterStruct<PrcUiBgmDatabaseModels.PrcBgmPlaylistEntry>> PlaylistEntries { get; set; }
    }

    namespace PrcUiBgmDatabaseModels
    {

        public class PrcBgmDbRootEntry
        {
            [PrcHexMapping("ui_bgm_id")]
            public PrcHash40 UiBgmId { get; set; }

            [PrcHexMapping("stream_set_id")]
            public PrcHash40 StreamSetId { get; set; }

            [PrcHexMapping("rarity")]
            public PrcHash40 Rarity { get; set; }

            [PrcHexMapping("record_type")]
            public PrcHash40 RecordType { get; set; }

            [PrcHexMapping("ui_gametitle_id")]
            public PrcHash40 UiGameTitleId { get; set; }

            [PrcHexMapping("ui_gametitle_id_1")]
            public PrcHash40 UiGameTitleId1 { get; set; }

            [PrcHexMapping("ui_gametitle_id_2")]
            public PrcHash40 UiGameTitleId2 { get; set; }

            [PrcHexMapping("ui_gametitle_id_3")]
            public PrcHash40 UiGameTitleId3 { get; set; }

            [PrcHexMapping("ui_gametitle_id_4")]
            public PrcHash40 UiGameTitleId4 { get; set; }

            [PrcHexMapping("name_id")]
            public string NameId { get; set; }

            [PrcHexMapping("save_no")]
            public short SaveNo { get; set; }

            [PrcHexMapping("test_disp_order")]
            public short TestDispOrder { get; set; }

            [PrcHexMapping("menu_value")]
            public int MenuValue { get; set; }

            [PrcHexMapping("jp_region")]
            public bool JpRegion { get; set; }

            [PrcHexMapping("other_region")]
            public bool OtherRegion { get; set; }

            [PrcHexMapping("possessed")]
            public bool Possessed { get; set; }

            [PrcHexMapping("prize_lottery")]
            public bool PrizeLottery { get; set; }

            [PrcHexMapping("shop_price")]
            public uint ShopPrice { get; set; }

            [PrcHexMapping("count_target")]
            public bool CountTarget { get; set; }

            [PrcHexMapping("menu_loop")]
            public byte MenuLoop { get; set; }

            [PrcHexMapping("is_selectable_stage_make")]
            public bool IsSelectableStageMake { get; set; }

            [PrcHexMapping(0x18db285704)]
            public bool Unk1 { get; set; }

            [PrcHexMapping(0x16fe9a28fe)]
            public bool Unk2 { get; set; }

            [PrcHexMapping("is_dlc")]
            public bool IsDlc { get; set; }

            [PrcHexMapping("is_patch")]
            public bool IsPatch { get; set; }

            [PrcHexMapping(0x0ff71e57ec)]
            public PrcHash40 Unk3 { get; set; }

            [PrcHexMapping(0x14341640b8)]
            public PrcHash40 Unk4 { get; set; }

            [PrcHexMapping(0x1560c0949b)]
            public PrcHash40 Unk5 { get; set; }

            public override string ToString()
            {
                return UiBgmId.ToString();
            }
        }

        public class PrcBgmStageBgmEntry
        {
            [PrcHexMapping("ui_bgm_id")]
            public PrcHash40 UiBgmId { get; set; }

            [PrcHexMapping("stream_set_id")]
            public PrcHash40 StreamSetId { get; set; }

            [PrcHexMapping("ui_stage_id")]
            public PrcHash40 UiStageId { get; set; }

            public override string ToString()
            {
                return UiBgmId.ToString();
            }
        }

        public class PrcBgmStreamSetEntry
        {
            [PrcHexMapping("stream_set_id")]
            public PrcHash40 StreamSetId { get; set; }

            [PrcHexMapping("special_category")]
            public PrcHash40 SpecialCategory { get; set; }

            [PrcHexMapping("info0")]
            public PrcHash40 Info0 { get; set; }

            [PrcHexMapping("info1")]
            public PrcHash40 Info1 { get; set; }

            [PrcHexMapping("info2")]
            public PrcHash40 Info2 { get; set; }

            [PrcHexMapping("info3")]
            public PrcHash40 Info3 { get; set; }

            [PrcHexMapping("info4")]
            public PrcHash40 Info4 { get; set; }

            [PrcHexMapping("info5")]
            public PrcHash40 Info5 { get; set; }

            [PrcHexMapping("info6")]
            public PrcHash40 Info6 { get; set; }

            [PrcHexMapping("info7")]
            public PrcHash40 Info7 { get; set; }

            [PrcHexMapping("info8")]
            public PrcHash40 Info8 { get; set; }

            [PrcHexMapping("info9")]
            public PrcHash40 Info9 { get; set; }

            [PrcHexMapping("info10")]
            public PrcHash40 Info10 { get; set; }

            [PrcHexMapping("info11")]
            public PrcHash40 Info11 { get; set; }

            [PrcHexMapping("info12")]
            public PrcHash40 Info12 { get; set; }

            [PrcHexMapping("info13")]
            public PrcHash40 Info13 { get; set; }

            [PrcHexMapping("info14")]
            public PrcHash40 Info14 { get; set; }

            [PrcHexMapping("info15")]
            public PrcHash40 Info15 { get; set; }

            public override string ToString()
            {
                return StreamSetId.ToString();
            }
        }

        public class PrcBgmAssignedInfoEntry
        {
            [PrcHexMapping("info_id")]
            public PrcHash40 InfoId { get; set; }

            [PrcHexMapping("stream_id")]
            public PrcHash40 StreamId { get; set; }

            [PrcHexMapping("condition")]
            public PrcHash40 Condition { get; set; }

            [PrcHexMapping("condition_process")]
            public PrcHash40 ConditionProcess { get; set; }

            [PrcHexMapping("start_frame")]
            public int StartFrame { get; set; }

            [PrcHexMapping("change_fadein_frame")]
            public int ChangeFadeInFrame { get; set; }

            [PrcHexMapping("change_start_delay_frame")]
            public int ChangeStartDelayFrame { get; set; }

            [PrcHexMapping("change_fadeout_frame")]
            public int ChangeFadoutFrame { get; set; }

            [PrcHexMapping("change_stop_delay_frame")]
            public int ChangeStopDelayFrame { get; set; }

            [PrcHexMapping("menu_change_fadein_frame")]
            public int MenuChangeFadeInFrame { get; set; }

            [PrcHexMapping("menu_change_start_delay_frame")]
            public int MenuChangeStartDelayFrame { get; set; }

            [PrcHexMapping("menu_change_fadeout_frame")]
            public int MenuChangeFadeOutFrame { get; set; }

            [PrcHexMapping(0x1c6a38c480)]
            public int Unk1 { get; set; }

            public override string ToString()
            {
                return InfoId.ToString();
            }
        }

        public class PrcBgmStreamPropertyEntry
        {
            [PrcHexMapping("stream_id")]
            public PrcHash40 StreamId { get; set; }

            [PrcHexMapping("data_name0")]
            public string DateName0 { get; set; }

            [PrcHexMapping("data_name1")]
            public string DateName1 { get; set; }

            [PrcHexMapping("data_name2")]
            public string DateName2 { get; set; }

            [PrcHexMapping("data_name3")]
            public string DateName3 { get; set; }

            [PrcHexMapping("data_name4")]
            public string DateName4 { get; set; }

            [PrcHexMapping("loop")]
            public byte Loop { get; set; }

            [PrcHexMapping("end_point")]
            public string EndPoint { get; set; }

            [PrcHexMapping("fadeout_frame")]
            public ushort FadeOutFrame { get; set; }

            [PrcHexMapping("start_point_suddendeath")]
            public string StartPointSuddenDeath { get; set; }

            [PrcHexMapping("start_point_transition")]
            public string StartPointTransition { get; set; }

            [PrcHexMapping("start_point0")]
            public string StartPoint0 { get; set; }

            [PrcHexMapping("start_point1")]
            public string StartPoint1 { get; set; }

            [PrcHexMapping("start_point2")]
            public string StartPoint2 { get; set; }

            [PrcHexMapping("start_point3")]
            public string StartPoint3 { get; set; }

            [PrcHexMapping("start_point4")]
            public string StartPoint4 { get; set; }

            public override string ToString()
            {
                return StreamId.ToString();
            }
        }

        public class PrcBgmFighterJingleBgmEntry
        {
            [PrcHexMapping("ui_chara_id")]
            public PrcHash40 UiCharaId { get; set; }

            [PrcHexMapping("data_name")]
            public string DataName { get; set; }

            public override string ToString()
            {
                return UiCharaId.ToString();
            }
        }

        public class PrcBgmPlaylistEntry
        {
            [PrcHexMapping("ui_bgm_id")]
            public PrcHash40 UiBgmId { get; set; }

            [PrcHexMapping("order0")]
            public short Order0 { get; set; }

            [PrcHexMapping("incidence0")]
            public ushort Incidence0 { get; set; }

            [PrcHexMapping("order1")]
            public short Order1 { get; set; }

            [PrcHexMapping("incidence1")]
            public ushort Incidence1 { get; set; }

            [PrcHexMapping("order2")]
            public short Order2 { get; set; }

            [PrcHexMapping("incidence2")]
            public ushort Incidence2 { get; set; }

            [PrcHexMapping("order3")]
            public short Order3 { get; set; }

            [PrcHexMapping("incidence3")]
            public ushort Incidence3 { get; set; }

            [PrcHexMapping("order4")]
            public short Order4 { get; set; }

            [PrcHexMapping("incidence4")]
            public ushort Incidence4 { get; set; }

            [PrcHexMapping("order5")]
            public short Order5 { get; set; }

            [PrcHexMapping("incidence5")]
            public ushort Incidence5 { get; set; }

            [PrcHexMapping("order6")]
            public short Order6 { get; set; }

            [PrcHexMapping("incidence6")]
            public ushort Incidence6 { get; set; }

            [PrcHexMapping("order7")]
            public short Order7 { get; set; }

            [PrcHexMapping("incidence7")]
            public ushort Incidence7 { get; set; }

            [PrcHexMapping("order8")]
            public short Order8 { get; set; }

            [PrcHexMapping("incidence8")]
            public ushort Incidence8 { get; set; }

            [PrcHexMapping("order9")]
            public short Order9 { get; set; }

            [PrcHexMapping("incidence9")]
            public ushort Incidence9 { get; set; }

            [PrcHexMapping("order10")]
            public short Order10 { get; set; }

            [PrcHexMapping("incidence10")]
            public ushort Incidence10 { get; set; }

            [PrcHexMapping("order11")]
            public short Order11 { get; set; }

            [PrcHexMapping("incidence11")]
            public ushort Incidence11 { get; set; }

            [PrcHexMapping("order12")]
            public short Order12 { get; set; }

            [PrcHexMapping("incidence12")]
            public ushort Incidence12 { get; set; }

            [PrcHexMapping("order13")]
            public short Order13 { get; set; }

            [PrcHexMapping("incidence13")]
            public ushort Incidence13 { get; set; }

            [PrcHexMapping("order14")]
            public short Order14 { get; set; }

            [PrcHexMapping("incidence14")]
            public ushort Incidence14 { get; set; }

            [PrcHexMapping("order15")]
            public short Order15 { get; set; }

            [PrcHexMapping("incidence15")]
            public ushort Incidence15 { get; set; }

            public override string ToString()
            {
                return UiBgmId.ToString();
            }
        }
    }
}
