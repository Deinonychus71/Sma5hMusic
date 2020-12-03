using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sm5sh.Interfaces;
using Sm5sh.Mods.Music.Helpers;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Models;
using System.IO;

namespace Sm5sh.Mods.Music
{
    public class Sm5shMusic : BaseSm5shMod
    {
        private readonly ILogger _logger;
        private readonly IOptions<Sm5shMusicOptions> _config;
        private readonly IAudioStateService _audioStateService;
        private readonly IMusicModManagerService _musicModManagerService;
        private readonly INus3AudioService _nus3AudioService;

        public override string ModName => "Sm5shMusic";

        public Sm5shMusic(IOptions<Sm5shMusicOptions> config, IMusicModManagerService musicModManagerService, IAudioStateService audioStateService,
            INus3AudioService nus3AudioService, IStateManager state, ILogger<Sm5shMusic> logger)
            : base(state)
        {
            _logger = logger;
            _audioStateService = audioStateService;
            _nus3AudioService = nus3AudioService;
            _musicModManagerService = musicModManagerService;
            _state = state;
            _config = config;
        }

        public override bool Init()
        {
            _logger.LogInformation("Sm5shMusic Path: {MusicModPath}", _config.Value.Sm5shMusic.ModPath);
            _logger.LogInformation("Audio Conversion Format: {AudioConversionFormat}", _config.Value.Sm5shMusic.AudioConversionFormat);
            _logger.LogInformation("Resources Path: {ResourcesPath}", _config.Value.Sm5shMusic.EnableAudioCaching ? "Enabled - If songs are mismatched try to clear the cache!" : "Disabled");
            _logger.LogInformation("Cache Path: {CachePath}", _config.Value.Sm5shMusic.CachePath);
            _logger.LogInformation("Default Locale: {DefaultLocale}", _config.Value.Sm5shMusic.DefaultLocale);


            //Load Music Mods
            _logger.LogInformation("Loading AudioState Service");
            _audioStateService.InitBgmEntriesFromStateManager();
            _logger.LogInformation("Loading Sm5shMusic Mods");
            var musicMods = _musicModManagerService.RefreshMusicMods();

            foreach (var musicMod in musicMods)
            {
                //Add to Audio State Service
                var newBgmEntries = musicMod.GetBgms();
                foreach (var newBgmEntry in newBgmEntries)
                {
                    _audioStateService.AddBgmEntry(newBgmEntry);
                }
            }

            return true;
        }

        public override bool Build(bool useCache)
        {
            _logger.LogInformation("Starting Build...");

            //Persist DB changes
            _audioStateService.SaveBgmEntriesToStateManager();

            //Save NUS3Audio/Nus3Bank
            foreach (var bgmEntry in _audioStateService.GetModBgmEntries())
            {
                var nusBankOutputFile = Path.Combine(_config.Value.OutputPath, "stream;", "sound", "bgm", string.Format(Constants.GameResources.NUS3BANK_FILE, bgmEntry.ToneId));
                var nusAudioOutputFile = Path.Combine(_config.Value.OutputPath, "stream;", "sound", "bgm", string.Format(Constants.GameResources.NUS3AUDIO_FILE, bgmEntry.ToneId));

                //We always generate a new Nus3Bank as the internal ID might change
                _logger.LogInformation("Generating Nus3Bank for {ToneId}", bgmEntry.ToneId);
                _nus3AudioService.GenerateNus3Bank(bgmEntry.ToneId, bgmEntry.NUS3BankConfig.AudioVolume, nusBankOutputFile);

                //Test for audio cache
                _logger.LogInformation("Generating or Copying Nus3Audio for {ToneId}", bgmEntry.ToneId);
                if (!ConvertNus3Audio(useCache, bgmEntry, nusAudioOutputFile))
                    _logger.LogError("Error! The song with ToneId {ToneId}, File {Filename} could not be processed.", bgmEntry.ToneId, bgmEntry.Filename);
            }

            return true;
        }

        private bool ConvertNus3Audio(bool useCache, BgmEntry bgmEntry, string nusAudioOutputFile)
        {
            bool result = false;

            //Test for audio cache
            if (useCache)
            {
                var cachedAudioFile = Path.Combine(_config.Value.Sm5shMusic.CachePath, string.Format(Constants.GameResources.NUS3AUDIO_FILE, bgmEntry.ToneId));
                if (!File.Exists(cachedAudioFile))
                {
                    result = _nus3AudioService.GenerateNus3Audio(bgmEntry.ToneId, bgmEntry.Filename, cachedAudioFile);
                }
                else
                {
                    _logger.LogDebug("Retrieving nus3audio {InternalToneName} from cache {CacheFile}", bgmEntry.ToneId, cachedAudioFile);
                }
                if (File.Exists(cachedAudioFile))
                {
                    _logger.LogDebug("Copy nus3audio {InternalToneName} from cache {CacheFile} to {Nus3AudioOutputFile}", bgmEntry.ToneId, cachedAudioFile, nusAudioOutputFile);
                    File.Copy(cachedAudioFile, nusAudioOutputFile);
                    return true;
                }
            }
            else
            {
                result = _nus3AudioService.GenerateNus3Audio(bgmEntry.ToneId, bgmEntry.Filename, nusAudioOutputFile);
            }

            return result;
        }
    }
}
