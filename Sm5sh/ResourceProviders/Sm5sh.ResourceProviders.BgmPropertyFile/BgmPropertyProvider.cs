using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sm5sh.Attributes;
using Sm5sh.Interfaces;
using Sm5sh.Mods.Data.Sound.Config;
using Sm5sh.Mods.Data.Sound.Config.BgmPropertyStructs;
using Sm5sh.ResourceProviders.BgmPropertyFile.Helpers;
using Sm5sh.ResourceProviders.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sm5sh.ResourceProviders
{
    [ResourceProviderMatch(BgmPropertyFileConstants.BGM_PROPERTY_PATH)]
    public class BgmPropertyProvider : BaseResourceProvider
    {
        private readonly ILogger _logger;
        private readonly IProcessService _processService;
        private readonly YmlHelper _ymlHelper;
        private readonly string _bgmPropertyExeFile;
        private readonly string _bgmPropertyHashFile;

        public BgmPropertyProvider(IOptions<Sm5shOptions> config, IProcessService processService, ILogger<BgmPropertyProvider> logger)
            : base(config)
        {
            _logger = logger;
            _ymlHelper = new YmlHelper();
            _processService = processService;
            _bgmPropertyExeFile = Path.Combine(config.Value.ToolsPath, BgmPropertyFileConstants.BGM_PROPERTY_EXE_FILE);
            _bgmPropertyHashFile = Path.Combine(config.Value.ToolsPath, BgmPropertyFileConstants.BGM_PROPERTY_HASH_FILE);

            if (!File.Exists(_bgmPropertyExeFile))
                throw new Exception($"bgm-property.exe: {_bgmPropertyExeFile} could not be found.");

            if (!File.Exists(_bgmPropertyHashFile))
                throw new Exception($"bgm_hashes.txt: {_bgmPropertyHashFile} could not be found.");
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

            Directory.CreateDirectory(_config.Value.TempPath);
            var tempFile = Path.Combine(_config.Value.TempPath, BgmPropertyFileConstants.BGM_PROPERTY_TEMP_FILE);

            //Retrieve YML from Bgm Property
            try
            {
                _processService.RunProcess(_bgmPropertyExeFile, $"-l \"{_bgmPropertyHashFile}\" \"{inputFile}\" \"{tempFile}\"");
            }
            catch (Exception e)
            {
                throw new Exception("Error while generating bgm_property.yml file.", e);
            }

            var output = _ymlHelper.ReadYmlFile<List<BgmPropertyEntry>>(tempFile);

            if (output.Count == 0)
            {
                throw new Exception("Error while generating YML for BGM Property.");
            }

            if (output[0].NameId.EndsWith('\r'))
            {
                throw new Exception("Error while generating YML for BGM Property. Your bgm_hashes may have crlf endings which are known to cause an issue with bgm-property");
            }


            //Delete temp file
            File.Delete(tempFile);

            return (T)(object)new BinBgmProperty() { Entries = output.ToDictionary(p => p.NameId, p => p) };
        }

        public override bool WriteFile<T>(string inputFile, string outputFile, T inputObj)
        {
            if (!typeof(T).IsAssignableFrom(typeof(BinBgmProperty)))
                throw new Exception($"Tried to use BgmPropertyProvider with wrong mapping type '{nameof(BinBgmProperty)}'");

            Directory.CreateDirectory(_config.Value.TempPath);
            var tempFile = Path.Combine(_config.Value.TempPath, BgmPropertyFileConstants.BGM_PROPERTY_TEMP_FILE);

            //Serialize
            _ymlHelper.WriteYmlFile(tempFile, ((BinBgmProperty)(object)inputObj).Entries.Values);

            //Build Bgm Property
            try
            {
                _processService.RunProcess(_bgmPropertyExeFile, $"\"{tempFile}\" \"{outputFile}\"");
                if (!File.Exists(outputFile))
                {
                    throw new Exception("Bgm property was not generated. Check the yaml manually in the temp folder");
                }
                File.Delete(tempFile);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while generating bgm property file");
                return false;
            }

            return true;
        }
    }
}
