using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MsbtEditor;
using Sma5h.Attributes;
using Sma5h.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sma5h.ResourceProviders
{
    [ResourceProviderMatch(".msbt")]
    public class MsbtResourceProvider : BaseResourceProvider
    {
        private readonly ILogger _logger;

        public MsbtResourceProvider(IOptionsMonitor<Sma5hOptions> config, ILogger<MsbtResourceProvider> logger)
            : base(config)
        {
            _logger = logger;
        }

        public override T ReadFile<T>(string inputFile)
        {
            if (!typeof(T).IsAssignableFrom(typeof(MsbtDatabase)))
                throw new Exception($"Tried to use MsbtResourceProvider with wrong mapping type '{nameof(MsbtDatabase)}'");

            try
            {
                _logger.LogDebug("Reading msbt file {InputFile}", inputFile);

                var output = new MsbtDatabase() { Entries = new Dictionary<string, string>() };
                var msbtFile = new MSBT(inputFile);

                foreach (var msbtEntry in msbtFile.LBL1.Labels)
                {
                    var value = msbtFile.TXT2.OriginalStrings.FirstOrDefault(p => p.Index == msbtEntry.Index);
                    output.Entries.Add(((Label)msbtEntry).Name, Encoding.Unicode.GetString(value.Value).TrimEnd('\0'));
                }

                return (T)(object)output;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while reading prc file {InputFile}", inputFile);
                return default;
            }
        }

        public override bool WriteFile<T>(string inputFile, string outputFile, T inputObj)
        {
            if (!typeof(T).IsAssignableFrom(typeof(MsbtDatabase)))
                throw new Exception($"Tried to used MsbtResourceProvider with wrong mapping type '{nameof(MsbtDatabase)}'");

            try
            {
                var msbtDb = (MsbtDatabase)(object)inputObj;

                _logger.LogDebug("MSBT: {NrbEntries} entries, InputFile: {InputFile}, OutputFile: {OutputFile}", msbtDb.Entries.Count, inputFile, outputFile);
                File.Copy(inputFile, outputFile);

                var msbtFile = new MSBT(outputFile);

                //Clean everything
                var labels = msbtFile.LBL1.Labels.Select(p => (Label)p).ToList();
                foreach (var label in labels)
                    msbtFile.RemoveLabel(label);

                //Add everything
                foreach (var newMsbtEntry in msbtDb.Entries)
                {
                    var newEntry = msbtFile.AddLabel(newMsbtEntry.Key);
                    var valueStr = newMsbtEntry.Value;
                    if (string.IsNullOrEmpty(newMsbtEntry.Value))
                        valueStr = "MISSING";
                    newEntry.Value = Encoding.Unicode.GetBytes(valueStr + "\0");
                }
                msbtFile.Save();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "MSBT Generation error");
            }

            return true;
        }

        /*
         * public bool GenerateNewEntries(List<MsbtNewEntryModel> newMsbtEntries, string inputMsbtFile, string outputMsbtFile)
        {
            try
            {
                _logger.LogDebug("MSBT: {NrbEntries} entries, InputFile: {InputFile}, OutputFile: {OutputFile}", newMsbtEntries.Count, inputMsbtFile, outputMsbtFile);
                File.Copy(inputMsbtFile, outputMsbtFile);
                var msbtFile = new MSBT(outputMsbtFile);
                foreach (var newMsbtEntry in newMsbtEntries)
                {
                    _logger.LogDebug("MSBT: Adding {Label}:{Value}", newMsbtEntry.Label, newMsbtEntry.Value);
                    var newEntry = msbtFile.AddLabel(newMsbtEntry.Label);
                    newEntry.Value = Encoding.Unicode.GetBytes(newMsbtEntry.Value + "\0");
                }
                msbtFile.Save();
            }
            catch(Exception e)
            {
                _logger.LogError(e, "MSBT Generation error");
            }

            return true;
        }*/
    }
}
