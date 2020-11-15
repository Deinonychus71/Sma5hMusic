using CsvHelper;
using Microsoft.Extensions.Logging;
using Sm5sh.Mods.Music.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sm5sh.Mods.Music.Services
{
    public class Nus3AudioService : INus3AudioService
    {
        private readonly ILogger _logger;
        private readonly IResourceService _resourceService;
        private readonly IProcessService _processService;
        private ushort _lastBankId;

        public Nus3AudioService(IResourceService resourceService, IProcessService processService, ILogger<INus3AudioService> logger)
        {
            _logger = logger;
            _resourceService = resourceService;
            _processService = processService;
            var nus3BankIds = GetCoreNus3BankIds();
            _lastBankId = (ushort)(nus3BankIds.Count > 0 ? GetCoreNus3BankIds().Values.OrderByDescending(p => p).First() : 0);
        }

        public Dictionary<string, ushort> GetCoreNus3BankIds()
        {
            var output = new Dictionary<string, ushort>();

            var nusBankResourceFile = _resourceService.GetNusBankIdsCsvResource();
            if (!File.Exists(nusBankResourceFile))
                return output;

            _logger.LogDebug("Retrieving NusBankIds from CSV {CSVResource}", nusBankResourceFile);
            using (var reader = new StreamReader(nusBankResourceFile))
            {
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Configuration.PrepareHeaderForMatch = (header, index) => Regex.Replace(header, @"\s", string.Empty);
                    var records = csv.GetRecords<dynamic>();
                    foreach (var record in records)
                    {
                        var id = Convert.ToUInt16(record.ID, 16);
                        output.Add(record.NUS3BankName, id);
                    }
                }
            }
            return output;
        }

        public bool GenerateNus3Audio(string toneId, string inputMediaFile, string outputMediaFile)
        {
            _logger.LogDebug("Generate nus3audio {InternalToneName} from {AudioInputFile} to {Nus3AudioOutputFile}", toneId, inputMediaFile, outputMediaFile);

            if (!File.Exists(inputMediaFile))
            {
                _logger.LogError("File {mediaPath} does not exist....", inputMediaFile);
                return false;
            }

            var nus3AudioCliPath = _resourceService.GetNus3AudioCLIExe();
            if (!File.Exists(nus3AudioCliPath))
            {
                _logger.LogError("Executable {Nus3AudioCli} could not be found.", nus3AudioCliPath);
                return false;
            }

            try
            {
                _processService.RunProcess(nus3AudioCliPath, $"-n -w \"{outputMediaFile}\"");
                _processService.RunProcess(nus3AudioCliPath, $"-A {toneId} \"{inputMediaFile}\" -w \"{outputMediaFile}\"");
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Error while generating nus3audio file");
                return false;
            }

            return true;
        }

        public bool GenerateNus3Bank(string toneId, string inputMediaFile, string outputMediaFile)
        {
            _logger.LogDebug("Generate nus3bank {InternalToneName} from {Nus3BankInputFile} to {Nus3BankOutputFile}", toneId, inputMediaFile, outputMediaFile);

            if (!File.Exists(inputMediaFile))
            {
                _logger.LogError("File {mediaPath} does not exist....", inputMediaFile);
                return false;
            }

            //TODO Rough implementation, to fix
            using (var memoryStream = new MemoryStream())
            {
                using (var fileStream = File.Open(inputMediaFile, FileMode.Open, FileAccess.Read))
                {
                    fileStream.CopyTo(memoryStream);
                }
                using (var w = new BinaryWriter(memoryStream))
                {
                    w.BaseStream.Position = 0xc8;
                    w.Write(GetNewNus3BankId());
                }
                File.WriteAllBytes(outputMediaFile, memoryStream.ToArray());
            }

            return true;
        }

        private ushort GetNewNus3BankId()
        {
            _lastBankId++;
            return _lastBankId;
        }
    }
}
