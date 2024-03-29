﻿using AutoMapper;

namespace Sma5h.Mods.Music.Models.AutoMapper
{
    public class MappingDb : Profile
    {
        public MappingDb()
        {
            CreateMap<AudioCuePoints, BgmPropertyEntry>()
               .ForMember(i => i.AudioVolume, me => me.Ignore())
               .ForMember(i => i.LoopEndMs, me => me.MapFrom(p => p.LoopEndMs))
               .ForMember(i => i.LoopEndSample, me => me.MapFrom(p => p.LoopEndSample))
               .ForMember(i => i.LoopStartMs, me => me.MapFrom(p => p.LoopStartMs))
               .ForMember(i => i.LoopStartSample, me => me.MapFrom(p => p.LoopStartSample))
               .ForMember(i => i.NameId, me => me.Ignore())
               .ForMember(i => i.TotalSamples, me => me.MapFrom(p => p.TotalSamples))
               .ForMember(i => i.TotalTimeMs, me => me.MapFrom(p => p.TotalTimeMs));
            CreateMap<BgmPropertyEntry, AudioCuePoints>()
               .ForMember(i => i.LoopEndMs, me => me.MapFrom(p => p.LoopEndMs))
               .ForMember(i => i.LoopEndSample, me => me.MapFrom(p => p.LoopEndSample))
               .ForMember(i => i.LoopStartMs, me => me.MapFrom(p => p.LoopStartMs))
               .ForMember(i => i.LoopStartSample, me => me.MapFrom(p => p.LoopStartSample))
               .ForMember(i => i.TotalSamples, me => me.MapFrom(p => p.TotalSamples))
               .ForMember(i => i.TotalTimeMs, me => me.MapFrom(p => p.TotalTimeMs));

            CreateMap<Sma5h.Data.Ui.Param.Database.PrcUiGameTitleDatabaseModels.PrcGameTitleDbRootEntry, GameTitleEntry>()
                .ForMember(i => i.MSBTTitle, me => me.Ignore())
                .ForMember(i => i.NameId, me => me.MapFrom(p => p.NameId))
                .ForMember(i => i.Release, me => me.MapFrom(p => p.Release))
                .ForMember(i => i.UiGameTitleId, me => me.Ignore())
                .ForMember(i => i.UiSeriesId, me => me.MapFrom(p => p.UiSeriesId))
                .ForMember(i => i.Unk1, me => me.MapFrom(p => p.Unk1));
            CreateMap<GameTitleEntry, Sma5h.Data.Ui.Param.Database.PrcUiGameTitleDatabaseModels.PrcGameTitleDbRootEntry>()
                .ForMember(i => i.NameId, me => me.MapFrom(p => p.NameId))
                .ForMember(i => i.Release, me => me.MapFrom(p => p.Release))
                .ForMember(i => i.UiGameTitleId, me => me.MapFrom(p => p.UiGameTitleId))
                .ForMember(i => i.UiSeriesId, me => me.MapFrom(p => p.UiSeriesId))
                .ForMember(i => i.Unk1, me => me.MapFrom(p => p.Unk1));

            CreateMap<Data.Sound.Config.BgmPropertyStructs.BgmPropertyEntry, BgmPropertyEntry>()
                .ForMember(i => i.AudioVolume, me => me.Ignore())
                .ForMember(i => i.LoopEndMs, me => me.MapFrom(p => p.LoopEndMs))
                .ForMember(i => i.LoopEndSample, me => me.MapFrom(p => p.LoopEndSample))
                .ForMember(i => i.LoopStartMs, me => me.MapFrom(p => p.LoopStartMs))
                .ForMember(i => i.LoopStartSample, me => me.MapFrom(p => p.LoopStartSample))
                .ForMember(i => i.TotalSamples, me => me.MapFrom(p => p.TotalSamples))
                .ForMember(i => i.TotalTimeMs, me => me.MapFrom(p => p.TotalTimeMs))
                .ForMember(i => i.NameId, me => me.Ignore());
            CreateMap<BgmPropertyEntry, Data.Sound.Config.BgmPropertyStructs.BgmPropertyEntry>()
                .ForMember(i => i.LoopEndMs, me => me.MapFrom(p => p.LoopEndMs))
                .ForMember(i => i.LoopEndSample, me => me.MapFrom(p => p.LoopEndSample))
                .ForMember(i => i.LoopStartMs, me => me.MapFrom(p => p.LoopStartMs))
                .ForMember(i => i.LoopStartSample, me => me.MapFrom(p => p.LoopStartSample))
                .ForMember(i => i.TotalSamples, me => me.MapFrom(p => p.TotalSamples))
                .ForMember(i => i.TotalTimeMs, me => me.MapFrom(p => p.TotalTimeMs))
                .ForMember(i => i.NameId, me => me.MapFrom(p => p.NameId));

            CreateMap<Sma5h.Data.Ui.Param.Database.PrcUiBgmDatabaseModels.PrcBgmDbRootEntry, BgmDbRootEntry>()
                .ForMember(i => i.CountTarget, me => me.MapFrom(p => p.CountTarget))
                .ForMember(i => i.IsDlc, me => me.MapFrom(p => p.IsDlc))
                .ForMember(i => i.IsPatch, me => me.MapFrom(p => p.IsPatch))
                .ForMember(i => i.IsSelectableStageMake, me => me.MapFrom(p => p.IsSelectableStageMake))
                .ForMember(i => i.IsSelectableMovieEdit, me => me.MapFrom(p => p.IsSelectableMovieEdit))
                .ForMember(i => i.IsSelectableOriginal, me => me.MapFrom(p => p.IsSelectableOriginal))
                .ForMember(i => i.JpRegion, me => me.MapFrom(p => p.JpRegion))
                .ForMember(i => i.MenuLoop, me => me.MapFrom(p => p.MenuLoop))
                .ForMember(i => i.MenuValue, me => me.MapFrom(p => p.MenuValue))
                .ForMember(i => i.NameId, me => me.MapFrom(p => p.NameId))
                .ForMember(i => i.OtherRegion, me => me.MapFrom(p => p.OtherRegion))
                .ForMember(i => i.Possessed, me => me.MapFrom(p => p.Possessed))
                .ForMember(i => i.PrizeLottery, me => me.MapFrom(p => p.PrizeLottery))
                .ForMember(i => i.Rarity, me => me.MapFrom(p => p.Rarity))
                .ForMember(i => i.RecordType, me => me.MapFrom(p => p.RecordType))
                .ForMember(i => i.SaveNo, me => me.MapFrom(p => p.SaveNo))
                .ForMember(i => i.ShopPrice, me => me.MapFrom(p => p.ShopPrice))
                .ForMember(i => i.StreamSetId, me => me.MapFrom(p => p.StreamSetId))
                .ForMember(i => i.TestDispOrder, me => me.MapFrom(p => p.TestDispOrder))
                .ForMember(i => i.UiBgmId, me => me.Ignore())
                .ForMember(i => i.UiGameTitleId, me => me.MapFrom(p => p.UiGameTitleId))
                .ForMember(i => i.UiGameTitleId1, me => me.MapFrom(p => p.UiGameTitleId1))
                .ForMember(i => i.UiGameTitleId2, me => me.MapFrom(p => p.UiGameTitleId2))
                .ForMember(i => i.UiGameTitleId3, me => me.MapFrom(p => p.UiGameTitleId3))
                .ForMember(i => i.UiGameTitleId4, me => me.MapFrom(p => p.UiGameTitleId4))
                .ForMember(i => i.DlcUiCharaId, me => me.MapFrom(p => p.DlcUiCharaId))
                .ForMember(i => i.DlcMiiHatMotifId, me => me.MapFrom(p => p.DlcMiiHatMotifId))
                .ForMember(i => i.DlcMiiBodyMotifId, me => me.MapFrom(p => p.DlcMiiBodyMotifId))
                .ForMember(i => i.Unk6, me => me.MapFrom(p => p.Unk6));
            CreateMap<BgmDbRootEntry, Sma5h.Data.Ui.Param.Database.PrcUiBgmDatabaseModels.PrcBgmDbRootEntry>()
                .ForMember(i => i.CountTarget, me => me.MapFrom(p => p.CountTarget))
                .ForMember(i => i.IsDlc, me => me.MapFrom(p => p.IsDlc))
                .ForMember(i => i.IsPatch, me => me.MapFrom(p => p.IsPatch))
                .ForMember(i => i.IsSelectableStageMake, me => me.MapFrom(p => p.IsSelectableStageMake))
                .ForMember(i => i.IsSelectableMovieEdit, me => me.MapFrom(p => p.IsSelectableMovieEdit))
                .ForMember(i => i.IsSelectableOriginal, me => me.MapFrom(p => p.IsSelectableOriginal))
                .ForMember(i => i.JpRegion, me => me.MapFrom(p => p.JpRegion))
                .ForMember(i => i.MenuLoop, me => me.MapFrom(p => p.MenuLoop))
                .ForMember(i => i.MenuValue, me => me.MapFrom(p => p.MenuValue))
                .ForMember(i => i.NameId, me => me.MapFrom(p => p.NameId))
                .ForMember(i => i.OtherRegion, me => me.MapFrom(p => p.OtherRegion))
                .ForMember(i => i.Possessed, me => me.MapFrom(p => p.Possessed))
                .ForMember(i => i.PrizeLottery, me => me.MapFrom(p => p.PrizeLottery))
                .ForMember(i => i.Rarity, me => me.MapFrom(p => p.Rarity))
                .ForMember(i => i.RecordType, me => me.MapFrom(p => p.RecordType))
                .ForMember(i => i.SaveNo, me => me.MapFrom(p => p.SaveNo))
                .ForMember(i => i.ShopPrice, me => me.MapFrom(p => p.ShopPrice))
                .ForMember(i => i.StreamSetId, me => me.MapFrom(p => p.StreamSetId))
                .ForMember(i => i.TestDispOrder, me => me.MapFrom(p => p.TestDispOrder))
                .ForMember(i => i.UiBgmId, me => me.MapFrom(p => p.UiBgmId))
                .ForMember(i => i.UiGameTitleId, me => me.MapFrom(p => p.UiGameTitleId))
                .ForMember(i => i.UiGameTitleId1, me => me.MapFrom(p => p.UiGameTitleId1))
                .ForMember(i => i.UiGameTitleId2, me => me.MapFrom(p => p.UiGameTitleId2))
                .ForMember(i => i.UiGameTitleId3, me => me.MapFrom(p => p.UiGameTitleId3))
                .ForMember(i => i.UiGameTitleId4, me => me.MapFrom(p => p.UiGameTitleId4))
                .ForMember(i => i.DlcUiCharaId, me => me.MapFrom(p => p.DlcUiCharaId))
                .ForMember(i => i.DlcMiiHatMotifId, me => me.MapFrom(p => p.DlcMiiHatMotifId))
                .ForMember(i => i.DlcMiiBodyMotifId, me => me.MapFrom(p => p.DlcMiiBodyMotifId))
                .ForMember(i => i.Unk6, me => me.MapFrom(p => p.Unk6));

            CreateMap<Sma5h.Data.Ui.Param.Database.PrcUiBgmDatabaseModels.PrcBgmStreamSetEntry, BgmStreamSetEntry>()
                .ForMember(i => i.Info0, me => me.MapFrom(p => p.Info0))
                .ForMember(i => i.Info1, me => me.MapFrom(p => p.Info1))
                .ForMember(i => i.Info2, me => me.MapFrom(p => p.Info2))
                .ForMember(i => i.Info3, me => me.MapFrom(p => p.Info3))
                .ForMember(i => i.Info4, me => me.MapFrom(p => p.Info4))
                .ForMember(i => i.Info5, me => me.MapFrom(p => p.Info5))
                .ForMember(i => i.Info6, me => me.MapFrom(p => p.Info6))
                .ForMember(i => i.Info7, me => me.MapFrom(p => p.Info7))
                .ForMember(i => i.Info8, me => me.MapFrom(p => p.Info8))
                .ForMember(i => i.Info9, me => me.MapFrom(p => p.Info9))
                .ForMember(i => i.Info10, me => me.MapFrom(p => p.Info10))
                .ForMember(i => i.Info11, me => me.MapFrom(p => p.Info11))
                .ForMember(i => i.Info12, me => me.MapFrom(p => p.Info12))
                .ForMember(i => i.Info13, me => me.MapFrom(p => p.Info13))
                .ForMember(i => i.Info14, me => me.MapFrom(p => p.Info14))
                .ForMember(i => i.Info15, me => me.MapFrom(p => p.Info15))
                .ForMember(i => i.SpecialCategory, me => me.MapFrom(p => p.SpecialCategory))
                .ForMember(i => i.StreamSetId, me => me.Ignore());
            CreateMap<BgmStreamSetEntry, Sma5h.Data.Ui.Param.Database.PrcUiBgmDatabaseModels.PrcBgmStreamSetEntry>()
                .ForMember(i => i.Info0, me => me.MapFrom(p => p.Info0))
                .ForMember(i => i.Info1, me => me.MapFrom(p => p.Info1))
                .ForMember(i => i.Info2, me => me.MapFrom(p => p.Info2))
                .ForMember(i => i.Info3, me => me.MapFrom(p => p.Info3))
                .ForMember(i => i.Info4, me => me.MapFrom(p => p.Info4))
                .ForMember(i => i.Info5, me => me.MapFrom(p => p.Info5))
                .ForMember(i => i.Info6, me => me.MapFrom(p => p.Info6))
                .ForMember(i => i.Info7, me => me.MapFrom(p => p.Info7))
                .ForMember(i => i.Info8, me => me.MapFrom(p => p.Info8))
                .ForMember(i => i.Info9, me => me.MapFrom(p => p.Info9))
                .ForMember(i => i.Info10, me => me.MapFrom(p => p.Info10))
                .ForMember(i => i.Info11, me => me.MapFrom(p => p.Info11))
                .ForMember(i => i.Info12, me => me.MapFrom(p => p.Info12))
                .ForMember(i => i.Info13, me => me.MapFrom(p => p.Info13))
                .ForMember(i => i.Info14, me => me.MapFrom(p => p.Info14))
                .ForMember(i => i.Info15, me => me.MapFrom(p => p.Info15))
                .ForMember(i => i.SpecialCategory, me => me.MapFrom(p => p.SpecialCategory))
                .ForMember(i => i.StreamSetId, me => me.MapFrom(p => p.StreamSetId));

            CreateMap<Sma5h.Data.Ui.Param.Database.PrcUiBgmDatabaseModels.PrcBgmAssignedInfoEntry, BgmAssignedInfoEntry>()
                .ForMember(i => i.ChangeFadeInFrame, me => me.MapFrom(p => p.ChangeFadeInFrame))
                .ForMember(i => i.ChangeFadoutFrame, me => me.MapFrom(p => p.ChangeFadoutFrame))
                .ForMember(i => i.ChangeStartDelayFrame, me => me.MapFrom(p => p.ChangeStartDelayFrame))
                .ForMember(i => i.ChangeStopDelayFrame, me => me.MapFrom(p => p.ChangeStopDelayFrame))
                .ForMember(i => i.Condition, me => me.MapFrom(p => p.Condition))
                .ForMember(i => i.ConditionProcess, me => me.MapFrom(p => p.ConditionProcess))
                .ForMember(i => i.InfoId, me => me.Ignore())
                .ForMember(i => i.MenuChangeFadeInFrame, me => me.MapFrom(p => p.MenuChangeFadeInFrame))
                .ForMember(i => i.MenuChangeFadeOutFrame, me => me.MapFrom(p => p.MenuChangeFadeOutFrame))
                .ForMember(i => i.MenuChangeStartDelayFrame, me => me.MapFrom(p => p.MenuChangeStartDelayFrame))
                .ForMember(i => i.MenuChangeStopDelayFrame, me => me.MapFrom(p => p.MenuChangeStopDelayFrame))
                .ForMember(i => i.StartFrame, me => me.MapFrom(p => p.StartFrame))
                .ForMember(i => i.StreamId, me => me.MapFrom(p => p.StreamId));
            CreateMap<BgmAssignedInfoEntry, Sma5h.Data.Ui.Param.Database.PrcUiBgmDatabaseModels.PrcBgmAssignedInfoEntry>()
                .ForMember(i => i.ChangeFadeInFrame, me => me.MapFrom(p => p.ChangeFadeInFrame))
                .ForMember(i => i.ChangeFadoutFrame, me => me.MapFrom(p => p.ChangeFadoutFrame))
                .ForMember(i => i.ChangeStartDelayFrame, me => me.MapFrom(p => p.ChangeStartDelayFrame))
                .ForMember(i => i.ChangeStopDelayFrame, me => me.MapFrom(p => p.ChangeStopDelayFrame))
                .ForMember(i => i.Condition, me => me.MapFrom(p => p.Condition))
                .ForMember(i => i.ConditionProcess, me => me.MapFrom(p => p.ConditionProcess))
                .ForMember(i => i.InfoId, me => me.MapFrom(p => p.InfoId))
                .ForMember(i => i.MenuChangeFadeInFrame, me => me.MapFrom(p => p.MenuChangeFadeInFrame))
                .ForMember(i => i.MenuChangeFadeOutFrame, me => me.MapFrom(p => p.MenuChangeFadeOutFrame))
                .ForMember(i => i.MenuChangeStartDelayFrame, me => me.MapFrom(p => p.MenuChangeStartDelayFrame))
                .ForMember(i => i.MenuChangeStopDelayFrame, me => me.MapFrom(p => p.MenuChangeStopDelayFrame))
                .ForMember(i => i.StartFrame, me => me.MapFrom(p => p.StartFrame))
                .ForMember(i => i.StreamId, me => me.MapFrom(p => p.StreamId));

            CreateMap<Sma5h.Data.Ui.Param.Database.PrcUiBgmDatabaseModels.PrcBgmStreamPropertyEntry, BgmStreamPropertyEntry>()
                .ForMember(i => i.DataName0, me => me.MapFrom(p => p.DataName0))
                .ForMember(i => i.DataName1, me => me.MapFrom(p => p.DataName1))
                .ForMember(i => i.DataName2, me => me.MapFrom(p => p.DataName2))
                .ForMember(i => i.DataName3, me => me.MapFrom(p => p.DataName3))
                .ForMember(i => i.DataName4, me => me.MapFrom(p => p.DataName4))
                .ForMember(i => i.EndPoint, me => me.MapFrom(p => p.EndPoint))
                .ForMember(i => i.FadeOutFrame, me => me.MapFrom(p => p.FadeOutFrame))
                .ForMember(i => i.Loop, me => me.MapFrom(p => p.Loop))
                .ForMember(i => i.StartPoint0, me => me.MapFrom(p => p.StartPoint0))
                .ForMember(i => i.StartPoint1, me => me.MapFrom(p => p.StartPoint1))
                .ForMember(i => i.StartPoint2, me => me.MapFrom(p => p.StartPoint2))
                .ForMember(i => i.StartPoint3, me => me.MapFrom(p => p.StartPoint3))
                .ForMember(i => i.StartPoint4, me => me.MapFrom(p => p.StartPoint4))
                .ForMember(i => i.StartPointSuddenDeath, me => me.MapFrom(p => p.StartPointSuddenDeath))
                .ForMember(i => i.StartPointTransition, me => me.MapFrom(p => p.StartPointTransition))
                .ForMember(i => i.StreamId, me => me.Ignore());
            CreateMap<BgmStreamPropertyEntry, Sma5h.Data.Ui.Param.Database.PrcUiBgmDatabaseModels.PrcBgmStreamPropertyEntry>()
                .ForMember(i => i.DataName0, me => me.MapFrom(p => p.DataName0))
                .ForMember(i => i.DataName1, me => me.MapFrom(p => p.DataName1))
                .ForMember(i => i.DataName2, me => me.MapFrom(p => p.DataName2))
                .ForMember(i => i.DataName3, me => me.MapFrom(p => p.DataName3))
                .ForMember(i => i.DataName4, me => me.MapFrom(p => p.DataName4))
                .ForMember(i => i.EndPoint, me => me.MapFrom(p => p.EndPoint))
                .ForMember(i => i.FadeOutFrame, me => me.MapFrom(p => p.FadeOutFrame))
                .ForMember(i => i.Loop, me => me.MapFrom(p => p.Loop))
                .ForMember(i => i.StartPoint0, me => me.MapFrom(p => p.StartPoint0))
                .ForMember(i => i.StartPoint1, me => me.MapFrom(p => p.StartPoint1))
                .ForMember(i => i.StartPoint2, me => me.MapFrom(p => p.StartPoint2))
                .ForMember(i => i.StartPoint3, me => me.MapFrom(p => p.StartPoint3))
                .ForMember(i => i.StartPoint4, me => me.MapFrom(p => p.StartPoint4))
                .ForMember(i => i.StartPointSuddenDeath, me => me.MapFrom(p => p.StartPointSuddenDeath))
                .ForMember(i => i.StartPointTransition, me => me.MapFrom(p => p.StartPointTransition))
                .ForMember(i => i.StreamId, me => me.MapFrom(p => p.StreamId));

            CreateMap<Sma5h.Data.Ui.Param.Database.PrcUiBgmDatabaseModels.PrcBgmPlaylistEntry, PlaylistEntryModels.PlaylistValueEntry>()
                .ForMember(i => i.UiBgmId, me => me.MapFrom(p => p.UiBgmId))
                .ForMember(i => i.Incidence0, me => me.MapFrom(p => p.Incidence0))
                .ForMember(i => i.Order0, me => me.MapFrom(p => p.Order0))
                .ForMember(i => i.Incidence1, me => me.MapFrom(p => p.Incidence1))
                .ForMember(i => i.Order1, me => me.MapFrom(p => p.Order1))
                .ForMember(i => i.Incidence2, me => me.MapFrom(p => p.Incidence2))
                .ForMember(i => i.Order2, me => me.MapFrom(p => p.Order2))
                .ForMember(i => i.Incidence3, me => me.MapFrom(p => p.Incidence3))
                .ForMember(i => i.Order3, me => me.MapFrom(p => p.Order3))
                .ForMember(i => i.Incidence4, me => me.MapFrom(p => p.Incidence4))
                .ForMember(i => i.Order4, me => me.MapFrom(p => p.Order4))
                .ForMember(i => i.Incidence5, me => me.MapFrom(p => p.Incidence5))
                .ForMember(i => i.Order5, me => me.MapFrom(p => p.Order5))
                .ForMember(i => i.Incidence6, me => me.MapFrom(p => p.Incidence6))
                .ForMember(i => i.Order6, me => me.MapFrom(p => p.Order6))
                .ForMember(i => i.Incidence7, me => me.MapFrom(p => p.Incidence7))
                .ForMember(i => i.Order7, me => me.MapFrom(p => p.Order7))
                .ForMember(i => i.Incidence8, me => me.MapFrom(p => p.Incidence8))
                .ForMember(i => i.Order8, me => me.MapFrom(p => p.Order8))
                .ForMember(i => i.Incidence9, me => me.MapFrom(p => p.Incidence9))
                .ForMember(i => i.Order9, me => me.MapFrom(p => p.Order9))
                .ForMember(i => i.Incidence10, me => me.MapFrom(p => p.Incidence10))
                .ForMember(i => i.Order10, me => me.MapFrom(p => p.Order10))
                .ForMember(i => i.Incidence11, me => me.MapFrom(p => p.Incidence11))
                .ForMember(i => i.Order11, me => me.MapFrom(p => p.Order11))
                .ForMember(i => i.Incidence12, me => me.MapFrom(p => p.Incidence12))
                .ForMember(i => i.Order12, me => me.MapFrom(p => p.Order12))
                .ForMember(i => i.Incidence13, me => me.MapFrom(p => p.Incidence13))
                .ForMember(i => i.Order13, me => me.MapFrom(p => p.Order13))
                .ForMember(i => i.Incidence14, me => me.MapFrom(p => p.Incidence14))
                .ForMember(i => i.Order14, me => me.MapFrom(p => p.Order14))
                .ForMember(i => i.Incidence5, me => me.MapFrom(p => p.Incidence15))
                .ForMember(i => i.Order15, me => me.MapFrom(p => p.Order15));
            CreateMap<PlaylistEntryModels.PlaylistValueEntry, Sma5h.Data.Ui.Param.Database.PrcUiBgmDatabaseModels.PrcBgmPlaylistEntry>()
                .ForMember(i => i.UiBgmId, me => me.MapFrom(p => p.UiBgmId))
                .ForMember(i => i.Incidence0, me => me.MapFrom(p => p.Incidence0))
                .ForMember(i => i.Order0, me => me.MapFrom(p => p.Order0))
                .ForMember(i => i.Incidence1, me => me.MapFrom(p => p.Incidence1))
                .ForMember(i => i.Order1, me => me.MapFrom(p => p.Order1))
                .ForMember(i => i.Incidence2, me => me.MapFrom(p => p.Incidence2))
                .ForMember(i => i.Order2, me => me.MapFrom(p => p.Order2))
                .ForMember(i => i.Incidence3, me => me.MapFrom(p => p.Incidence3))
                .ForMember(i => i.Order3, me => me.MapFrom(p => p.Order3))
                .ForMember(i => i.Incidence4, me => me.MapFrom(p => p.Incidence4))
                .ForMember(i => i.Order4, me => me.MapFrom(p => p.Order4))
                .ForMember(i => i.Incidence5, me => me.MapFrom(p => p.Incidence5))
                .ForMember(i => i.Order5, me => me.MapFrom(p => p.Order5))
                .ForMember(i => i.Incidence6, me => me.MapFrom(p => p.Incidence6))
                .ForMember(i => i.Order6, me => me.MapFrom(p => p.Order6))
                .ForMember(i => i.Incidence7, me => me.MapFrom(p => p.Incidence7))
                .ForMember(i => i.Order7, me => me.MapFrom(p => p.Order7))
                .ForMember(i => i.Incidence8, me => me.MapFrom(p => p.Incidence8))
                .ForMember(i => i.Order8, me => me.MapFrom(p => p.Order8))
                .ForMember(i => i.Incidence9, me => me.MapFrom(p => p.Incidence9))
                .ForMember(i => i.Order9, me => me.MapFrom(p => p.Order9))
                .ForMember(i => i.Incidence10, me => me.MapFrom(p => p.Incidence10))
                .ForMember(i => i.Order10, me => me.MapFrom(p => p.Order10))
                .ForMember(i => i.Incidence11, me => me.MapFrom(p => p.Incidence11))
                .ForMember(i => i.Order11, me => me.MapFrom(p => p.Order11))
                .ForMember(i => i.Incidence12, me => me.MapFrom(p => p.Incidence12))
                .ForMember(i => i.Order12, me => me.MapFrom(p => p.Order12))
                .ForMember(i => i.Incidence13, me => me.MapFrom(p => p.Incidence13))
                .ForMember(i => i.Order13, me => me.MapFrom(p => p.Order13))
                .ForMember(i => i.Incidence14, me => me.MapFrom(p => p.Incidence14))
                .ForMember(i => i.Order14, me => me.MapFrom(p => p.Order14))
                .ForMember(i => i.Incidence5, me => me.MapFrom(p => p.Incidence15))
                .ForMember(i => i.Order15, me => me.MapFrom(p => p.Order15));

            CreateMap<Sma5h.Data.Ui.Param.Database.PrcUiStageDatabaseModels.StageDbRootEntry, StageEntry>()
                .ForMember(i => i.UiStageId, me => me.MapFrom(p => p.UiStageId))
                .ForMember(i => i.NameId, me => me.MapFrom(p => p.NameId))
                .ForMember(i => i.SaveNo, me => me.MapFrom(p => p.SaveNo))
                .ForMember(i => i.UiSeriesId, me => me.MapFrom(p => p.UiSeriesId))
                .ForMember(i => i.CanSelect, me => me.MapFrom(p => p.CanSelect))
                .ForMember(i => i.DispOrder, me => me.MapFrom(p => p.DispOrder))
                .ForMember(i => i.StagePlaceId, me => me.MapFrom(p => p.StagePlaceId))
                .ForMember(i => i.SecretStagePlaceId, me => me.MapFrom(p => p.SecretStagePlaceId))
                .ForMember(i => i.CanDemo, me => me.MapFrom(p => p.CanDemo))
                .ForMember(i => i.Unk1, me => me.MapFrom(p => p.Unk1))
                .ForMember(i => i.IsUsableFlag, me => me.MapFrom(p => p.IsUsableFlag))
                .ForMember(i => i.IsUsableAmiibo, me => me.MapFrom(p => p.IsUsableAmiibo))
                .ForMember(i => i.SecretCommandId, me => me.MapFrom(p => p.SecretCommandId))
                .ForMember(i => i.SecretCommandIdJoycon, me => me.MapFrom(p => p.SecretCommandIdJoycon))
                .ForMember(i => i.BgmSetId, me => me.MapFrom(p => p.BgmSetId))
                .ForMember(i => i.BgmSettingNo, me => me.MapFrom(p => p.BgmSettingNo))
                .ForMember(i => i.BgmSelector, me => me.MapFrom(p => p.BgmSelector))
                .ForMember(i => i.IsDlc, me => me.MapFrom(p => p.IsDlc))
                .ForMember(i => i.IsPatch, me => me.MapFrom(p => p.IsPatch))
                .ForMember(i => i.DlcCharaId, me => me.MapFrom(p => p.DlcCharaId));
            CreateMap<StageEntry, Sma5h.Data.Ui.Param.Database.PrcUiStageDatabaseModels.StageDbRootEntry>()
                .ForMember(i => i.UiStageId, me => me.MapFrom(p => p.UiStageId))
                .ForMember(i => i.NameId, me => me.MapFrom(p => p.NameId))
                .ForMember(i => i.SaveNo, me => me.MapFrom(p => p.SaveNo))
                .ForMember(i => i.UiSeriesId, me => me.MapFrom(p => p.UiSeriesId))
                .ForMember(i => i.CanSelect, me => me.MapFrom(p => p.CanSelect))
                .ForMember(i => i.DispOrder, me => me.MapFrom(p => p.DispOrder))
                .ForMember(i => i.StagePlaceId, me => me.MapFrom(p => p.StagePlaceId))
                .ForMember(i => i.SecretStagePlaceId, me => me.MapFrom(p => p.SecretStagePlaceId))
                .ForMember(i => i.CanDemo, me => me.MapFrom(p => p.CanDemo))
                .ForMember(i => i.Unk1, me => me.MapFrom(p => p.Unk1))
                .ForMember(i => i.IsUsableFlag, me => me.MapFrom(p => p.IsUsableFlag))
                .ForMember(i => i.IsUsableAmiibo, me => me.MapFrom(p => p.IsUsableAmiibo))
                .ForMember(i => i.SecretCommandId, me => me.MapFrom(p => p.SecretCommandId))
                .ForMember(i => i.SecretCommandIdJoycon, me => me.MapFrom(p => p.SecretCommandIdJoycon))
                .ForMember(i => i.BgmSetId, me => me.MapFrom(p => p.BgmSetId))
                .ForMember(i => i.BgmSettingNo, me => me.MapFrom(p => p.BgmSettingNo))
                .ForMember(i => i.BgmSelector, me => me.MapFrom(p => p.BgmSelector))
                .ForMember(i => i.IsDlc, me => me.MapFrom(p => p.IsDlc))
                .ForMember(i => i.IsPatch, me => me.MapFrom(p => p.IsPatch))
                .ForMember(i => i.DlcCharaId, me => me.MapFrom(p => p.DlcCharaId));

            CreateMap<Sma5h.Data.Ui.Param.Database.PrcUiSeriesDatabaseModels.PrcSeriesDbRootEntry, SeriesEntry>()
                .ForMember(i => i.UiSeriesId, me => me.MapFrom(p => p.UiSeriesId))
                .ForMember(i => i.NameId, me => me.MapFrom(p => p.NameId))
                .ForMember(i => i.DispOrder, me => me.MapFrom(p => p.DispOrder))
                .ForMember(i => i.DispOrderSound, me => me.MapFrom(p => p.DispOrderSound))
                .ForMember(i => i.SaveNo, me => me.MapFrom(p => p.SaveNo))
                .ForMember(i => i.Unk1, me => me.MapFrom(p => p.Unk1))
                .ForMember(i => i.IsDlc, me => me.MapFrom(p => p.IsDlc))
                .ForMember(i => i.IsPatch, me => me.MapFrom(p => p.IsPatch))
                .ForMember(i => i.DlcCharaId, me => me.MapFrom(p => p.DlcCharaId))
                .ForMember(i => i.IsUseAmiiboBg, me => me.MapFrom(p => p.IsUseAmiiboBg));
            CreateMap<SeriesEntry, Sma5h.Data.Ui.Param.Database.PrcUiSeriesDatabaseModels.PrcSeriesDbRootEntry>()
                .ForMember(i => i.UiSeriesId, me => me.MapFrom(p => p.UiSeriesId))
                .ForMember(i => i.NameId, me => me.MapFrom(p => p.NameId))
                .ForMember(i => i.DispOrder, me => me.MapFrom(p => p.DispOrder))
                .ForMember(i => i.DispOrderSound, me => me.MapFrom(p => p.DispOrderSound))
                .ForMember(i => i.SaveNo, me => me.MapFrom(p => p.SaveNo))
                .ForMember(i => i.Unk1, me => me.MapFrom(p => p.Unk1))
                .ForMember(i => i.IsDlc, me => me.MapFrom(p => p.IsDlc))
                .ForMember(i => i.IsPatch, me => me.MapFrom(p => p.IsPatch))
                .ForMember(i => i.DlcCharaId, me => me.MapFrom(p => p.DlcCharaId))
                .ForMember(i => i.IsUseAmiiboBg, me => me.MapFrom(p => p.IsUseAmiiboBg));
        }
    }
}
