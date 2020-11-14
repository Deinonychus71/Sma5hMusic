﻿using Microsoft.Extensions.Logging;
using Sm5sh.Helpers.YmlHelper;
using Sm5sh.Services.Interfaces;
using System;

namespace Sm5sh.Services.Shared
{
    public class YmlService : IYmlService
    {
        private readonly ILogger _logger;
        private readonly YmlHelper _yml;

        public YmlService(ILogger<IYmlService> logger)
        {
            _logger = logger;
            _yml = new YmlHelper();

        }

        public T ReadYmlFile<T>(string inputFile) where T : IYmlParsable, new()
        {
            try
            {
                _logger.LogDebug("Reading yml file {InputFile}", inputFile);
                return _yml.ReadYmlFile<T>(inputFile);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while reading yml file {InputFile}", inputFile);
                return default;
            }
        }

        public bool WriteYmlFile<T>(string outputFile, T inputObj) where T : IYmlParsable
        {
            try
            {
                _logger.LogDebug("Writing yml file {OutputFile}", outputFile);
                _yml.WriteYmlFile(outputFile, inputObj);
                return true;
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Error while saving yml file {OutputFile}", outputFile);
                return false;
            }
        }
    }
}
