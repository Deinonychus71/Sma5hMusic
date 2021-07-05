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

            var configuration = new ConfigurationBuilder()
               .SetBasePath(GetBasePath())
               .AddInMemoryCollection(GetDefaultConfiguration())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .Build();
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
            return new Dictionary<string, string>()
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
                { "Sma5hMusicOverride:ModPath", $"Mods{Path.DirectorySeparatorChar}MusicOverride" },
            };
        }
    }
}
