using Microsoft.Extensions.Logging;
using Sm5sh.Data.Sound.Config;
using Sm5sh.Services.FileSpecific.Interfaces;
using Sm5sh.Services.Interfaces;
using System;
using System.IO;

namespace Sm5sh.Services.FileSpecific
{
    public class BgmPropertyService : IBgmPropertyService
    {
        private readonly ILogger _logger;
        private readonly IYmlService _ymlService;
        private readonly IProcessService _processService;

        public BgmPropertyService(IYmlService ymlService, ILogger<IBgmPropertyService> logger)
        {
            _logger = logger;
            _ymlService = ymlService;
        }

        public BinBgmProperty ReadBgmPropertyFile(string bgmPropertyExeFile, string bgmPropertyHashFile, string bgmPropertyFile, string tempPath)
        {
            if (!File.Exists(bgmPropertyFile))
            {
                _logger.LogError("bgm_property.bin {BgmPropertyFile} could not be found.", bgmPropertyFile);
                return null;
            }

            if (!File.Exists(bgmPropertyExeFile))
            {
                _logger.LogError("bgm-property.exe {BgmPropertyExeFile} could not be found.", bgmPropertyExeFile);
                return null;
            }

            if (!File.Exists(bgmPropertyHashFile))
            {
                _logger.LogError("bgm_hashes.txt {BgmPropertyHashFile} could not be found.", bgmPropertyHashFile);
                return null;
            }

            var tempFile = Path.Combine(tempPath, "temp.yml");

            //Retrieve YML from Bgm Property
            try
            {
                _processService.RunProcess(bgmPropertyExeFile, $"-l \"{bgmPropertyHashFile}\" \"{bgmPropertyFile}\" \"{tempFile}\"");
            }
            catch (Exception e)
            {
                throw new Exception("Error while generating bgm_property.yml file.", e);
            }

            var output = _ymlService.ReadYmlFile<BinBgmProperty>(tempFile);

            //Delete temp file
            File.Delete(tempFile);

            return output;
        }

        public bool WriteBgmPropertyFile(string bgmPropertyExeFile, string outputFile, string tempPath, BinBgmProperty inputObj)
        {
            if (!File.Exists(bgmPropertyExeFile))
            {
                _logger.LogError("bgm-property.exe {BgmPropertyExeFile} could not be found.", bgmPropertyExeFile);
                return false;
            }

            var tempFile = Path.Combine(tempPath, "temp.yml");

            //Serialize
            var ymlStr = _ymlService.WriteYmlFile(tempFile, inputObj);

            //Build Bgm Property
            try
            {
                _processService.RunProcess(bgmPropertyExeFile, $"\"{tempPath}\" \"{outputFile}\"");
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
