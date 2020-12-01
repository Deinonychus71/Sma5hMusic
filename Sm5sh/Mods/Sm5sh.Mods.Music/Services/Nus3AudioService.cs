using CsvHelper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sm5sh.Interfaces;
using Sm5sh.Mods.Music.Helpers;
using Sm5sh.Mods.Music.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sm5sh.Mods.Music.Services
{
    public class Nus3AudioService : INus3AudioService
    {
        private readonly ILogger _logger;
        private readonly IProcessService _processService;
        private readonly IAudioMetadataService _audioMetadataService;
        private readonly IOptions<Sm5shMusicOptions> _config;
        private readonly string _nus3AudioExeFile;
        private readonly string _nus3BankTemplateFile;
        private ushort _lastBankId;

        public Nus3AudioService(IOptions<Sm5shMusicOptions> config, IAudioMetadataService audioMetadataService, IProcessService processService, ILogger<INus3AudioService> logger)
        {
            _logger = logger;
            _processService = processService;
            _audioMetadataService = audioMetadataService;
            _config = config;
            _nus3AudioExeFile = Path.Combine(config.Value.ToolsPath, Constants.Resources.NUS3AUDIO_EXE_FILE);
            _nus3BankTemplateFile = Path.Combine(config.Value.ResourcesPath, Constants.Resources.NUS3BANK_TEMPLATE_FILE);

            if (!File.Exists(_nus3AudioExeFile))
                throw new Exception($"nus3audio.exe: {_nus3AudioExeFile} could not be found.");

            if (!File.Exists(_nus3BankTemplateFile))
                throw new Exception($"template.nus3bank: {_nus3BankTemplateFile} could not be found.");

            var nus3BankIds = GetCoreNus3BankIds();
            _lastBankId = (ushort)(nus3BankIds.Count > 0 ? GetCoreNus3BankIds().Values.OrderByDescending(p => p).First() : 0);
        }

        public bool GenerateNus3Audio(string toneId, string inputMediaFile, string outputMediaFile)
        {
            _logger.LogDebug("Generate nus3audio {InternalToneName} from {AudioInputFile} to {Nus3AudioOutputFile}", toneId, inputMediaFile, outputMediaFile);

            if (!File.Exists(inputMediaFile))
            {
                _logger.LogError("File {mediaPath} does not exist....", inputMediaFile);
                return false;
            }

            //Test nus3audio
            if (Path.GetExtension(inputMediaFile).ToLower() == ".nus3audio")
            {
                //Double checking that tone ids match
                var fileToneId = GetToneIdFromNus3Audio(inputMediaFile);
                if (fileToneId != toneId)
                {
                    _logger.LogError("The ToneId within the nus3audio {ToneIdNus3Audio} doesn't match the ToneId {ToneId} registered in the mod..", fileToneId, toneId);
                    return false;
                }
                File.Copy(inputMediaFile, outputMediaFile);
                return true;
            }

            //Handle conversation if necessary
            if (Constants.EXTENSIONS_NEED_CONVERSION.Contains(Path.GetExtension(inputMediaFile).ToLower()))
                return ConvertIncompatibleFormat(toneId, ref inputMediaFile, outputMediaFile);

            //Create nus3audio
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputMediaFile));
                _processService.RunProcess(_nus3AudioExeFile, $"-n -w \"{outputMediaFile}\"");
                _processService.RunProcess(_nus3AudioExeFile, $"-A {toneId} \"{inputMediaFile}\" -w \"{outputMediaFile}\"");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while generating nus3audio file");
                return false;
            }

            return true;
        }

        public string GetToneIdFromNus3Audio(string inputMediaFile)
        {
            _logger.LogDebug("Retrieving ToneId from {InputMediaFile}...", inputMediaFile);

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var fileStream = File.Open(inputMediaFile, FileMode.Open, FileAccess.Read))
                    {
                        using (var w = new BinaryReader(fileStream))
                        {
                            w.BaseStream.Position = 0x48; //ToneId

                            var sb = new StringBuilder();
                            char c;
                            while ((c = w.ReadChar()) != '\0')
                            {

                                sb.Append(c);
                            };
                            return sb.ToString();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return null;
            }
        }

        public bool GenerateNus3Bank(string toneId, float volume, string outputMediaFile)
        {
            _logger.LogDebug("Generate nus3bank {InternalToneName} from {Nus3BankInputFile} to {Nus3BankOutputFile}", toneId, _nus3BankTemplateFile, outputMediaFile);

            using (var memoryStream = new MemoryStream())
            {
                using (var fileStream = File.Open(_nus3BankTemplateFile, FileMode.Open, FileAccess.Read))
                {
                    fileStream.CopyTo(memoryStream);
                }
                var bytes = memoryStream.ToArray();
                var found = ByteHelper.Locate(bytes, new byte[] { 0xE8, 0x22, 0x00, 0x00 });
                using (var w = new BinaryWriter(memoryStream))
                {
                    w.BaseStream.Position = 0xc8; //NameId
                    w.Write(GetNewNus3BankId());
                    if (found.Length != 3)
                    {
                        _logger.LogError("Error while locating the volume offset in the nus3bank");
                    }
                    else
                    {
                        w.BaseStream.Position = found[1] + 4;
                        w.Write(volume);
                    }
                }
                Directory.CreateDirectory(Path.GetDirectoryName(outputMediaFile));
                File.WriteAllBytes(outputMediaFile, memoryStream.ToArray());
            }

            return true;
        }

        private ushort GetNewNus3BankId()
        {
            _lastBankId++;
            return _lastBankId;
        }

        private bool ConvertIncompatibleFormat(string toneId, ref string inputMediaFile, string outputMediaFile, bool isFallback = false)
        {
            bool result = false;
            var formatConversation = isFallback ? _config.Value.Sm5shMusic.AudioConversionFormatFallBack : _config.Value.Sm5shMusic.AudioConversionFormat;
            var tempFile = Path.Combine(_config.Value.TempPath, string.Format(Constants.Resources.NUS3AUDIO_TEMP_FILE, formatConversation));
            if (_audioMetadataService.ConvertAudio(inputMediaFile, tempFile))
            {
                inputMediaFile = tempFile;
                result = GenerateNus3Audio(toneId, inputMediaFile, outputMediaFile);
            }
            if (File.Exists(tempFile))
                File.Delete(tempFile);

            if (!result && !isFallback && !string.IsNullOrEmpty(_config.Value.Sm5shMusic.AudioConversionFormatFallBack)
                && _config.Value.Sm5shMusic.AudioConversionFormat.ToLower() != _config.Value.Sm5shMusic.AudioConversionFormatFallBack.ToLower())
            {
                _logger.LogWarning("The conversion from {InputMediaFile} to {OutputMediaFile} failed. Trying fallback conversation format.", inputMediaFile, outputMediaFile);
                return ConvertIncompatibleFormat(toneId, ref inputMediaFile, outputMediaFile, true);
            }

            return result;
        }

        private Dictionary<string, ushort> GetCoreNus3BankIds()
        {
            var output = new Dictionary<string, ushort>();

            var nusBankResourceFile = Path.Combine(_config.Value.GameResourcesPath, Constants.Resources.NUS3BANK_IDS_FILE);
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
    }
}
