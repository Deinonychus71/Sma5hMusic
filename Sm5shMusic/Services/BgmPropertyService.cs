using Microsoft.Extensions.Logging;
using Sm5shMusic.Interfaces;
using Sm5shMusic.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Sm5shMusic.Services
{
    public class BgmPropertyService : IBgmPropertyService
    {
        private readonly ILogger _logger;
        private readonly IResourceService _resourceService;
        private readonly IWorkspaceManager _workspace;
        private readonly IProcessService _processService;
        private readonly IDeserializer _yamlDeserializer;
        private readonly ISerializer _yamlSerializer;

        public BgmPropertyService(IResourceService resourceService, IProcessService processService, IWorkspaceManager workspace, ILogger<IBgmPropertyService> logger)
        {
            _logger = logger;
            _resourceService = resourceService;
            _workspace = workspace;
            _processService = processService;
            _yamlDeserializer = new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
            _yamlSerializer = new SerializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
        }

        public bool GenerateBgmProperty(List<MusicModBgmEntry> bgmEntries)
        {
            var bgmPropertyExecPath = _resourceService.GetBgmPropertyExe();
            if (!File.Exists(bgmPropertyExecPath))
            {
                _logger.LogError("Executable {BgmPropertyExe} could not be found.", bgmPropertyExecPath);
                return false;
            }

            var tempBgmPropertyYml = _workspace.GetWorkspaceOutputForBgmPropertyTempFile();
            var templateBgmPropertyYml = _resourceService.GetBgmPropertyYmlResource();

            //Pick either bgm_property.yml or bgm_property.bin if yml doesn't exist
            if (File.Exists(templateBgmPropertyYml))
            {
                File.Copy(templateBgmPropertyYml, tempBgmPropertyYml);
            }
            else
            {
                if (!ExtractBgmPropertyYml(bgmPropertyExecPath, tempBgmPropertyYml))
                    return false;
            }

            //Deserialize
            var yamlStr = File.ReadAllText(tempBgmPropertyYml);
            var yamlEntries = _yamlDeserializer.Deserialize<List<BgmPropertyModel>>(yamlStr);

            foreach(var bgmEntry in bgmEntries)
            {
                yamlEntries.Add(new BgmPropertyModel()
                {
                    NameId = bgmEntry.InternalToneName,
                    TotalSamples = bgmEntry.CuePoints.TotalSample,
                    TotalTimeMs = bgmEntry.CuePoints.TotalTimeMs,
                    LoopStartMs = bgmEntry.CuePoints.LoopStartMs,
                    LoopStartSample = bgmEntry.CuePoints.LoopStartSample,
                    LoopEndMs = bgmEntry.CuePoints.LoopEndMs,
                    LoopEndSample = bgmEntry.CuePoints.LoopEndSample,
                });
            }

            //Serialize
            yamlStr = _yamlSerializer.Serialize(yamlEntries);
            File.WriteAllText(tempBgmPropertyYml, yamlStr);

            //Generate Bgm Property Bin output
            var bgmPropertyBin = _workspace.GetWorkspaceOutputForBgmPropertyFile();
            GenerateBgmProperty(bgmPropertyExecPath, tempBgmPropertyYml, bgmPropertyBin);

            return true;
        }

        private bool ExtractBgmPropertyYml(string bgmPropertyExecPath, string outputTempBgmPropertyYml)
        {
            var bgmPropertyFileResource = _resourceService.GetBgmPropertyResource();
            if (!File.Exists(bgmPropertyFileResource))
            {
                _logger.LogError("bgm_property.bin {BgmProperty} could not be found.", bgmPropertyFileResource);
                return false;
            }

            var bgmPropertyHashesFileResource = _resourceService.GetBgmPropertyHashes();
            if (!File.Exists(bgmPropertyHashesFileResource))
            {
                _logger.LogError("bgm_hashes.txt {BgmProperty} could not be found.", bgmPropertyHashesFileResource);
                return false;
            }

            var bgmPropertyOutput = _workspace.GetWorkspaceOutputForBgmPropertyFile();

            try
            {
                _processService.RunProcess(bgmPropertyExecPath, $"-l \"{bgmPropertyHashesFileResource}\" \"{bgmPropertyFileResource}\" \"{outputTempBgmPropertyYml}\"");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while generating nus3audio file");
                return false;
            }

            return true;
        }

        private bool GenerateBgmProperty(string bgmPropertyExecPath, string tempBgmPropertyYml, string outputBgmPropertyBin)
        {
            try
            {
                _processService.RunProcess(bgmPropertyExecPath, $"\"{tempBgmPropertyYml}\" \"{outputBgmPropertyBin}\"");
                File.Delete(tempBgmPropertyYml);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while generating nus3audio file");
                return false;
            }
        }
    }
}
