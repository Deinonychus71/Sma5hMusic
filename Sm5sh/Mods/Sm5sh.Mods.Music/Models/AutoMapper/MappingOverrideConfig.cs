﻿using AutoMapper;

namespace Sm5sh.Mods.Music.Models.AutoMapper
{
    public class MappingOverrideConfig : Profile
    {
        public MappingOverrideConfig()
        {
            CreateMap<BgmEntryModels.BgmPlaylistEntry, MusicOverride.MusicOverrideConfigModels.BgmPlaylistConfig>()
                .ForMember(i => i.UiBgmId, me => me.MapFrom(p => p.UiBgmId))
                .ForMember(i => i.Incidence0, me => me.MapFrom(p => p.Incidence0))
                .ForMember(i => i.Incidence1, me => me.MapFrom(p => p.Incidence1))
                .ForMember(i => i.Incidence2, me => me.MapFrom(p => p.Incidence2))
                .ForMember(i => i.Incidence3, me => me.MapFrom(p => p.Incidence3))
                .ForMember(i => i.Incidence4, me => me.MapFrom(p => p.Incidence4))
                .ForMember(i => i.Incidence5, me => me.MapFrom(p => p.Incidence5))
                .ForMember(i => i.Incidence6, me => me.MapFrom(p => p.Incidence6))
                .ForMember(i => i.Incidence7, me => me.MapFrom(p => p.Incidence7))
                .ForMember(i => i.Incidence8, me => me.MapFrom(p => p.Incidence8))
                .ForMember(i => i.Incidence9, me => me.MapFrom(p => p.Incidence9))
                .ForMember(i => i.Incidence10, me => me.MapFrom(p => p.Incidence10))
                .ForMember(i => i.Incidence11, me => me.MapFrom(p => p.Incidence11))
                .ForMember(i => i.Incidence12, me => me.MapFrom(p => p.Incidence12))
                .ForMember(i => i.Incidence13, me => me.MapFrom(p => p.Incidence13))
                .ForMember(i => i.Incidence14, me => me.MapFrom(p => p.Incidence14))
                .ForMember(i => i.Incidence15, me => me.MapFrom(p => p.Incidence15))
                .ForMember(i => i.Order0, me => me.MapFrom(p => p.Order0))
                .ForMember(i => i.Order1, me => me.MapFrom(p => p.Order1))
                .ForMember(i => i.Order2, me => me.MapFrom(p => p.Order2))
                .ForMember(i => i.Order3, me => me.MapFrom(p => p.Order3))
                .ForMember(i => i.Order4, me => me.MapFrom(p => p.Order4))
                .ForMember(i => i.Order5, me => me.MapFrom(p => p.Order5))
                .ForMember(i => i.Order6, me => me.MapFrom(p => p.Order6))
                .ForMember(i => i.Order7, me => me.MapFrom(p => p.Order7))
                .ForMember(i => i.Order8, me => me.MapFrom(p => p.Order8))
                .ForMember(i => i.Order9, me => me.MapFrom(p => p.Order9))
                .ForMember(i => i.Order10, me => me.MapFrom(p => p.Order10))
                .ForMember(i => i.Order11, me => me.MapFrom(p => p.Order11))
                .ForMember(i => i.Order12, me => me.MapFrom(p => p.Order12))
                .ForMember(i => i.Order13, me => me.MapFrom(p => p.Order13))
                .ForMember(i => i.Order14, me => me.MapFrom(p => p.Order14))
                .ForMember(i => i.Order15, me => me.MapFrom(p => p.Order15));
            CreateMap<MusicOverride.MusicOverrideConfigModels.BgmPlaylistConfig, BgmEntryModels.BgmPlaylistEntry>()
                .ForMember(i => i.Parent, me => me.Ignore())
                .ForMember(i => i.UiBgmId, me => me.Ignore())
                .ForMember(i => i.Incidence0, me => me.MapFrom(p => p.Incidence0))
                .ForMember(i => i.Incidence1, me => me.MapFrom(p => p.Incidence1))
                .ForMember(i => i.Incidence2, me => me.MapFrom(p => p.Incidence2))
                .ForMember(i => i.Incidence3, me => me.MapFrom(p => p.Incidence3))
                .ForMember(i => i.Incidence4, me => me.MapFrom(p => p.Incidence4))
                .ForMember(i => i.Incidence5, me => me.MapFrom(p => p.Incidence5))
                .ForMember(i => i.Incidence6, me => me.MapFrom(p => p.Incidence6))
                .ForMember(i => i.Incidence7, me => me.MapFrom(p => p.Incidence7))
                .ForMember(i => i.Incidence8, me => me.MapFrom(p => p.Incidence8))
                .ForMember(i => i.Incidence9, me => me.MapFrom(p => p.Incidence9))
                .ForMember(i => i.Incidence10, me => me.MapFrom(p => p.Incidence10))
                .ForMember(i => i.Incidence11, me => me.MapFrom(p => p.Incidence11))
                .ForMember(i => i.Incidence12, me => me.MapFrom(p => p.Incidence12))
                .ForMember(i => i.Incidence13, me => me.MapFrom(p => p.Incidence13))
                .ForMember(i => i.Incidence14, me => me.MapFrom(p => p.Incidence14))
                .ForMember(i => i.Incidence15, me => me.MapFrom(p => p.Incidence15))
                .ForMember(i => i.Order0, me => me.MapFrom(p => p.Order0))
                .ForMember(i => i.Order1, me => me.MapFrom(p => p.Order1))
                .ForMember(i => i.Order2, me => me.MapFrom(p => p.Order2))
                .ForMember(i => i.Order3, me => me.MapFrom(p => p.Order3))
                .ForMember(i => i.Order4, me => me.MapFrom(p => p.Order4))
                .ForMember(i => i.Order5, me => me.MapFrom(p => p.Order5))
                .ForMember(i => i.Order6, me => me.MapFrom(p => p.Order6))
                .ForMember(i => i.Order7, me => me.MapFrom(p => p.Order7))
                .ForMember(i => i.Order8, me => me.MapFrom(p => p.Order8))
                .ForMember(i => i.Order9, me => me.MapFrom(p => p.Order9))
                .ForMember(i => i.Order10, me => me.MapFrom(p => p.Order10))
                .ForMember(i => i.Order11, me => me.MapFrom(p => p.Order11))
                .ForMember(i => i.Order12, me => me.MapFrom(p => p.Order12))
                .ForMember(i => i.Order13, me => me.MapFrom(p => p.Order13))
                .ForMember(i => i.Order14, me => me.MapFrom(p => p.Order14))
                .ForMember(i => i.Order15, me => me.MapFrom(p => p.Order15));
        }
    }
}