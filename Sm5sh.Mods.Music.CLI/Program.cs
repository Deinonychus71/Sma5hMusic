using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Sm5sh.Interfaces;
using Sm5sh.Mods.Music;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.ResourceProviders;
using Sm5sh.Mods.Music.Services;
using Sm5sh.ResourceProviders;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Sm5sh.CLI
{
    class Program
    {
        async static Task Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services, args);
            var serviceProvider = services.BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                Script entry = scope.ServiceProvider.GetService<Script>();
                await entry.Run();
            }

            await Task.Delay(1000);
            Console.WriteLine("The program has completed its task. Press enter to exit");
            Console.ReadLine();
        }

        private static void ConfigureServices(IServiceCollection services, string[] args)
        {
            var configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .AddCommandLine(args)
               .Build();

            var loggerFactory = LoggerFactory.Create(builder => builder
                .AddFilter<ConsoleLoggerProvider>((ll) => ll >= LogLevel.Information)
                .AddFile(Path.Combine(configuration.GetValue<string>("LogPath"), "log_{Date}.txt"), LogLevel.Debug, retainedFileCountLimit: 7)
                .AddSimpleConsole((c) => {
                    c.SingleLine = true;
                }));

            services.AddLogging();
            services.AddOptions();
            services.AddSingleton(configuration);
            services.AddSingleton(loggerFactory);

            //Sm5sh Core
            services.Configure<Sm5shOptions>(configuration);
            services.AddSingleton<IProcessService, ProcessService>();
            services.AddSingleton<IStateManager, StateManager>();
            services.AddSingleton<IResourceProvider, MsbtResourceProvider>();
            services.AddSingleton<IResourceProvider, PrcResourceProvider>();

            //MODS - TODO LOAD DYNAMICALLY?
            //Mod Music
            services.Configure<Sm5shMusicOptions>(configuration);
            services.AddSingleton<ISm5shMod, BgmMod>();
            services.AddSingleton<IResourceProvider, BgmPropertyProvider>();
            services.AddTransient<IAudioStateService, AudioStateService>();
            services.AddSingleton<IAudioMetadataService, VGAudioMetadataService>();
            services.AddSingleton<INus3AudioService, Nus3AudioService>();

            //CLI
            services.AddScoped<IWorkspaceManager, WorkspaceManager>();
            services.AddScoped<Script>();
            
            services.AddLogging();
        }
    }
}
