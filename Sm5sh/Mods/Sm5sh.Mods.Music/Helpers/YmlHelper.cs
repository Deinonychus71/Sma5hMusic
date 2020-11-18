using System;
using System.IO;
using YamlDotNet.Serialization;

namespace Sm5sh.Mods.Music.Helpers
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

        public T ReadYmlFile<T>(string inputFile) where T : new()
        {
            //Validate file
            if (!File.Exists(inputFile))
                throw new Exception($"YML File does not exist: {inputFile}");

            //Open file
            var yamlStr = File.ReadAllText(inputFile);

            //Deserialize
            var outputObj = _yamlDeserializer.Deserialize<T>(yamlStr);

            return outputObj;
        }

        public void WriteYmlFile<T>(string outputFile, T inputObj)
        {
            //Write recursively
            var outputStr = _yamlSerializer.Serialize(inputObj);

            //Save
            File.WriteAllText(outputFile, outputStr);
        }
    }
}
