using CsvHelper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sma5h.Attributes;
using Sma5h.ResourceProviders.Prc.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Sma5h.ResourceProviders
{
    [ResourceProviderMatch(".prc")]
    public class PrcResourceProvider : BaseResourceProvider
    {
        private readonly ILogger _logger;
        private readonly PrcHelper _prc;

        public PrcResourceProvider(IOptionsMonitor<Sma5hOptions> config, ILogger<PrcResourceProvider> logger)
            : base(config)
        {
            _logger = logger;
            var inputFileParamLabels = Path.Combine(config.CurrentValue.ResourcesPath, "ParamLabels.csv");
            _prc = new PrcHelper(GetParamLabels(inputFileParamLabels));
        }

        public override T ReadFile<T>(string inputFile)
        {
            try
            {
                _logger.LogDebug("Reading prc file {InputFile}", inputFile);
                return _prc.ReadPrcFile<T>(inputFile);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while reading prc file {InputFile}", inputFile);
                return default;
            }
        }

        public override bool WriteFile<T>(string inputFile, string outputFile, T inputObj)
        {
            try
            {
                _logger.LogDebug("Writing prc file {OutputFile}", outputFile);
                _prc.WritePrcFile(outputFile, inputObj);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while saving prc file {OutputFile}", outputFile);
                return false;
            }
        }

        private Dictionary<ulong, string> GetParamLabels(string inputFileParamLabels)
        {
            var output = new Dictionary<ulong, string>();

            if (!File.Exists(inputFileParamLabels))
                throw new Exception($"Param Hashes {inputFileParamLabels} doesn't exist.");

            var hashLines = File.ReadAllLines(inputFileParamLabels);
            foreach(var hashLine in hashLines)
            {
                if (!string.IsNullOrEmpty(hashLine))
                {
                    var hashSplit = hashLine.Split(',');
                    output.Add(Convert.ToUInt64(hashSplit[0], 16), hashSplit[1]);
                }
            }
            return output;
        }
    }
}
