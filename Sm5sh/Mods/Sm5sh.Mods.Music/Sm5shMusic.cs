using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sm5sh.Interfaces;
using Sm5sh.Mods.Music.Helpers;
using Sm5sh.Mods.Music.Interfaces;
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
            _logger.LogInformation("Loading Sm5shMusic Mods");
            var musicMods = _musicModManagerService.RefreshMusicMods();

            foreach (var musicMod in musicMods)
            {
                //Add to Audio State Service
                var newBgmEntries = musicMod.GetBgms();
                foreach(var newBgmEntry in newBgmEntries)
                {
                    _audioStateService.AddBgmEntry(newBgmEntry);
                }
            }

            return true;
        }

        public override bool Build()
        {
            //Persist DB changes
            _audioStateService.SaveBgmEntriesToStateManager();

            //Save NUS3Audio/Nus3Bank
            foreach (var bgmEntry in _audioStateService.GetModBgmEntries())
            {
                var nusBankOutputFile = Path.Combine(_config.Value.OutputPath, "stream;", "sound", "bgm", string.Format(Constants.GameResources.NUS3BANK_FILE, bgmEntry.ToneId));
                var nusAudioOutputFile = Path.Combine(_config.Value.OutputPath, "stream;", "sound", "bgm", string.Format(Constants.GameResources.NUS3AUDIO_FILE, bgmEntry.ToneId));
                
                //We always generate a new Nus3Bank as the internal ID might change
                _nus3AudioService.GenerateNus3Bank(bgmEntry.ToneId, nusBankOutputFile);

                //Test for audio cache
                if (_config.Value.Sm5shMusic.EnableAudioCaching)
                {
                    var cachedAudioFile = Path.Combine(_config.Value.Sm5shMusic.CachePath, string.Format(Constants.GameResources.NUS3AUDIO_FILE, bgmEntry.ToneId));
                    if (!File.Exists(cachedAudioFile))
                    {
                        _nus3AudioService.GenerateNus3Audio(bgmEntry.ToneId, bgmEntry.Filename, cachedAudioFile);
                    }
                    else
                    {
                        _logger.LogDebug("Retrieving nus3audio {InternalToneName} from cache {CacheFile}", bgmEntry.ToneId, cachedAudioFile);
                    }
                    _logger.LogDebug("Copy nus3audio {InternalToneName} from cache {CacheFile} to {Nus3AudioOutputFile}", bgmEntry.ToneId, cachedAudioFile, nusAudioOutputFile);
                    File.Copy(cachedAudioFile, nusAudioOutputFile);
                }
                else
                {
                    _nus3AudioService.GenerateNus3Audio(bgmEntry.ToneId, bgmEntry.Filename, nusAudioOutputFile);
                }
            }

            return true;
        }
    }
}
