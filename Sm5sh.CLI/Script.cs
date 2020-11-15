using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Sm5sh.Interfaces;
using System.Collections.Generic;

namespace Sm5sh.CLI
{
    public class Script
    {
        private readonly ILogger _logger;
        private readonly IStateManager _state;
        private readonly IServiceProvider _serviceProvider;
        private readonly IWorkspaceManager _workspace;
        private readonly IOptions<Sm5shOptions> _config;

        public Script(IServiceProvider serviceProvider, IOptions<Sm5shOptions> config, IWorkspaceManager workspace, IStateManager state, ILogger<Script> logger)
        {
            _serviceProvider = serviceProvider;
            _workspace = workspace;
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

            //Init State Manager
            _state.Init();

            //Init workspace
            if (!_workspace.Init())
                return;

            //Load Mods
            var mods = _serviceProvider.GetServices<ISm5shMod>();

            //Emulate UI that would init mods
            var initMods = new List<ISm5shMod>();
            foreach(var mod in mods)
            {
                _logger.LogInformation("Initialize mod {ModeName}", mod.ModName);
                if (mod.Init())
                    initMods.Add(mod);
            }
            //Emulate UI that would save changes from mods
            foreach (var mod in initMods)
            {
                _logger.LogInformation("Save changes for mod {ModeName}", mod.ModName);
                mod.SaveChanges();
            }

            //Generate Output mod
            _logger.LogInformation("--------------------");
            _logger.LogInformation("Starting State Manager Mod Generation");
            _state.WriteChanges();
            _logger.LogInformation("COMPLETE - Please check the logs for any error.");
            _logger.LogInformation("--------------------");
        }
    }
}
