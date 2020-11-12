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
            _logger.LogDebug("VGAudio Metadata for {FilePath}: TotalSamples: {TotalSamples}, LoopStartSample: {LoopStartSample}, LoopEndSample: {LoopEndSample}, Frequency: {Frequency}",
                filePath, audioCuePoints.TotalSamples, audioCuePoints.LoopStartSample, audioCuePoints.LoopEndSample, audioCuePoints.Frequency);

            if (audioCuePoints.TotalSamples == 0 || audioCuePoints.Frequency == 0 || audioCuePoints.LoopEndSample == 0)
            {
                _logger.LogWarning("VGAudio Metadata for {FilePath}: Total Samples, Frequency or/and loop end sample was 0! Check the logs for more information. Use song_cue_points_override property in the payload to override these values.", filePath);
                _logger.LogDebug("VGAudio Metadata for {FilePath}: {Data}", filePath, output);
            }

            return audioCuePoints;
        }

        public bool ConvertAudio(string inputMediaFile, string outputMediaFile)
        {
            _logger.LogDebug("Convert BRSTM from {AudioMediaFile} to {AudioOutputFile}", inputMediaFile, outputMediaFile);

            if (!File.Exists(inputMediaFile))
            {
                _logger.LogError("File {mediaPath} does not exist....", inputMediaFile);
                return false;
            }

            if (File.Exists(outputMediaFile))
            {
                _logger.LogDebug("The conversion from {InputMediaFile} to {OutputMediaFile} was skipped. The file already exists.", inputMediaFile, outputMediaFile);
                return true;
            }

            var builder = new StringBuilder();

            var oldValue = Console.Out;
            using (var writer = new StringWriter(builder))
            {
                Console.SetOut(writer);
                Converter.RunConverterCli(new string[] { "-i", inputMediaFile, "-o", outputMediaFile });
            }
            Console.SetOut(oldValue);

            var output = builder.ToString();

            _logger.LogDebug("VGAudio Convert for {OutputMediaFile}: {Data}", outputMediaFile, output);

            if (!File.Exists(outputMediaFile))
            {
                _logger.LogError("VGAudio Error - The conversion from {InputMediaFile} to {OutputMediaFile} failed.", inputMediaFile, outputMediaFile);
                return false;
            }

            return true;
        }

        private ulong ReadValueUInt64Safe(string searchString, string parsingStartIndex, string parsingEndIndex = " ")
        {
            var output = searchString.Split(parsingStartIndex);
            if(output.Length > 1)
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
            if (output.Length > 1)
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
            if (output.Length > 1)
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
