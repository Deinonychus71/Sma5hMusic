using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sma5h.Mods.Music;
using Sma5hMusic.GUI.Dialogs;
using Sma5hMusic.GUI.Interfaces;
using Sma5hMusic.GUI.Mods.Music.Models.AutoMapper;
using Sma5hMusic.GUI.Services;
using Sma5hMusic.GUI.ViewModels;
using Sma5hMusic.GUI.Views;
using Splat.Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using VGMMusic;

namespace Sma5hMusic.GUI
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

        public static IConfiguration Configuration { get; private set; }

        public static void ConfigureServices()
        {
            var services = new ServiceCollection();

            var configurationBuilder = new ConfigurationBuilder()
               .SetBasePath(GetBasePath())
               .AddInMemoryCollection(GetDefaultConfiguration())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            if (!configurationBuilder.Build().GetSection("Sma5hMusic:PlaylistMapping:AutoMapping").Exists())
                configurationBuilder.AddInMemoryCollection(GetPlaylistMappingConfig());

            var configuration = configurationBuilder.Build();
            Configuration = configuration;

            var loggerFactory = LoggerFactory.Create(builder => builder
                .SetMinimumLevel(LogLevel.Debug)
                .AddFile(configuration.GetSection("Logging"), c =>
                {
                    c.FormatLogFileName = (o) => $"{Path.ChangeExtension(o, string.Empty)}{DateTime.Today:yyyyMMdd}{Path.GetExtension(o)}";
                })
                .AddProvider(new CustomConsoleLoggerProvider(LogLevel.Information)));

            services.AddLogging();
            services.AddOptions();
            services.AddSingleton(configuration);
            services.AddSingleton(loggerFactory);

            //Sma5h Core
            services.AddSma5hCore(configuration);
            services.AddSma5hMusic(configuration);
            services.AddSma5hMusicOverride(configuration);

            //Add UI ViewModels
            services.AddSingleton<IDialogWindow, MainWindow>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<ModPropertiesModalWindowViewModel>();
            services.AddSingleton<BgmPropertiesModalWindowViewModel>();
            services.AddSingleton<ContextMenuViewModel>();
            services.AddSingleton<BgmFiltersViewModel>();

            //Add UI Services
            services.Configure<ApplicationSettings>(configuration);
            services.AddSingleton<IDevToolsService, DevToolsService>();
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
            var defaultConfig = new Dictionary<string, string>()
            {
                { "Logging:IncludeScopes", "False" },
                { "Logging:LogLevel:Default", "Debug" },
                { "Logging:LogLevel:System", "Information" },
                { "Logging:LogLevel:Microsoft", "Error" },
                { "Logging:File:Path", $"Log{Path.DirectorySeparatorChar}log.txt" },
                { "Logging:File:Append", "True" },
                { "Logging:File:FileSizeLimitBytes", "10485760" },
                { "Logging:File:MaxRollingFiles", "10" },
                { "GameResourcesPath",$"Resources{Path.DirectorySeparatorChar}Game" },
                { "ResourcesPath", "Resources" },
                { "OutputPath", "ArcOutput" },
                { "BackupPath", "Backup" },
                { "ToolsPath", "Tools" },
                { "TempPath", "Temp" },
                { "LogPath", "Log" },
                { "SkipOutputPathCleanupConfirmation", "false" },
                { "Sma5hMusic:ModPath", $"Mods{Path.DirectorySeparatorChar}MusicMods" },
                { "Sma5hMusic:CachePath", "Cache" },
                { "Sma5hMusic:EnableAudioCaching", "false" },
                { "Sma5hMusic:PlaylistMapping:GenerationMode", "OnlyMissingSongs" },
                { "Sma5hMusic:PlaylistMapping:AutoMappingIncidence", "0" },
                { "Sma5hMusic:AudioConversionFormat", "lopus" },
                { "Sma5hMusic:AudioConversionFormatFallBack", "idsp" },
                { "Sma5hMusic:DefaultLocale", "us_en" }, //Delete? This should now be handled from GUI with DefaultCopyLocale
                { "Sma5hMusicGUI:Advanced", "false" },
                { "Sma5hMusicGUI:UIScale", "Small" },
                { "Sma5hMusicGUI:UITheme", "Dark" },
                { "Sma5hMusicGUI:DefaultGUILocale", "us_en" },
                { "Sma5hMusicGUI:CopyToEmptyLocales", "true" },
                { "Sma5hMusicGUI:DefaultMSBTLocale", "us_en" },
                { "Sma5hMusicGUI:PlaylistIncidenceDefault", "0" },
                { "Sma5hMusicGUI:SkipWarningGameVersion", "false" },
                { "Sma5hMusicGUI:InGameVolume", "false" },
                { "Sma5hMusicGUI:HideIndexColumn", "false" },
                { "Sma5hMusicGUI:HideSeriesColumn", "false" },
                { "Sma5hMusicGUI:HideRecordColumn", "false" },
                { "Sma5hMusicGUI:HideModColumn", "false" },
                { "Sma5hMusicOverride:ModPath", $"Mods{Path.DirectorySeparatorChar}MusicOverride" },
            };

            return defaultConfig;
        }

        private static Dictionary<string, string> GetPlaylistMappingConfig()
        {
            var playlistMapping = new Dictionary<string, string[]>()
            {
                { "bgmsmashbtl", new string[] { "ui_series_smashbros" } },
                { "bgmmario", new string[] { "ui_series_mario" } },
                { "bgmmkart", new string[] { "ui_series_mariokart" } },
                { "bgmdk", new string[] { "ui_series_donkeykong" } },
                { "bgmzelda", new string[] { "ui_series_zelda" } },
                { "bgmmetroid", new string[] { "ui_series_metroid" } },
                { "bgmyoshi", new string[] { "ui_series_yoshi" } },
                { "bgmkirby", new string[] { "ui_series_kirby" } },
                { "bgmfox", new string[] { "ui_series_starfox" } },
                { "bgmpokemon", new string[] { "ui_series_pokemon" } },
                { "bgmfzero", new string[] { "ui_series_fzero" } },
                { "bgmmother", new string[] { "ui_series_mother", "ui_series_wreckingcrew", "ui_series_famicomrobot" } },
                { "bgmfe", new string[] { "ui_series_fireemblem" } },
                { "bgmmaster", new string[] { "ui_gametitle_fire_emblem_three_houses_en", "ui_gametitle_fire_emblem_three_houses_jp", "ui_series_fireemblem" } },
                { "bgmgamewatch", new string[] { "ui_series_gamewatch" } },
                { "bgmicaros", new string[] { "ui_series_palutena" } },
                { "bgmwario", new string[] { "ui_series_wario" } },
                { "bgmpikmin", new string[] { "ui_series_pikmin" } },
                { "bgmanimal", new string[] { "ui_series_doubutsu" } },
                { "bgmwiifit", new string[] { "ui_series_wiifit" } },
                { "bgmpunchout", new string[] { "ui_series_punchout" } },
                { "bgmxenoblade", new string[] { "ui_series_xenoblade" } },
                { "bgmelement", new string[] { "ui_gametitle_xenoblade2", "ui_series_xenoblade" } },
                { "bgmmetalgear", new string[] { "ui_series_metalgear" } },
                { "bgmsonic", new string[] { "ui_series_sonic" } },
                { "bgmrockman", new string[] { "ui_series_rockman" } },
                { "bgmpacman", new string[] { "ui_series_pacman" } },
                { "bgmsf", new string[] { "ui_series_streetfighter" } },
                { "bgmff", new string[] { "ui_series_finalfantasy" } },
                { "bgmedge", new string[] { "ui_series_finalfantasy" } },
                { "bgmbeyo", new string[] { "ui_series_bayonetta" } },
                { "bgmspla", new string[] { "ui_series_splatoon" } },
                { "bgmdracula", new string[] { "ui_series_castlevania" } },
                { "bgmjack", new string[] { "ui_series_persona" } },
                { "bgmbrave", new string[] { "ui_series_dragonquest" } },
                { "bgmbuddy", new string[] { "ui_series_banjokazooie" } },
                { "bgmdolly", new string[] { "ui_series_fatalfury" } },
                { "bgmtantan", new string[] { "ui_series_arms" } },
                { "bgmpickel", new string[] { "ui_series_minecraft" } },
                { "bgmdemon", new string[] { "ui_series_tekken" } },
                { "bgmother", new string[] { "ui_series_etc" } }
            };

            var output = new Dictionary<string, string>();
            foreach (var playlistConfig in playlistMapping)
                output.Add($"Sma5hMusic:PlaylistMapping:AutoMapping:{playlistConfig.Key}", string.Join(",", playlistConfig.Value));

            return output;
        }
    }
}