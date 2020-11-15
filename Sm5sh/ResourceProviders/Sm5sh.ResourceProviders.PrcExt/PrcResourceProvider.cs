using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Options;
using Sm5sh.Attributes;
using CsvHelper;
using Sm5sh.ResourceProviders.Prc.Helpers;

namespace Sm5sh.ResourceProviders
{
    [ResourceProviderMatch(".prc")]
    public class PrcResourceProvider : BaseResourceProvider
    {
        private readonly ILogger _logger;
        private readonly PrcHelper _prc;

        public PrcResourceProvider(IOptions<Sm5shOptions> config, ILogger<PrcResourceProvider> logger)
            : base(config)
        {
            _logger = logger;
            var inputFileParamLabels = Path.Combine(config.Value.ResourcesPath, "param_labels.csv");
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
            catch(Exception e)
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

            using (var reader = new StreamReader(inputFileParamLabels))
            {
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<dynamic>();
                    foreach (var record in records)
                    {
                        var id = Convert.ToUInt64(record.ID, 16);
                        output.Add(id, record.Label);
                    }
                }
            }
            return output;
        }
    }
}
