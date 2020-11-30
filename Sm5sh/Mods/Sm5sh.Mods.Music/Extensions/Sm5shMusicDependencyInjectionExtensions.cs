using Microsoft.Extensions.Configuration;
using Sm5sh.Interfaces;
using Sm5sh.Mods.Music;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Services;
using Sm5sh.ResourceProviders;
using Sm5sh.Mods.Music.Models.AutoMapper;
using AutoMapper;
using VGMMusic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class Sm5shMusicDependencyInjectionExtensions
    {
        public static IServiceCollection AddSm5shMusic(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<Sm5shMusicOptions>(configuration);
            services.AddSingleton<ISm5shMod, Sm5shMusic>();
            services.AddSingleton<IResourceProvider, BgmPropertyProvider>();
            services.AddSingleton<IResourceProvider, MsbtResourceProvider>();
            services.AddSingleton<IResourceProvider, PrcResourceProvider>();
            services.AddSingleton<IMusicModManagerService, MusicModManagerService>();
            services.AddSingleton<IAudioStateService, AudioStateService>();
            //services.AddSingleton<IAudioMetadataService, VGAudioMetadataService>();
            services.AddSingleton<IAudioMetadataService, VGMStreamAudioMetadataService>();
            services.AddSingleton<IVGMMusicPlayer, VGMMusicPlayer>();
            services.AddSingleton<INus3AudioService, Nus3AudioService>();
            services.AddAutoMapper(typeof(MappingDb), typeof(MappingAdvancedConfig));
            return services;
        }

        public static IServiceCollection AddSm5shMusicOverride(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<Sm5shMusicOverrideOptions>(configuration);
            services.AddSingleton<ISm5shMusicOverride, Sm5shMusicOverride>();
            services.AddSingleton<ISm5shMod, Sm5shMusicOverride>((o) => o.GetRequiredService<ISm5shMusicOverride>() as Sm5shMusicOverride);
            services.AddSingleton<IResourceProvider, PrcResourceProvider>();
            services.AddSingleton<IAudioStateService, AudioStateService>();
            services.AddAutoMapper(typeof(MappingDb), typeof(MappingOverrideConfig));
            return services;
        }
    }
}
