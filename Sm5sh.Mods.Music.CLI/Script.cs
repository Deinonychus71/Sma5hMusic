using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Sm5shMusic.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Sm5shMusic.Helpers;
using Sm5shMusic.Models;
using Newtonsoft.Json;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Interfaces;
using Sm5sh;

namespace Sm5shMusic
{
    public class Script
    {
        private readonly ILogger _logger;
        private readonly IStateManager _state;
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<Sm5shOptions> _config;

        public Script(IServiceProvider serviceProvider, IOptions<Sm5shOptions> config, IStateManager state, ILogger<Script> logger)
        {
            _serviceProvider = serviceProvider;
            _state = state;
            _logger = logger;
            _config = config;
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
            _logger.LogInformation("ModsPath: {ModsPath}", _config.Value.ModsPath);
            //_logger.LogInformation("AudioCache: {AudioCache}", _settings.EnableAudioCaching ? "Enabled - If songs are mismatched try to clear the cache!" : "Disabled");

            //var stageModJsonFile = LoadStagePlaylistMod();
            //_logger.LogInformation("StagePlaylistMod: {StagePlaylistMod}", stageModJsonFile != null ? "Enabled" : "Disabled");

            //Check proper resources exist
            //if (!CheckApplicationFolders())
            //    return;

            //Init workspace
            //if (!_workspace.Init())
            //    return;

            //Load Music Mods
            _logger.LogInformation("Running Mods");
            var mods = _serviceProvider.GetServices<ISm5shMod>();

            //Emulate UI that would init mods
            foreach(var mod in mods)
            {
                _logger.LogInformation("Initialize mod {ModeName}", "TOADD");
                mod.Init();
            }
            //Emulate UI that would save changes from mods
            foreach (var mod in mods)
            {
                _logger.LogInformation("Save changes for mod {ModeName}", "TOADD");
                mod.SaveChanges();
            }

            //Generate Output mod
            _logger.LogInformation("--------------------");
            _logger.LogInformation("Starting State Manager Mod Generation");
            /*var bgmEntries = musicMods.SelectMany(p => p.BgmEntries).Select(p => p.Value).ToList();
            _arcModGeneratorService.GenerateArcMusicMod(bgmEntries);
            if(stageModJsonFile != null)
            {
                _logger.LogInformation("--------------------");
                _logger.LogInformation("Starting Arc Stage Playlist Mod Generation");
                _arcModGeneratorService.GenerateArcStagePlaylistMod(bgmEntries, stageModJsonFile);
            }*/
            _state.WriteChanges();
            _logger.LogInformation("COMPLETE - Please check the logs for any error.");
            _logger.LogInformation("--------------------");
        }

        /*private bool CheckApplicationFolders()
        {
            Directory.CreateDirectory(_settings.WorkspacePath);
            Directory.CreateDirectory(_settings.TempPath);

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

        private List<StagePlaylistModConfig> LoadStagePlaylistMod()
        {
            var stageModJsonFile = Path.Combine(_settings.MusicModPath, Constants.MusicModFiles.StageModMetadataJsonFile);
            var generateStageMod = File.Exists(stageModJsonFile);
            if (!generateStageMod)
            {
                _logger.LogDebug("The file {StageModJsonFile} could not be found. The stage playlists will not be updated.", stageModJsonFile);
                return null;
            }

            var stagePlaylistsEntries = JsonConvert.DeserializeObject<List<StagePlaylistModConfig>>(File.ReadAllText(stageModJsonFile));

            if(stagePlaylistsEntries.Any(p => !Constants.ValidStages.Contains(p.StageId)))
            {
                _logger.LogError("{StagePlaylistMod} will be disabled. At least one stage is not registered in the Stage DB.");
                return null;
            }

            if (stagePlaylistsEntries.Any(p => p.OrderId < 0 || p.OrderId > 15))
            {
                _logger.LogError("{StagePlaylistMod} will be disabled. At least one stage has an invalid order id. The value is 0 to 15.");
                return null;
            }

            return stagePlaylistsEntries;
        }*/
    }
}
