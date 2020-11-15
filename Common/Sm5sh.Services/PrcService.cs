using CsvHelper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Sm5sh.Services.Interfaces;
using Sm5sh.Helpers.PrcHelper;

namespace Sm5sh.Services
{
    public class PrcService : IPrcService
    {
        private readonly ILogger _logger;
        private readonly PrcHelper _prc;

        public PrcService(string inputFileParamLabels, ILogger<IPrcService> logger)
        {
            _logger = logger;
            _prc = new PrcHelper(GetParamLabels(inputFileParamLabels));

        }

        public T ReadPrcFile<T>(string inputFile) where T : IPrcParsable, new()
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

        public bool WritePrcFile<T>(string outputFile, T inputObj) where T : IPrcParsable
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
