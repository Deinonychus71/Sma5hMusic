using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Sm5shMusic.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;
using Sm5shMusic.Helpers;

namespace Sm5shMusic
{
    public class Script
    {
        private readonly Settings _settings;
        private readonly IResourceService _resourceService;
        private readonly IWorkspaceManager _workspace;
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IArcModGeneratorService _arcModGeneratorService;

        public Script(IOptions<Settings> settings, IWorkspaceManager workspace, IResourceService resourceService,
            IArcModGeneratorService arcModGeneratorService, IServiceProvider serviceProvider, ILogger<Script> logger)
        {
            _settings = settings.Value;
            _resourceService = resourceService;
            _workspace = workspace;
            _serviceProvider = serviceProvider;
            _arcModGeneratorService = arcModGeneratorService;
            _logger = logger;
        }

        public async Task Run()
        {
            _logger.LogInformation("Sm5shMusic v.01");
            _logger.LogInformation("--------------------");
            _logger.LogInformation("research: soneek");
            _logger.LogInformation("prcEditor: https://github.com/BenHall-7/paracobNET");
            _logger.LogInformation("msbtEditor: https://github.com/IcySon55/3DLandMSBTeditor");
            _logger.LogInformation("nus3audio:  https://github.com/jam1garner/nus3audio-rs");
            _logger.LogInformation("bgm-property:  https://github.com/jam1garner/smash-bgm-property");
            _logger.LogInformation("VGAudio:  https://github.com/Thealexbarney/VGAudio");
            _logger.LogInformation("--------------------");

            await Task.Delay(1000);
            _logger.LogInformation("MusicModPath: {MusicModPath}", _settings.MusicModPath);

            //Check proper resources exist
            if (!CheckApplicationFolders())
                return;

            //Init workspace
            if (!_workspace.Init())
                return;

            //Load Music Mods
            _logger.LogInformation("Loading Music Mods");
            var musicMods = new List<IMusicModManager>();
            foreach (var musicModFolder in Directory.GetDirectories(_settings.MusicModPath))
            {
                var newMusicMod = ActivatorUtilities.CreateInstance<Managers.MusicModManager>(_serviceProvider, musicModFolder);
                if (newMusicMod.Init())
                    musicMods.Add(newMusicMod);
            }

            //Generate Output mod
            _logger.LogInformation("--------------------");
            _logger.LogInformation("Starting Arc Mod Generation");
            var bgmEntries = musicMods.SelectMany(p => p.BgmEntries).ToDictionary(p => p.Key, p => p.Value);
            _arcModGeneratorService.GenerateArcMod(bgmEntries.Values.ToList());
        }

        private bool CheckApplicationFolders()
        {
            Directory.CreateDirectory(_settings.WorkspacePath);

            //Check if MusicModPath exists
            if (!Directory.Exists(_settings.MusicModPath))
            {
                _logger.LogError("Directory {MusicModPath} does not exist, aborting...", _settings.MusicModPath);
                return false;
            }

            //Check if LibsPath exists
            if (!Directory.Exists(_settings.LibsPath))
            {
                _logger.LogError("Directory {LibsPath} does not exist, aborting...", _settings.LibsPath);
                return false;
            }

            //Check if ResourcesPath exists
            if (!Directory.Exists(_settings.ResourcesPath))
            {
                _logger.LogError("Directory {ResourcesPath} does not exist, aborting...", _settings.ResourcesPath);
                return false;
            }

            //Check if Nus3AudioCLIExe exists
            if (!File.Exists(_resourceService.GetNus3AudioCLIExe()))
            {
                _logger.LogError("File {Nus3AudioCLIExe} does not exist, aborting...", _resourceService.GetNus3AudioCLIExe());
                return false;
            }

            //Check if BgmPropertyExe exists
            if (!File.Exists(_resourceService.GetBgmPropertyExe()))
            {
                _logger.LogError("File {BgmPropertyExe} does not exist, aborting...", _resourceService.GetBgmPropertyExe());
                return false;
            }

            //Check if BgmPropertyExe exists
            if (!File.Exists(_resourceService.GetBgmPropertyExe()))
            {
                _logger.LogError("File {BgmPropertyExe} does not exist, aborting...", _resourceService.GetBgmPropertyExe());
                return false;
            }

            //Check if BgmDbLabelsCsv exists
            if (!File.Exists(_resourceService.GetBgmDbLabelsCsvResource()))
            {
                _logger.LogError("File {BgmDbLabelsCsv} does not exist, aborting...", _resourceService.GetBgmDbLabelsCsvResource());
                return false;
            }

            //Check if NusBankIdsCsv exists
            if (!File.Exists(_resourceService.GetNusBankIdsCsvResource()))
            {
                _logger.LogError("File {NusBankIdsCsv} does not exist, aborting...", _resourceService.GetNusBankIdsCsvResource());
                return false;
            }

            //Check if BgmPropertyYml exists
            if (!File.Exists(_resourceService.GetBgmPropertyYmlResource()))
            {
                _logger.LogError("File {BgmPropertyYml} does not exist, aborting...", _resourceService.GetBgmPropertyYmlResource());
                return false;
            }

            //Check if BgmPropertyHashes exists
            if (!File.Exists(_resourceService.GetBgmPropertyHashes()))
            {
                _logger.LogError("File {BgmPropertyHashes} does not exist, aborting...", _resourceService.GetBgmPropertyHashes());
                return false;
            }

            //Check if NusBankTemplate exists
            if (!File.Exists(_resourceService.GetNusBankTemplateResource()))
            {
                _logger.LogError("File {NusBankTemplate} does not exist, aborting...", _resourceService.GetNusBankTemplateResource());
                return false;
            }

            //Check if GameTitleDb exists
            if (!File.Exists(_resourceService.GetGameTitleDbResource()))
            {
                _logger.LogError("File {GameTitleDb} does not exist, aborting...", _resourceService.GetGameTitleDbResource());
                return false;
            }

            //Check if BgmDb exists
            if (!File.Exists(_resourceService.GetBgmDbResource()))
            {
                _logger.LogError("File {BgmDb} does not exist, aborting...", _resourceService.GetBgmDbResource());
                return false;
            }

            return true;
        }
    }
}
