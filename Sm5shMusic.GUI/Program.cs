using AutoMapper;
using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sm5shMusic.GUI.Dialogs;
using Sm5shMusic.GUI.Interfaces;
using Sm5shMusic.GUI.Mods.Music.Models.AutoMapper;
using Sm5shMusic.GUI.Services;
using Sm5shMusic.GUI.ViewModels;
using Sm5shMusic.GUI.Views;
using Splat.Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using VGMMusic;

namespace Sm5shMusic.GUI
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            ConfigureServices();
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                //.LogToTrace()
                .UseReactiveUI();
        }

        public static void ConfigureServices()
        {
            var services = new ServiceCollection();

            var configuration = new ConfigurationBuilder()
               .SetBasePath(GetBasePath())
               .AddInMemoryCollection(GetDefaultConfiguration())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               //.AddJsonFile(GetCurrentPathFile("user.json"), optional: false, reloadOnChange: true)
               .Build();

            var loggerFactory = LoggerFactory.Create(builder => builder
                .AddFile(Path.Combine(configuration.GetValue<string>("LogPath"), "log_{Date}.txt"), LogLevel.Debug, retainedFileCountLimit: 7)
                .AddProvider(new CustomConsoleLoggerProvider(LogLevel.Debug)));

            services.AddLogging();
            services.AddOptions();
            services.AddSingleton(configuration);
            services.AddSingleton(loggerFactory);

            //Sm5sh Core
            services.AddSm5shCore(configuration);
            services.AddSm5shMusic(configuration);
            services.AddSm5shMusicOverride(configuration);

            //Add UI ViewModels
            services.AddSingleton<IDialogWindow, MainWindow>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<ModPropertiesModalWindowViewModel>();
            services.AddSingleton<BgmPropertiesModalWindowViewModel>();
            services.AddSingleton<ContextMenuViewModel>();
            services.AddSingleton<BgmFiltersViewModel>();

            //Add UI Services
            services.AddSingleton<IVGMMusicPlayer, VGMMusicPlayer>();
            services.AddSingleton<IFileDialog, FileDialog>();
            services.AddSingleton<IMessageDialog, MessageDialog>();
            services.AddSingleton<IBuildDialog, BuildDialog>();
            services.AddSingleton<IViewModelManager, ViewModelManager>();
            services.AddSingleton<IGUIStateManager, GUIStateManager>();
            services.AddAutoMapper(typeof(MappingViewModels));

            //Add to Splat
            services.UseMicrosoftDependencyResolver();
        }

        private static string GetBasePath()
        {
            using var processModule = Process.GetCurrentProcess().MainModule;
            return Path.GetDirectoryName(processModule?.FileName);
        }

        private static Dictionary<string, string> GetDefaultConfiguration()
        {
            return new Dictionary<string, string>()
            {
                { "GameResourcesPath",$"Resources{Path.DirectorySeparatorChar}Game" },
                { "ResourcesPath", "Resources" },
                { "OutputPath", "ArcOutput" },
                { "ToolsPath", "Tools" },
                { "TempPath", "Temp" },
                { "LogPath", "Log" },
                { "SkipOutputPathCleanupConfirmation", "false" },
                { "Sm5shMusic:ModPath", $"Mods{Path.DirectorySeparatorChar}MusicMods" },
                { "Sm5shMusic:CachePath", "Cache" },
                { "Sm5shMusic:EnableAudioCaching", "false" },
                { "Sm5shMusic:AudioConversionFormat", "lopus" },
                { "Sm5shMusic:AudioConversionFormatFallBack", "idsp" },
                { "Sm5shMusic:DefaultLocale", "en_us" },
                { "Sm5shMusicOverride:ModPath", $"Mods{Path.DirectorySeparatorChar}MusicOverride" },
            };
        }
    }
}
