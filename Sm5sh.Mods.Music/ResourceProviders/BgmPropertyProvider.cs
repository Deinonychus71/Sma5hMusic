using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sm5sh.Attributes;
using Sm5sh.Interfaces;
using Sm5sh.Mods.Music.Data.Sound.Config;
using Sm5sh.Mods.Music.Data.Sound.Config.BgmPropertyStructs;
using Sm5sh.Mods.Music.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sm5sh.Mods.Music.ResourceProviders
{
    [ResourceProviderMatch("sound/config/bgm_property.bin")]
    public class BgmPropertyProvider : BaseResourceProvider
    {
        private readonly ILogger _logger;
        private readonly IYmlService _ymlService;
        private readonly IProcessService _processService;
        private readonly string _bgmPropertyExeFile;
        private readonly string _bgmPropertyHashFile;

        public BgmPropertyProvider(IOptions<Sm5shOptions> config, IProcessService processService, IYmlService ymlService, ILogger<BgmPropertyProvider> logger)
            : base(config)
        {
            _logger = logger;
            _ymlService = ymlService;
            _processService = processService;
            _bgmPropertyExeFile = Path.Combine(config.Value.ToolsPath, "BgmProperty", "bgm-property.exe");
            _bgmPropertyHashFile = Path.Combine(config.Value.ToolsPath, "BgmProperty", "bgm_hashes.txt");
        }

        public override T ReadFile<T>(string inputFile)
        {
            if (!typeof(T).IsAssignableFrom(typeof(BinBgmProperty)))
                throw new Exception($"Tried to use BgmPropertyProvider with wrong mapping type '{nameof(BinBgmProperty)}'");

            if (!File.Exists(inputFile))
            {
                _logger.LogError("bgm_property.bin {BgmPropertyFile} could not be found.", inputFile);
                return default;
            }

            if (!File.Exists(_bgmPropertyExeFile))
            {
                _logger.LogError("bgm-property.exe {BgmPropertyExeFile} could not be found.", _bgmPropertyExeFile);
                return default;
            }

            if (!File.Exists(_bgmPropertyHashFile))
            {
                _logger.LogError("bgm_hashes.txt {BgmPropertyHashFile} could not be found.", _bgmPropertyHashFile);
                return default;
            }

            var tempFile = Path.Combine(_config.Value.TempPath, "temp.yml");

            //Retrieve YML from Bgm Property
            try
            {
                _processService.RunProcess(_bgmPropertyExeFile, $"-l \"{_bgmPropertyHashFile}\" \"{inputFile}\" \"{tempFile}\"");
            }
            catch (Exception e)
            {
                throw new Exception("Error while generating bgm_property.yml file.", e);
            }

            var output = _ymlService.ReadYmlFile<List<BgmPropertyEntry>>(tempFile);

            //Delete temp file
            File.Delete(tempFile);

            return (T)(object)new BinBgmProperty() { Entries = output };
        }

        public override bool WriteFile<T>(string inputFile, string outputFile, T inputObj)
        {
            if (!typeof(T).IsAssignableFrom(typeof(BinBgmProperty)))
                throw new Exception($"Tried to use BgmPropertyProvider with wrong mapping type '{nameof(BinBgmProperty)}'");

            if (!File.Exists(_bgmPropertyExeFile))
            {
                _logger.LogError("bgm-property.exe {BgmPropertyExeFile} could not be found.", _bgmPropertyExeFile);
                return false;
            }

            var tempFile = Path.Combine(_config.Value.TempPath, "temp.yml");

            //Serialize
            var ymlStr = _ymlService.WriteYmlFile(tempFile, ((BinBgmProperty)(object)inputObj).Entries);

            //Build Bgm Property
            try
            {
                _processService.RunProcess(_bgmPropertyExeFile, $"\"{tempFile}\" \"{outputFile}\"");
                File.Delete(tempFile);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while generating nus3audio file");
                return false;
            }

            return true;
        }
    }
}
