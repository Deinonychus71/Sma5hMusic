﻿using System;
using System.IO;
using YamlDotNet.Serialization;

namespace Sm5sh.Helpers.YmlHelper
{
    public class YmlHelper
    {
        private readonly IDeserializer _yamlDeserializer;
        private readonly ISerializer _yamlSerializer;

        public YmlHelper(INamingConvention namingConvention = null)
        {
            if (namingConvention != null)
            {
                _yamlDeserializer = new DeserializerBuilder().WithNamingConvention(namingConvention).Build();
                _yamlSerializer = new SerializerBuilder().WithNamingConvention(namingConvention).Build();
            }
            else
            {
                _yamlDeserializer = new DeserializerBuilder().Build();
                _yamlSerializer = new SerializerBuilder().Build();
            }
        }

        public T ReadYmlFile<T>(string inputFile) where T : IYmlParsable, new()
        {
            //Validate file
            if (!File.Exists(inputFile))
                throw new Exception($"PRC File does not exist: {inputFile}");

            //Open file
            var yamlStr = File.ReadAllText(inputFile);

            //Deserialize
            var outputObj = _yamlDeserializer.Deserialize<T>(yamlStr);

            return outputObj;
        }

        public void WriteYmlFile<T>(string outputFile, T inputObj) where T : IYmlParsable
        {
            //Write recursively
            var outputStr = _yamlSerializer.Serialize(inputObj);

            //Save
            File.WriteAllText(outputFile, outputStr);
        }
    }
}
