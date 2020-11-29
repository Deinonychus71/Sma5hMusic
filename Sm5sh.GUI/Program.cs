using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Splat.Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sm5sh.Interfaces;
using Sm5sh.Mods.StagePlaylist;
using System;
using System.IO;
using Sm5sh.GUI.ViewModels;
using VGMMusic;
using Sm5sh.GUI.Views;
using Sm5sh.GUI.Interfaces;
using Sm5sh.GUI.Dialogs;
using AutoMapper;
using Sm5sh.Mods.Music.Models.AutoMapper;

namespace Sm5sh.GUI
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
                .LogToTrace()
                .UseReactiveUI();
        }

        public static void ConfigureServices()
        {
            var services = new ServiceCollection();

            var configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
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

            //Add UI Services
            services.AddSingleton<IVGMMusicPlayer, VGMMusicPlayer>();
            services.AddSingleton<IFileDialog, FileDialog>();
            services.AddSingleton<IMessageDialog, MessageDialog>();
            services.AddSingleton<IBuildDialog, BuildDialog>();
            services.AddAutoMapper(typeof(MappingViewModels));

            //Add to Splat
            services.UseMicrosoftDependencyResolver();
        }
    }
}
