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
                var musicModEntries = musicMod.GetMusicModEntries();
                foreach (var bgmDbRootEntry in musicModEntries.BgmDbRootEntries)
                    _audioStateService.AddBgmDbRootEntry(bgmDbRootEntry);
                foreach (var bgmAssignedInfoEntry in musicModEntries.BgmAssignedInfoEntries)
                    _audioStateService.AddBgmAssignedInfoEntry(bgmAssignedInfoEntry);
                foreach (var bgmStreamSetEntry in musicModEntries.BgmStreamSetEntries)
                    _audioStateService.AddBgmStreamSetEntry(bgmStreamSetEntry);
                foreach (var bgmStreamPropertyEntry in musicModEntries.BgmStreamPropertyEntries)
                    _audioStateService.AddBgmStreamPropertyEntry(bgmStreamPropertyEntry);
                foreach (var gameTitleEntry in musicModEntries.GameTitleEntries)
                    _audioStateService.AddGameTitleEntry(gameTitleEntry);
                foreach (var bgmPropertiesEntry in musicModEntries.BgmPropertyEntries)
                    _audioStateService.AddBgmPropertyEntry(bgmPropertiesEntry);
            }

            return true;
        }

        public override bool Build(bool useCache)
        {
            _logger.LogInformation("Starting Build...");

            //Persist DB changes
            _audioStateService.SaveBgmEntriesToStateManager();

            //Save NUS3Audio/Nus3Bank
            foreach (var bgmPropertyEntry in _audioStateService.GetModBgmPropertyEntries())
            {
                var nusBankOutputFile = Path.Combine(_config.Value.OutputPath, "stream;", "sound", "bgm", string.Format(MusicConstants.GameResources.NUS3BANK_FILE, bgmPropertyEntry.NameId));
                var nusAudioOutputFile = Path.Combine(_config.Value.OutputPath, "stream;", "sound", "bgm", string.Format(MusicConstants.GameResources.NUS3AUDIO_FILE, bgmPropertyEntry.NameId));

                //We always generate a new Nus3Bank as the internal ID might change
                _logger.LogInformation("Generating Nus3Bank for {NameId}", bgmPropertyEntry.NameId);
                _nus3AudioService.GenerateNus3Bank(bgmPropertyEntry.NameId, bgmPropertyEntry.AudioVolume, nusBankOutputFile);

                //Test for audio cache
                _logger.LogInformation("Generating or Copying Nus3Audio for {NameId}", bgmPropertyEntry.NameId);
                if (!ConvertNus3Audio(useCache, bgmPropertyEntry, nusAudioOutputFile))
                    _logger.LogError("Error! The song with ToneId {NameId}, File {Filename} could not be processed.", bgmPropertyEntry.NameId, bgmPropertyEntry.Filename);
            }

            return true;
        }

        private bool ConvertNus3Audio(bool useCache, BgmPropertyEntry bgmPropertyEntry, string nusAudioOutputFile)
        {
            bool result = false;

            //Test for audio cache
            if (useCache)
            {
                var cachedAudioFile = Path.Combine(_config.Value.Sm5shMusic.CachePath, string.Format(MusicConstants.GameResources.NUS3AUDIO_FILE, bgmPropertyEntry.NameId));
                if (!File.Exists(cachedAudioFile))
                {
                    result = _nus3AudioService.GenerateNus3Audio(bgmPropertyEntry.NameId, bgmPropertyEntry.Filename, cachedAudioFile);
                }
                else
                {
                    _logger.LogDebug("Retrieving nus3audio {InternalToneName} from cache {CacheFile}", bgmPropertyEntry.NameId, cachedAudioFile);
                }
                if (File.Exists(cachedAudioFile))
                {
                    _logger.LogDebug("Copy nus3audio {InternalToneName} from cache {CacheFile} to {Nus3AudioOutputFile}", bgmPropertyEntry.NameId, cachedAudioFile, nusAudioOutputFile);
                    File.Copy(cachedAudioFile, nusAudioOutputFile);
                    return true;
                }
            }
            else
            {
                result = _nus3AudioService.GenerateNus3Audio(bgmPropertyEntry.NameId, bgmPropertyEntry.Filename, nusAudioOutputFile);
            }

            return result;
        }
    }
}
