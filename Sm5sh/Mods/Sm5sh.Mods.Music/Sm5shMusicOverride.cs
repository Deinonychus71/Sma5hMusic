using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sm5sh.Interfaces;
using Sm5sh.Mods.Music.Helpers;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.MusicOverride;
using System.Collections.Generic;
using System.IO;

namespace Sm5sh.Mods.Music
{
    public class Sm5shMusicOverride : BaseSm5shMod
    {
        private readonly ILogger _logger;
        private readonly IOptions<Sm5shMusicOverrideOptions> _config;
        private readonly IAudioStateService _audioStateService;
        private MusicOverrideConfig _musicOverrideConfig;

        public override string ModName => "Sm5shMusicOverride";

        public Sm5shMusicOverride(IOptions<Sm5shMusicOverrideOptions> config, IStateManager state, IAudioStateService audioStateService, ILogger<Sm5shMusicOverride> logger)
            : base(state)
        {
            _logger = logger;
            _state = state;
            _config = config;
            _audioStateService = audioStateService;
        }

        public override bool Init()
        {
            _logger.LogInformation("Sm5shMusic Override Path: {MusicModPath}", _config.Value.Sm5shMusicOverride.ModPath);

            //Load Music Override
            _logger.LogInformation("Loading Sm5shMusic Override Config");
            LoadOrCreateMusicOverrideConfig();

            //Load and update BGM
            var shouldProcessPlaylists = _musicOverrideConfig.Playlists != null && _musicOverrideConfig.Playlists.Count > 0;

            foreach (var bgmEntry in _audioStateService.GetBgmEntries())
            {
                //Sound Test Order
                if (_musicOverrideConfig.SoundTestOrder.ContainsKey(bgmEntry.ToneId))
                    bgmEntry.DbRoot.TestDispOrder = _musicOverrideConfig.SoundTestOrder[bgmEntry.ToneId];

                //Playlists
                if (shouldProcessPlaylists)
                {
                    bgmEntry.Playlists.Clear(); //BGM ENTRY DOESN'T NEED THIS INFO
                }
            }

            return true;
        }

        public override bool Build()
        {
            //Persist DB changes
            _audioStateService.SaveBgmEntriesToStateManager();
            return true;
        }

        private void LoadOrCreateMusicOverrideConfig()
        {
            var overrideJsonFile = Path.Combine(_config.Value.Sm5shMusicOverride.ModPath, Constants.MusicModFiles.MUSIC_OVERRIDE_JSON_FILE);
            if (File.Exists(overrideJsonFile))
            {
                var file = File.ReadAllText(overrideJsonFile);
                _logger.LogDebug("Parsing {MusicOverrideFile} Json File", overrideJsonFile);
                var output = JsonConvert.DeserializeObject<MusicOverrideConfig>(file);
                _logger.LogDebug("Parsed {MusicOverrideFile} Json File", overrideJsonFile);
                _musicOverrideConfig = output;
                return;
            }
            else
            {
                //Cannot load music mod
                _logger.LogWarning("File {MusicOverrideFile} does not exist! Attempt to retrieve CSV.", _config.Value.Sm5shMusicOverride.ModPath);

                _musicOverrideConfig = new MusicOverrideConfig()
                {
                    Playlists = new Dictionary<string, List<MusicOverride.MusicOverrideConfigModels.BgmPlaylistConfig>>(),
                    SoundTestOrder = new Dictionary<string, short>()
                };

                if (!SaveMusicOverrideConfig())
                    _logger.LogError("There was an error while trying to save the current music override configuration");
            }
        }

        private bool SaveMusicOverrideConfig()
        {
            var overrideJsonFile = Path.Combine(_config.Value.Sm5shMusicOverride.ModPath, Constants.MusicModFiles.MUSIC_OVERRIDE_JSON_FILE);
            File.WriteAllText(overrideJsonFile, JsonConvert.SerializeObject(_musicOverrideConfig));

            return true;
        }
    }
}
