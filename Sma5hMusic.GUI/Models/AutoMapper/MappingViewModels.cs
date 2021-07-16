using AutoMapper;
using Sma5h.Mods.Music;
using Sma5h.Mods.Music.Models;
using System;

namespace Sma5hMusic.GUI.Mods.Music.Models.AutoMapper
{
    public class MappingViewModels : Profile
    {
        public MappingViewModels()
        {
            CreateMap<GUI.ViewModels.GlobalConfigurationViewModel, ApplicationSettings>()
                .ForMember(i => i.GameResourcesPath, me => me.MapFrom(p => p.GameResourcesPath))
                .ForMember(i => i.LogPath, me => me.MapFrom(p => p.LogPath))
                .ForMember(i => i.OutputPath, me => me.MapFrom(p => p.OutputPath))
                .ForMember(i => i.ResourcesPath, me => me.MapFrom(p => p.ResourcesPath))
                .ForMember(i => i.SkipOutputPathCleanupConfirmation, me => me.MapFrom(p => p.SkipOutputPathCleanupConfirmation))
                .ForMember(i => i.Sma5hMusic, me => me.MapFrom(p => new Sma5hMusicOptions.Sma5hMusicOptionsSection()
                {
                    AudioConversionFormat = p.AudioConversionFormat,
                    AudioConversionFormatFallBack = p.AudioConversionFormatFallBack,
                    CachePath = p.CachePath,
                    DefaultLocale = p.DefaultSma5hMusicLocale,
                    EnableAudioCaching = p.EnableAudioCaching,
                    ModPath = p.ModPath
                }))
                .ForMember(i => i.Sma5hMusicGUI, me => me.MapFrom(p => new ApplicationSettings.Sma5hMusicGuiOptionsSection()
                {
                    UIScale = Enum.Parse<Helpers.StylesHelper.UIScale>(p.UIScale, true),
                    UITheme = Enum.Parse<Helpers.StylesHelper.UITheme>(p.UITheme, true),
                    Advanced = p.Advanced,
                    DefaultGUILocale = p.DefaultGUILocale,
                    DefaultMSBTLocale = p.DefaultMSBTLocale,
                    CopyToEmptyLocales = p.CopyToEmptyLocales,
                    PlaylistIncidenceDefault = p.PlaylistIncidenceDefault,
                    SkipWarningGameVersion = p.SkipWarningGameVersion,
                    InGameVolume = p.InGameVolume
                }))
                .ForMember(i => i.Sma5hMusicOverride, me => me.MapFrom(p => new Sma5hMusicOverrideOptions.Sma5hMusicOverrideOptionsSection()
                {
                    ModPath = p.ModOverridePath
                }))
                .ForMember(i => i.TempPath, me => me.MapFrom(p => p.TempPath))
                .ForMember(i => i.ToolsPath, me => me.MapFrom(p => p.ToolsPath));
            CreateMap<ApplicationSettings, GUI.ViewModels.GlobalConfigurationViewModel>()
                .ForMember(i => i.GameResourcesPath, me => me.MapFrom(p => p.GameResourcesPath))
                .ForMember(i => i.LogPath, me => me.MapFrom(p => p.LogPath))
                .ForMember(i => i.OutputPath, me => me.MapFrom(p => p.OutputPath))
                .ForMember(i => i.ResourcesPath, me => me.MapFrom(p => p.ResourcesPath))
                .ForMember(i => i.SkipOutputPathCleanupConfirmation, me => me.MapFrom(p => p.SkipOutputPathCleanupConfirmation))
                .ForMember(i => i.AudioConversionFormat, me => me.MapFrom(p => p.Sma5hMusic.AudioConversionFormat))
                .ForMember(i => i.AudioConversionFormatFallBack, me => me.MapFrom(p => p.Sma5hMusic.AudioConversionFormatFallBack))
                .ForMember(i => i.CachePath, me => me.MapFrom(p => p.Sma5hMusic.CachePath))
                .ForMember(i => i.DefaultSma5hMusicLocale, me => me.MapFrom(p => p.Sma5hMusic.DefaultLocale))
                .ForMember(i => i.EnableAudioCaching, me => me.MapFrom(p => p.Sma5hMusic.EnableAudioCaching))
                .ForMember(i => i.ModPath, me => me.MapFrom(p => p.Sma5hMusic.ModPath))
                .ForMember(i => i.UIScale, me => me.MapFrom(p => p.Sma5hMusicGUI.UIScale))
                .ForMember(i => i.UITheme, me => me.MapFrom(p => p.Sma5hMusicGUI.UITheme))
                .ForMember(i => i.Advanced, me => me.MapFrom(p => p.Sma5hMusicGUI.Advanced))
                .ForMember(i => i.DefaultGUILocale, me => me.MapFrom(p => p.Sma5hMusicGUI.DefaultGUILocale))
                .ForMember(i => i.DefaultMSBTLocale, me => me.MapFrom(p => p.Sma5hMusicGUI.DefaultMSBTLocale))
                .ForMember(i => i.CopyToEmptyLocales, me => me.MapFrom(p => p.Sma5hMusicGUI.CopyToEmptyLocales))
                .ForMember(i => i.PlaylistIncidenceDefault, me => me.MapFrom(p => p.Sma5hMusicGUI.PlaylistIncidenceDefault))
                .ForMember(i => i.SkipWarningGameVersion, me => me.MapFrom(p => p.Sma5hMusicGUI.SkipWarningGameVersion))
                .ForMember(i => i.InGameVolume, me => me.MapFrom(p => p.Sma5hMusicGUI.InGameVolume))
                .ForMember(i => i.ModOverridePath, me => me.MapFrom(p => p.Sma5hMusicOverride.ModPath))
                .ForMember(i => i.TempPath, me => me.MapFrom(p => p.TempPath))
                .ForMember(i => i.ToolsPath, me => me.MapFrom(p => p.ToolsPath));

            CreateMap<GUI.ViewModels.StageEntryViewModel, StageEntry>()
                .ForMember(i => i.UiStageId, me => me.Ignore())
                .ForMember(i => i.NameId, me => me.Ignore())
                .ForMember(i => i.SaveNo, me => me.Ignore())
                .ForMember(i => i.UiSeriesId, me => me.Ignore())
                .ForMember(i => i.CanSelect, me => me.Ignore())
                .ForMember(i => i.DispOrder, me => me.Ignore())
                .ForMember(i => i.StagePlaceId, me => me.Ignore())
                .ForMember(i => i.SecretStagePlaceId, me => me.Ignore())
                .ForMember(i => i.CanDemo, me => me.Ignore())
                .ForMember(i => i.Unk1, me => me.Ignore())
                .ForMember(i => i.IsUsableFlag, me => me.Ignore())
                .ForMember(i => i.IsUsableAmiibo, me => me.Ignore())
                .ForMember(i => i.SecretCommandId, me => me.Ignore())
                .ForMember(i => i.SecretCommandIdJoycon, me => me.Ignore())
                .ForMember(i => i.BgmSetId, me => me.MapFrom(p => p.PlaylistId))
                .ForMember(i => i.BgmSettingNo, me => me.MapFrom(p => p.OrderId))
                .ForMember(i => i.BgmSelector, me => me.MapFrom(p => p.BgmSelector))
                .ForMember(i => i.IsDlc, me => me.Ignore())
                .ForMember(i => i.IsPatch, me => me.Ignore())
                .ForMember(i => i.DlcCharaId, me => me.Ignore());
            CreateMap<StageEntry, GUI.ViewModels.StageEntryViewModel>()
                .ForMember(i => i.DispOrder, me => me.MapFrom(p => p.DispOrder))
                .ForMember(i => i.OrderId, me => me.MapFrom(p => p.BgmSettingNo))
                .ForMember(i => i.BgmSelector, me => me.MapFrom(p => p.BgmSelector))
                .ForMember(i => i.PlaylistId, me => me.MapFrom(p => p.BgmSetId));

            CreateMap<GUI.ViewModels.GameTitleEntryViewModel, GameTitleEntry>()
                .ForMember(i => i.Source, me => me.Ignore())
                .ForMember(i => i.MSBTTitle, me => me.MapFrom(p => p.MSBTTitle))
                .ForMember(i => i.NameId, me => me.MapFrom(p => p.NameId))
                .ForMember(i => i.Release, me => me.MapFrom(p => p.Release))
                .ForMember(i => i.UiGameTitleId, me => me.Ignore())
                .ForMember(i => i.UiSeriesId, me => me.MapFrom(p => p.UiSeriesId))
                .ForMember(i => i.Unk1, me => me.MapFrom(p => p.Unk1));
            CreateMap<GameTitleEntry, GUI.ViewModels.GameTitleEntryViewModel>()
                .ForMember(i => i.Source, me => me.MapFrom(p => p.Source))
                .ForMember(i => i.MSBTTitle, me => me.MapFrom(p => p.MSBTTitle))
                .ForMember(i => i.NameId, me => me.MapFrom(p => p.NameId))
                .ForMember(i => i.Release, me => me.MapFrom(p => p.Release))
                .ForMember(i => i.UiGameTitleId, me => me.Ignore())
                .ForMember(i => i.UiSeriesId, me => me.MapFrom(p => p.UiSeriesId))
                .ForMember(i => i.Unk1, me => me.MapFrom(p => p.Unk1));

            CreateMap<ViewModels.BgmPropertyEntryViewModel, BgmPropertyEntry>()
                .ForMember(i => i.LoopEndMs, me => me.MapFrom(p => p.LoopEndMs))
                .ForMember(i => i.LoopEndSample, me => me.MapFrom(p => p.LoopEndSample))
                .ForMember(i => i.LoopStartMs, me => me.MapFrom(p => p.LoopStartMs))
                .ForMember(i => i.LoopStartSample, me => me.MapFrom(p => p.LoopStartSample))
                .ForMember(i => i.TotalSamples, me => me.MapFrom(p => p.TotalSamples))
                .ForMember(i => i.TotalTimeMs, me => me.MapFrom(p => p.TotalTimeMs))
                .ForMember(i => i.AudioVolume, me => me.MapFrom(p => p.AudioVolume))
                .ForMember(i => i.Filename, me => me.MapFrom(p => p.Filename))
                .ForMember(i => i.NameId, me => me.Ignore());
            CreateMap<BgmPropertyEntry, ViewModels.BgmPropertyEntryViewModel>()
                .ForMember(i => i.LoopEndMs, me => me.MapFrom(p => p.LoopEndMs))
                .ForMember(i => i.LoopEndSample, me => me.MapFrom(p => p.LoopEndSample))
                .ForMember(i => i.LoopStartMs, me => me.MapFrom(p => p.LoopStartMs))
                .ForMember(i => i.LoopStartSample, me => me.MapFrom(p => p.LoopStartSample))
                .ForMember(i => i.TotalSamples, me => me.MapFrom(p => p.TotalSamples))
                .ForMember(i => i.TotalTimeMs, me => me.MapFrom(p => p.TotalTimeMs))
                .ForMember(i => i.Filename, me => me.MapFrom(p => p.Filename))
                .ForMember(i => i.AudioVolume, me => me.MapFrom(p => p.AudioVolume))
                .ForMember(i => i.NameId, me => me.MapFrom(p => p.NameId));

            CreateMap<ViewModels.BgmDbRootEntryViewModel, BgmDbRootEntry>()
                .ForMember(i => i.UiBgmId, me => me.Ignore())
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
                .ForMember(i => i.UiGameTitleId, me => me.MapFrom(p => p.UiGameTitleId))
                .ForMember(i => i.UiGameTitleId1, me => me.MapFrom(p => p.UiGameTitleId1))
                .ForMember(i => i.UiGameTitleId2, me => me.MapFrom(p => p.UiGameTitleId2))
                .ForMember(i => i.UiGameTitleId3, me => me.MapFrom(p => p.UiGameTitleId3))
                .ForMember(i => i.UiGameTitleId4, me => me.MapFrom(p => p.UiGameTitleId4))
                .ForMember(i => i.DlcUiCharaId, me => me.MapFrom(p => p.DlcUiCharaId))
                .ForMember(i => i.DlcMiiHatMotifId, me => me.MapFrom(p => p.DlcMiiHatMotifId))
                .ForMember(i => i.DlcMiiBodyMotifId, me => me.MapFrom(p => p.DlcMiiBodyMotifId))
                .ForMember(i => i.Title, me => me.MapFrom(p => p.MSBTTitle))
                .ForMember(i => i.Author, me => me.MapFrom(p => p.MSBTAuthor))
                .ForMember(i => i.Copyright, me => me.MapFrom(p => p.MSBTCopyright));
            CreateMap<BgmDbRootEntry, ViewModels.BgmDbRootEntryViewModel>()
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
                .ForMember(i => i.MSBTTitle, me => me.MapFrom(p => p.Title))
                .ForMember(i => i.MSBTAuthor, me => me.MapFrom(p => p.Author))
                .ForMember(i => i.MSBTCopyright, me => me.MapFrom(p => p.Copyright));

            CreateMap<ViewModels.BgmStreamSetEntryViewModel, BgmStreamSetEntry>()
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
            CreateMap<BgmStreamSetEntry, ViewModels.BgmStreamSetEntryViewModel>()
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

            CreateMap<ViewModels.BgmAssignedInfoEntryViewModel, BgmAssignedInfoEntry>()
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
            CreateMap<BgmAssignedInfoEntry, ViewModels.BgmAssignedInfoEntryViewModel>()
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

            CreateMap<ViewModels.BgmStreamPropertyEntryViewModel, BgmStreamPropertyEntry>()
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
            CreateMap<BgmStreamPropertyEntry, ViewModels.BgmStreamPropertyEntryViewModel>()
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

            CreateMap<AudioCuePoints, ViewModels.BgmPropertyEntryViewModel>()
               .ForMember(i => i.AudioVolume, me => me.Ignore())
               .ForMember(i => i.LoopEndMs, me => me.MapFrom(p => p.LoopEndMs))
               .ForMember(i => i.LoopEndSample, me => me.MapFrom(p => p.LoopEndSample))
               .ForMember(i => i.LoopStartMs, me => me.MapFrom(p => p.LoopStartMs))
               .ForMember(i => i.LoopStartSample, me => me.MapFrom(p => p.LoopStartSample))
               .ForMember(i => i.NameId, me => me.Ignore())
               .ForMember(i => i.TotalSamples, me => me.MapFrom(p => p.TotalSamples))
               .ForMember(i => i.TotalTimeMs, me => me.MapFrom(p => p.TotalTimeMs));
            CreateMap<ViewModels.BgmPropertyEntryViewModel, AudioCuePoints>()
               .ForMember(i => i.LoopEndMs, me => me.MapFrom(p => p.LoopEndMs))
               .ForMember(i => i.LoopEndSample, me => me.MapFrom(p => p.LoopEndSample))
               .ForMember(i => i.LoopStartMs, me => me.MapFrom(p => p.LoopStartMs))
               .ForMember(i => i.LoopStartSample, me => me.MapFrom(p => p.LoopStartSample))
               .ForMember(i => i.TotalSamples, me => me.MapFrom(p => p.TotalSamples))
               .ForMember(i => i.TotalTimeMs, me => me.MapFrom(p => p.TotalTimeMs));

            //CreateMap<ViewModels.GameTitleEntryViewModel, ViewModels.GameTitleEntryViewModel>();
            CreateMap<ViewModels.BgmDbRootEntryViewModel, ViewModels.BgmDbRootEntryViewModel>();
            CreateMap<ViewModels.BgmAssignedInfoEntryViewModel, ViewModels.BgmAssignedInfoEntryViewModel>();
            CreateMap<ViewModels.BgmStreamPropertyEntryViewModel, ViewModels.BgmStreamPropertyEntryViewModel>();
            CreateMap<ViewModels.BgmStreamSetEntryViewModel, ViewModels.BgmStreamSetEntryViewModel>();
            CreateMap<ViewModels.BgmPropertyEntryViewModel, ViewModels.BgmPropertyEntryViewModel>();

            CreateMap<BgmDbRootEntry, BgmDbRootEntry>();
            CreateMap<BgmAssignedInfoEntry, BgmAssignedInfoEntry>();
            CreateMap<BgmStreamPropertyEntry, BgmStreamPropertyEntry>();
            CreateMap<BgmStreamSetEntry, BgmStreamSetEntry>();
            CreateMap<BgmPropertyEntry, BgmPropertyEntry>();
        }
    }
}
