using Sma5h.Interfaces;
using Sma5h.ResourceProviders.Prc.Helpers;
using System.Collections.Generic;

namespace Sma5h.Data.Ui.Param.Database
{
    public class PrcUiBgmDatabase : IStateManagerDb
    {
        [PrcDictionary("ui_bgm_id")]
        [PrcHexMapping("db_root")]
        public Dictionary<string, PrcUiBgmDatabaseModels.PrcBgmDbRootEntry> DbRootEntries { get; set; }

        [PrcDictionary("ui_bgm_id")]
        [PrcHexMapping("stage_bgm")]
        public Dictionary<string, PrcUiBgmDatabaseModels.PrcBgmStageBgmEntry> StageBgmEntries { get; set; }

        [PrcDictionary("stream_set_id")]
        [PrcHexMapping("stream_set")]
        public Dictionary<string, PrcUiBgmDatabaseModels.PrcBgmStreamSetEntry> StreamSetEntries { get; set; }

        [PrcDictionary("info_id")]
        [PrcHexMapping("assigned_info")]
        public Dictionary<string, PrcUiBgmDatabaseModels.PrcBgmAssignedInfoEntry> AssignedInfoEntries { get; set; }

        [PrcDictionary("stream_id")]
        [PrcHexMapping("stream_property")]
        public Dictionary<string, PrcUiBgmDatabaseModels.PrcBgmStreamPropertyEntry> StreamPropertyEntries { get; set; }

        [PrcDictionary("ui_chara_id")]
        [PrcHexMapping("fighter_jingle")]
        public Dictionary<string, PrcUiBgmDatabaseModels.PrcBgmFighterJingleBgmEntry> FighterJingleEntries { get; set; }

        [PrcFilterMatch("^(bgm)")]
        public List<PcrFilterStruct<PrcUiBgmDatabaseModels.PrcBgmPlaylistEntry>> PlaylistEntries { get; set; }
    }

    namespace PrcUiBgmDatabaseModels
    {

        public class PrcBgmDbRootEntry
        {
            [PrcHexMapping("ui_bgm_id", true)]
            public string UiBgmId { get; set; }

            [PrcHexMapping("stream_set_id", true)]
            public string StreamSetId { get; set; }

            [PrcHexMapping("rarity", true)]
            public string Rarity { get; set; }

            [PrcHexMapping("record_type", true)]
            public string RecordType { get; set; }

            [PrcHexMapping("ui_gametitle_id", true)]
            public string UiGameTitleId { get; set; }

            [PrcHexMapping("ui_gametitle_id_1", true)]
            public string UiGameTitleId1 { get; set; }

            [PrcHexMapping("ui_gametitle_id_2", true)]
            public string UiGameTitleId2 { get; set; }

            [PrcHexMapping("ui_gametitle_id_3", true)]
            public string UiGameTitleId3 { get; set; }

            [PrcHexMapping("ui_gametitle_id_4", true)]
            public string UiGameTitleId4 { get; set; }

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

            [PrcHexMapping("is_selectable_movie_edit")]
            public bool IsSelectableMovieEdit { get; set; }

            [PrcHexMapping("is_selectable_original")]
            public bool IsSelectableOriginal { get; set; }

            [PrcHexMapping("is_dlc")]
            public bool IsDlc { get; set; }

            [PrcHexMapping("is_patch")]
            public bool IsPatch { get; set; }

            [PrcHexMapping("dlc_ui_chara_id", true)]
            public string DlcUiCharaId { get; set; }

            [PrcHexMapping("dlc_mii_hat_motif_id", true)]
            public string DlcMiiHatMotifId { get; set; }

            [PrcHexMapping("dlc_mii_body_motif_id", true)]
            public string DlcMiiBodyMotifId { get; set; }

            public override string ToString()
            {
                return UiBgmId.ToString();
            }
        }

        public class PrcBgmStageBgmEntry
        {
            [PrcHexMapping("ui_bgm_id", true)]
            public string UiBgmId { get; set; }

            [PrcHexMapping("stream_set_id", true)]
            public string StreamSetId { get; set; }

            [PrcHexMapping("ui_stage_id", true)]
            public string UiStageId { get; set; }

            public override string ToString()
            {
                return UiBgmId.ToString();
            }
        }

        public class PrcBgmStreamSetEntry
        {
            [PrcHexMapping("stream_set_id", true)]
            public string StreamSetId { get; set; }

            [PrcHexMapping("special_category", true)]
            public string SpecialCategory { get; set; }

            [PrcHexMapping("info0", true)]
            public string Info0 { get; set; }

            [PrcHexMapping("info1", true)]
            public string Info1 { get; set; }

            [PrcHexMapping("info2", true)]
            public string Info2 { get; set; }

            [PrcHexMapping("info3", true)]
            public string Info3 { get; set; }

            [PrcHexMapping("info4", true)]
            public string Info4 { get; set; }

            [PrcHexMapping("info5", true)]
            public string Info5 { get; set; }

            [PrcHexMapping("info6", true)]
            public string Info6 { get; set; }

            [PrcHexMapping("info7", true)]
            public string Info7 { get; set; }

            [PrcHexMapping("info8", true)]
            public string Info8 { get; set; }

            [PrcHexMapping("info9", true)]
            public string Info9 { get; set; }

            [PrcHexMapping("info10", true)]
            public string Info10 { get; set; }

            [PrcHexMapping("info11", true)]
            public string Info11 { get; set; }

            [PrcHexMapping("info12", true)]
            public string Info12 { get; set; }

            [PrcHexMapping("info13", true)]
            public string Info13 { get; set; }

            [PrcHexMapping("info14", true)]
            public string Info14 { get; set; }

            [PrcHexMapping("info15", true)]
            public string Info15 { get; set; }

            public override string ToString()
            {
                return StreamSetId.ToString();
            }
        }

        public class PrcBgmAssignedInfoEntry
        {
            [PrcHexMapping("info_id", true)]
            public string InfoId { get; set; }

            [PrcHexMapping("stream_id", true)]
            public string StreamId { get; set; }

            [PrcHexMapping("condition", true)]
            public string Condition { get; set; }

            [PrcHexMapping("condition_process", true)]
            public string ConditionProcess { get; set; }

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

            [PrcHexMapping("menu_change_stop_delay_frame")]
            public int MenuChangeStopDelayFrame { get; set; }

            public override string ToString()
            {
                return InfoId.ToString();
            }
        }

        public class PrcBgmStreamPropertyEntry
        {
            [PrcHexMapping("stream_id", true)]
            public string StreamId { get; set; }

            [PrcHexMapping("data_name0")]
            public string DataName0 { get; set; }

            [PrcHexMapping("data_name1")]
            public string DataName1 { get; set; }

            [PrcHexMapping("data_name2")]
            public string DataName2 { get; set; }

            [PrcHexMapping("data_name3")]
            public string DataName3 { get; set; }

            [PrcHexMapping("data_name4")]
            public string DataName4 { get; set; }

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
            [PrcHexMapping("ui_chara_id", true)]
            public string UiCharaId { get; set; }

            [PrcHexMapping("data_name")]
            public string DataName { get; set; }

            public override string ToString()
            {
                return UiCharaId.ToString();
            }
        }

        public class PrcBgmPlaylistEntry
        {
            [PrcHexMapping("ui_bgm_id", true)]
            public string UiBgmId { get; set; }

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

            public void SetOrder(short orderValue, int index = -1)
            {
                var all = index == -1;

                if (all || index == 0)
                    Order0 = orderValue;
                if (all || index == 1)
                    Order1 = orderValue;
                if (all || index == 2)
                    Order2 = orderValue;
                if (all || index == 3)
                    Order3 = orderValue;
                if (all || index == 4)
                    Order4 = orderValue;
                if (all || index == 5)
                    Order5 = orderValue;
                if (all || index == 6)
                    Order6 = orderValue;
                if (all || index == 7)
                    Order7 = orderValue;
                if (all || index == 8)
                    Order8 = orderValue;
                if (all || index == 9)
                    Order9 = orderValue;
                if (all || index == 10)
                    Order10 = orderValue;
                if (all || index == 11)
                    Order11 = orderValue;
                if (all || index == 12)
                    Order12 = orderValue;
                if (all || index == 13)
                    Order13 = orderValue;
                if (all || index == 14)
                    Order14 = orderValue;
                if (all || index == 15)
                    Order15 = orderValue;
            }

            public void SetIncidence(ushort incidenceValue, int index = -1)
            {
                var all = index == -1;

                if (all || index == 0)
                    Incidence0 = incidenceValue;
                if (all || index == 1)
                    Incidence1 = incidenceValue;
                if (all || index == 2)
                    Incidence2 = incidenceValue;
                if (all || index == 3)
                    Incidence3 = incidenceValue;
                if (all || index == 4)
                    Incidence4 = incidenceValue;
                if (all || index == 5)
                    Incidence5 = incidenceValue;
                if (all || index == 6)
                    Incidence6 = incidenceValue;
                if (all || index == 7)
                    Incidence7 = incidenceValue;
                if (all || index == 8)
                    Incidence8 = incidenceValue;
                if (all || index == 9)
                    Incidence9 = incidenceValue;
                if (all || index == 10)
                    Incidence10 = incidenceValue;
                if (all || index == 11)
                    Incidence11 = incidenceValue;
                if (all || index == 12)
                    Incidence12 = incidenceValue;
                if (all || index == 13)
                    Incidence13 = incidenceValue;
                if (all || index == 14)
                    Incidence14 = incidenceValue;
                if (all || index == 15)
                    Incidence15 = incidenceValue;
            }

            public override string ToString()
            {
                return UiBgmId.ToString();
            }
        }
    }
}
