using CsvHelper.Configuration;
using Sma5h.Mods.Music.Models;
using Sma5hMusic.GUI.ViewModels;

namespace Sma5hMusic.GUI.Models.DevTools
{
    public class CSVSongExportEntry
    {
        public BgmDbRootEntryViewModel DbRoot { get; set; }
        public GameTitleEntryViewModel GameTitle { get; set; }
        public BgmStreamSetEntryViewModel StreamSet { get; set; }
        public BgmAssignedInfoEntryViewModel AssignedInfo { get; set; }
        public BgmStreamPropertyEntryViewModel StreamProperty { get; set; }
        public BgmPropertyEntryViewModel BgmProperty { get; set; }

    }

    public class CSVSongExportEntryMap : ClassMap<CSVSongExportEntry>
    {
        public CSVSongExportEntryMap()
        {
            Map(m => m.DbRoot.UiBgmId).Name("db_root.ui_bgm_id").Index(0);
            Map(m => m.DbRoot.StreamSetId).Name("db_root.stream_set_id").Index(1);
            Map(m => m.DbRoot.Title).Name("msbt_bgm.title").Index(2);
            Map(m => m.DbRoot.Author).Name("msbt_bgm.author").Index(3);
            Map(m => m.DbRoot.Copyright).Name("msbt_bgm.copyright").Index(4);
            Map(m => m.DbRoot.Rarity).Name("db_root.rarity").Index(5);
            Map(m => m.DbRoot.UiGameTitleId).Name("db_root.ui_gametitle_id").Index(6);
            Map(m => m.GameTitle.Title).Name("msbt_game.gametitle").Index(7);
            Map(m => m.DbRoot.UiGameTitleId1).Name("db_root.ui_gametitle_id_1").Index(8);
            Map(m => m.DbRoot.UiGameTitleId2).Name("db_root.ui_gametitle_id_2").Index(9);
            Map(m => m.DbRoot.UiGameTitleId3).Name("db_root.ui_gametitle_id_3").Index(10);
            Map(m => m.DbRoot.UiGameTitleId4).Name("db_root.ui_gametitle_id_4").Index(11);
            Map(m => m.DbRoot.NameId).Name("db_root.name_id").Index(12);
            Map(m => m.DbRoot.SaveNo).Name("db_root.save_no").Index(13);
            Map(m => m.DbRoot.TestDispOrder).Name("db_root.test_disp_order").Index(14);
            Map(m => m.DbRoot.MenuValue).Name("db_root.menu_value").Index(15);
            Map(m => m.DbRoot.JpRegion).Name("db_root.jp_region").Index(16);
            Map(m => m.DbRoot.OtherRegion).Name("db_root.other_region").Index(17);
            Map(m => m.DbRoot.Possessed).Name("db_root.possessed").Index(18);
            Map(m => m.DbRoot.PrizeLottery).Name("db_root.prize_lottery").Index(19);
            Map(m => m.DbRoot.ShopPrice).Name("db_root.shop_price").Index(20);
            Map(m => m.DbRoot.CountTarget).Name("db_root.count_target").Index(21);
            Map(m => m.DbRoot.MenuLoop).Name("db_root.menu_loop").Index(22);
            Map(m => m.DbRoot.IsSelectableStageMake).Name("db_root.is_selectable_stage_make").Index(23);
            Map(m => m.DbRoot.Unk1).Name("db_root.is_selectable_video_edit").Index(24);
            Map(m => m.DbRoot.Unk2).Name("db_root.is_selectable_original").Index(25);
            Map(m => m.DbRoot.IsDlc).Name("db_root.is_dlc").Index(26);
            Map(m => m.DbRoot.IsPatch).Name("db_root.is_patch").Index(27);
            Map(m => m.DbRoot.Unk3).Name("db_root.0x0ff71e57ec").Index(28);
            Map(m => m.DbRoot.Unk4).Name("db_root.0x14341640b8").Index(29);
            Map(m => m.DbRoot.Unk5).Name("db_root.0x1560c0949b").Index(30);
            Map(m => m.StreamSet.StreamSetId).Name("stream_set.stream_set_id").Index(31);
            Map(m => m.StreamSet.SpecialCategory).Name("stream_set.special_category").Index(32);
            Map(m => m.StreamSet.Info0).Name("stream_set.info0").Index(33);
            Map(m => m.StreamSet.Info1).Name("stream_set.info1").Index(34);
            Map(m => m.StreamSet.Info2).Name("stream_set.info2").Index(35);
            Map(m => m.StreamSet.Info3).Name("stream_set.info3").Index(36);
            Map(m => m.StreamSet.Info4).Name("stream_set.info4").Index(37);
            Map(m => m.StreamSet.Info5).Name("stream_set.info5").Index(38);
            Map(m => m.StreamSet.Info6).Name("stream_set.info6").Index(39);
            Map(m => m.StreamSet.Info7).Name("stream_set.info7").Index(40);
            Map(m => m.StreamSet.Info8).Name("stream_set.info8").Index(41);
            Map(m => m.StreamSet.Info9).Name("stream_set.info9").Index(42);
            Map(m => m.StreamSet.Info10).Name("stream_set.info10").Index(43);
            Map(m => m.StreamSet.Info11).Name("stream_set.info11").Index(44);
            Map(m => m.StreamSet.Info12).Name("stream_set.info12").Index(45);
            Map(m => m.StreamSet.Info13).Name("stream_set.info13").Index(46);
            Map(m => m.StreamSet.Info14).Name("stream_set.info14").Index(47);
            Map(m => m.StreamSet.Info15).Name("stream_set.info15").Index(48);
            Map(m => m.AssignedInfo.InfoId).Name("assigned_info.info_id").Index(49);
            Map(m => m.AssignedInfo.StreamId).Name("assigned_info.stream_id").Index(50);
            Map(m => m.AssignedInfo.Condition).Name("assigned_info.condition").Index(51);
            Map(m => m.AssignedInfo.ConditionProcess).Name("assigned_info.condition_process").Index(52);
            Map(m => m.AssignedInfo.StartFrame).Name("assigned_info.start_frame").Index(53);
            Map(m => m.AssignedInfo.ChangeFadeInFrame).Name("assigned_info.change_fadein_frame").Index(54);
            Map(m => m.AssignedInfo.ChangeStartDelayFrame).Name("assigned_info.change_start_delay_frame").Index(55);
            Map(m => m.AssignedInfo.ChangeFadoutFrame).Name("assigned_info.change_fadeout_frame").Index(56);
            Map(m => m.AssignedInfo.ChangeStopDelayFrame).Name("assigned_info.change_stop_delay_frame").Index(57);
            Map(m => m.AssignedInfo.MenuChangeFadeInFrame).Name("assigned_info.menu_change_fadein_frame").Index(58);
            Map(m => m.AssignedInfo.MenuChangeStartDelayFrame).Name("assigned_info.menu_change_start_delay_frame").Index(59);
            Map(m => m.AssignedInfo.MenuChangeFadeOutFrame).Name("assigned_info.menu_change_fadeout_frame").Index(60);
            Map(m => m.AssignedInfo.Unk1).Name("assigned_info.menu_change_stop_delay_frame").Index(61);
            Map(m => m.StreamProperty.StreamId).Name("stream_property.stream_id").Index(62);
            Map(m => m.StreamProperty.DataName0).Name("stream_property.data_name0").Index(63);
            Map(m => m.StreamProperty.DataName1).Name("stream_property.data_name1").Index(64);
            Map(m => m.StreamProperty.DataName2).Name("stream_property.data_name2").Index(65);
            Map(m => m.StreamProperty.DataName3).Name("stream_property.data_name3").Index(66);
            Map(m => m.StreamProperty.DataName4).Name("stream_property.data_name4").Index(67);
            Map(m => m.StreamProperty.Loop).Name("stream_property.loop").Index(68);
            Map(m => m.StreamProperty.EndPoint).Name("stream_property.end_point").Index(69);
            Map(m => m.StreamProperty.FadeOutFrame).Name("stream_property.fadeout_frame").Index(70);
            Map(m => m.StreamProperty.StartPointSuddenDeath).Name("stream_property.start_point_suddendeath").Index(71);
            Map(m => m.StreamProperty.StartPointTransition).Name("stream_property.start_point_transition").Index(72);
            Map(m => m.StreamProperty.StartPoint0).Name("stream_property.start_point0").Index(73);
            Map(m => m.StreamProperty.StartPoint1).Name("stream_property.start_point1").Index(74);
            Map(m => m.StreamProperty.StartPoint2).Name("stream_property.start_point2").Index(75);
            Map(m => m.StreamProperty.StartPoint3).Name("stream_property.start_point3").Index(76);
            Map(m => m.StreamProperty.StartPoint4).Name("stream_property.start_point4").Index(77);
            Map(m => m.BgmProperty.NameId).Name("bgm_property.name_id").Index(78);
            Map(m => m.BgmProperty.LoopStartMs).Name("bgm_property.loop_start_ms").Index(79);
            Map(m => m.BgmProperty.LoopStartSample).Name("bgm_property.loop_start_sample").Index(80);
            Map(m => m.BgmProperty.LoopEndMs).Name("bgm_property.loop_end_ms").Index(81);
            Map(m => m.BgmProperty.LoopEndSample).Name("bgm_property.loop_end_sample").Index(82);
            Map(m => m.BgmProperty.TotalTimeMs).Name("bgm_property.total_time_ms").Index(83);
            Map(m => m.BgmProperty.TotalSamples).Name("bgm_property.total_samples").Index(84);
            Map(m => m.BgmProperty.Frequency).Name("bgm_property.frequency").Index(85);
            Map(m => m.BgmProperty.AudioVolume).Name("bgm_property.volume").Index(86);
            Map(m => m.BgmProperty.Filename).Name("bgm_property.filename").Index(87);
            Map(m => m.DbRoot.ModPath).Name("mod.mod_path").Index(88);
        }
    }
}
