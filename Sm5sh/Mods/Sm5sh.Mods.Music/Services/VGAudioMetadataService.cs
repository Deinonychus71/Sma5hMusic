using Microsoft.Extensions.Logging;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Models;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VGAudio.Cli;

namespace Sm5sh.Mods.Music.Services
{
    public class VGAudioMetadataService : IAudioMetadataService
    {
        private readonly ILogger _logger;

        public VGAudioMetadataService(ILogger<IAudioMetadataService> logger)
        {
            _logger = logger;
        }

        public Task<AudioCuePoints> GetCuePoints(string inputFile)
        {
            _logger.LogDebug("Retrieving audio metadata for {FilePath}...", inputFile);

            var builder = new StringBuilder();

            var oldValue = Console.Out;
            using (var writer = new StringWriter(builder))
            {
                Console.SetOut(writer);
                Converter.RunConverterCli(new string[] { "-m", "-i", inputFile });
            }
            Console.SetOut(oldValue);

            var output = builder.ToString();

            var totalSamples = ReadValueUInt32Safe(output, "Sample count: ");
            var loopStartSample = ReadValueUInt32Safe(output, "Loop start: ");
            var loopEndSample = ReadValueUInt32Safe(output, "Loop end: ");
            var frequency = ReadValueUInt32Safe(output, "Sample rate: ");

            _logger.LogDebug("VGAudio Metadata for {FilePath}: TotalSamples: {TotalSamples}, LoopStartSample: {LoopStartSample}, LoopEndSample: {LoopEndSample}, Frequency: {Frequency}",
                inputFile, totalSamples, loopStartSample, loopEndSample, frequency);

            if (totalSamples == 0 || frequency == 0 || loopEndSample == 0)
            {
                _logger.LogWarning("VGAudio Metadata for {FilePath}: Total Samples, Frequency or/and loop end sample was 0! Check the logs for more information. Use song_cue_points_override property in the payload to override these values.", inputFile);
                _logger.LogDebug("VGAudio Metadata for {FilePath}: {Data}", inputFile, output);
            }

            return Task.FromResult(new AudioCuePoints()
            {
                TotalSamples = totalSamples,
                LoopStartSample = loopStartSample,
                LoopEndSample = loopEndSample,
                TotalTimeMs = frequency > 0 ? totalSamples * 1000 / frequency : 0,
                LoopStartMs = frequency > 0 ? loopStartSample * 1000 / frequency : 0,
                LoopEndMs = frequency > 0 ? loopEndSample * 1000 / frequency : 0
            });
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
                if (outputMediaFile.EndsWith("lopus"))
                {
                    //Special tags for opus
                    Converter.RunConverterCli(new string[] { "-i", inputMediaFile, "-o", outputMediaFile, "--opusheader", "Namco", "--cbr" });
                }
                else
                {
                    Converter.RunConverterCli(new string[] { "-i", inputMediaFile, "-o", outputMediaFile });
                }
            }
            Console.SetOut(oldValue);

            var output = builder.ToString();

            _logger.LogDebug("VGAudio Convert for {OutputMediaFile}: {Data}", outputMediaFile, output.Trim('\r', '\n'));

            if (!File.Exists(outputMediaFile) || new FileInfo(outputMediaFile).Length == 0)
            {
                _logger.LogError("VGAudio Error - The conversion from {InputMediaFile} to {OutputMediaFile} failed. Reason {Reason}", inputMediaFile, outputMediaFile, output.Trim('\r', '\n'));
                return false;
            }

            return true;
        }

        private ulong ReadValueUInt64Safe(string searchString, string parsingStartIndex, string parsingEndIndex = " ")
        {
            var output = searchString.Split(parsingStartIndex);
            if (output.Length > 1)
            {
                var foundValue = output[1].Split(parsingEndIndex)[0];
                if (ulong.TryParse(foundValue, out ulong result))
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
