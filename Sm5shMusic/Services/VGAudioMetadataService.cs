using Microsoft.Extensions.Logging;
using Sm5shMusic.Interfaces;
using Sm5shMusic.Models;
using System;
using System.IO;
using System.Text;
using VGAudio.Cli;

namespace Sm5shMusic.Services
{
    public class VGAudioMetadataService: IAudioMetadataService
    {
        private readonly ILogger _logger;

        public VGAudioMetadataService(ILogger<Nus3AudioService> logger)
        {
            _logger = logger;
        }

        public AudioCuePoints GetCuePoints(string filePath)
        {
            _logger.LogDebug("Retrieving audio metadata for {FilePath}...", filePath);

            var builder = new StringBuilder();

            var oldValue = Console.Out;
            using (var writer = new StringWriter(builder))
            {
                Console.SetOut(writer);
                Converter.RunConverterCli(new string[] { "-m", "-i", filePath });
            }
            Console.SetOut(oldValue);

            var output = builder.ToString();

            var audioCuePoints = new AudioCuePoints()
            {
                TotalSamples = ReadValueUInt64Safe(output, "Sample count: "),
                LoopStartSample = ReadValueUInt64Safe(output, "Loop start: "),
                LoopEndSample = ReadValueUInt64Safe(output, "Loop end: "),
                Frequency = ReadValueUInt32Safe(output, "Sample rate: "),
            };

            if (audioCuePoints.TotalSamples == 0)
                _logger.LogWarning("{FilePath}: Total Samples was 0!");

            if (audioCuePoints.Frequency == 0)
                _logger.LogWarning("{FilePath}: Frequency was 0!");

            if (audioCuePoints.LoopEndSample == 0)
                _logger.LogWarning("{FilePath}: Loop end sample was 0!");

            return audioCuePoints;
        }

        private ulong ReadValueUInt64Safe(string searchString, string parsingStartIndex, string parsingEndIndex = " ")
        {
            var output = searchString.Split(parsingStartIndex);
            if(output.Length > 0)
            {
                var foundValue = output[1].Split(parsingEndIndex)[0];
                if(ulong.TryParse(foundValue, out ulong result))
                {
                    return result;
                }
            }
            return 0;
        }

        private uint ReadValueUInt32Safe(string searchString, string parsingStartIndex, string parsingEndIndex = " ")
        {
            var output = searchString.Split(parsingStartIndex);
            if (output.Length > 0)
            {
                var foundValue = output[1].Split(parsingEndIndex)[0];
                if (uint.TryParse(foundValue, out uint result))
                {
                    return result;
                }
            }
            return 0;
        }

        private ushort ReadValueUInt16Safe(string searchString, string parsingStartIndex, string parsingEndIndex = " ")
        {
            var output = searchString.Split(parsingStartIndex);
            if (output.Length > 0)
            {
                var foundValue = output[1].Split(parsingEndIndex)[0];
                if (ushort.TryParse(foundValue, out ushort result))
                {
                    return result;
                }
            }
            return 0;
        }
    }
}
