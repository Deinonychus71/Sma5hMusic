using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sm5sh.Interfaces;
using Sm5sh.Mods.Music.Helpers;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Models;
using Sm5sh.Mods.Music.MusicMods.AdvancedMusicModModels;
using Sm5sh.Mods.Music.MusicOverride;
using Sm5sh.Mods.Music.MusicOverride.MusicOverrideConfigModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sm5sh.Mods.Music
{
    public class Sm5shMusicOverride : BaseSm5shMod, ISm5shMusicOverride
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IOptions<Sm5shMusicOverrideOptions> _config;
        private readonly IAudioStateService _audioStateService;
        private const Formatting _defaultFormatting = Formatting.Indented;
        private MusicOverrideConfig _musicOverrideConfig;

        public override string ModName => "Sm5shMusicOverride";

        public Sm5shMusicOverride(IOptions<Sm5shMusicOverrideOptions> config, IStateManager state, IMapper mapper, IAudioStateService audioStateService, ILogger<Sm5shMusicOverride> logger)
            : base(state)
        {
            _logger = logger;
            _state = state;
            _mapper = mapper;
            _config = config;
            _audioStateService = audioStateService;
        }

        public override bool Init()
        {
            _logger.LogInformation("Sm5shMusic Override Path: {MusicModPath}", _config.Value.Sm5shMusicOverride.ModPath);

            //Load Music Override
            _logger.LogInformation("Loading Sm5shMusic Override Config");
            LoadOrCreateMusicOverrideConfig();

            if(_musicOverrideConfig.SoundTestOrder.Count > 0)
                _logger.LogInformation("Overriding SoundTest Order...");

            foreach (var bgmEntry in _audioStateService.GetBgmEntries())
            {
                //Sound Test Order
                if (_musicOverrideConfig.SoundTestOrder.ContainsKey(bgmEntry.ToneId))
                    bgmEntry.DbRoot.TestDispOrder = _musicOverrideConfig.SoundTestOrder[bgmEntry.ToneId];

                //Replace Core Files / Delete Core
                if (bgmEntry.Source == EntrySource.Core && _musicOverrideConfig.CoreBgmOverrides.ContainsKey(bgmEntry.ToneId))
                {
                    _logger.LogInformation("Overriding Core Song {ToneId}...", bgmEntry.ToneId);
                    var bgmConfig = _musicOverrideConfig.CoreBgmOverrides[bgmEntry.ToneId];
                    if (bgmConfig.IsDeleted)
                        _audioStateService.RemoveBgmEntry(bgmConfig.ToneId);
                    else
                    {
                        var filename = bgmEntry.Filename; //Temporary file while we need filename
                        _mapper.Map(_musicOverrideConfig.CoreBgmOverrides[bgmEntry.ToneId], bgmEntry);
                        bgmEntry.Filename = filename;
                    }
                }
            }

            //Playlist Override
            if(_musicOverrideConfig.Playlists != null && _musicOverrideConfig.Playlists.Count > 0)
            {
                _logger.LogInformation("Overriding Playlists...");
                foreach (var playlistEntry in _audioStateService.GetPlaylists())
                    _audioStateService.RemovePlaylistEntry(playlistEntry.Id);

                foreach (var playlistConfig in _musicOverrideConfig.Playlists)
                {
                    var newPlaylist = new PlaylistEntry(playlistConfig.Key, playlistConfig.Value.Title);
                    foreach(var overrideTrack in playlistConfig.Value.Tracks)
                    {
                        newPlaylist.Tracks.Add(_mapper.Map<Models.PlaylistEntryModels.PlaylistValueEntry>(overrideTrack));
                    }
                    _audioStateService.AddPlaylistEntry(newPlaylist);
                }
            }

            //Stage Override
            if (_musicOverrideConfig.StageOverrides != null && _musicOverrideConfig.StageOverrides.Count > 0)
            {
                _logger.LogInformation("Overriding Stage Playlists...");
                foreach (var stageEntry in _audioStateService.GetStagesEntries())
                {
                    if (_musicOverrideConfig.StageOverrides.ContainsKey(stageEntry.UiStageId))
                        _mapper.Map(_musicOverrideConfig.StageOverrides[stageEntry.UiStageId], stageEntry);
                }
            }

            //Game Override
            if (_musicOverrideConfig.CoreGameOverrides != null)
            {
                foreach (var gameTitleEntry in _audioStateService.GetGameTitleEntries())
                {
                    if (_musicOverrideConfig.CoreGameOverrides.ContainsKey(gameTitleEntry.UiGameTitleId))
                    {
                        _logger.LogInformation("Overriding Core Game {GameId}...", gameTitleEntry.UiGameTitleId);
                        _mapper.Map(_musicOverrideConfig.CoreGameOverrides[gameTitleEntry.UiGameTitleId], gameTitleEntry);
                    }
                }
            }

            return true;
        }

        public override bool Build(bool useCache)
        {
            //Persist DB changes
            //_audioStateService.SaveBgmEntriesToStateManager();
            return true;
        }

        private void LoadOrCreateMusicOverrideConfig()
        {
            _musicOverrideConfig = new MusicOverrideConfig()
            {
                CoreBgmOverrides = new Dictionary<string, BgmConfig>(),
                Playlists = new Dictionary<string, PlaylistConfig>(),
                SoundTestOrder = new Dictionary<string, short>(),
                StageOverrides = new Dictionary<string, StageConfig>(),
                CoreGameOverrides = new Dictionary<string, GameConfig>()
            };

            //Override order
            var overrideOrderJsonFile = Path.Combine(_config.Value.Sm5shMusicOverride.ModPath, Constants.MusicModFiles.MUSIC_OVERRIDE_ORDER_JSON_FILE);
            if (File.Exists(overrideOrderJsonFile))
            {
                var file = File.ReadAllText(overrideOrderJsonFile);
                _logger.LogDebug("Parsing {MusicOverrideFile} Json File", overrideOrderJsonFile);
                var outputOrder = JsonConvert.DeserializeObject<Dictionary<string, short>>(file);
                _logger.LogDebug("Parsed {MusicOverrideFile} Json File", overrideOrderJsonFile);
                _musicOverrideConfig.SoundTestOrder = outputOrder;
            }
            else
                _logger.LogInformation("File {MusicOverrideFile} does not exist.", overrideOrderJsonFile);

            //Override Playlist
            var overridePlaylistJsonFile = Path.Combine(_config.Value.Sm5shMusicOverride.ModPath, Constants.MusicModFiles.MUSIC_OVERRIDE_PLAYLIST_JSON_FILE);
            if (File.Exists(overridePlaylistJsonFile))
            {
                var file = File.ReadAllText(overridePlaylistJsonFile);
                _logger.LogDebug("Parsing {MusicOverrideFile} Json File", overridePlaylistJsonFile);
                var outputPlaylist = JsonConvert.DeserializeObject<Dictionary<string, PlaylistConfig>>(file);
                _logger.LogDebug("Parsed {MusicOverrideFile} Json File", overridePlaylistJsonFile);
                _musicOverrideConfig.Playlists = outputPlaylist;
            }
            else
                _logger.LogInformation("File {MusicOverrideFile} does not exist.", overridePlaylistJsonFile);

            //Override Stage
            var overrideStageJsonFile = Path.Combine(_config.Value.Sm5shMusicOverride.ModPath, Constants.MusicModFiles.MUSIC_OVERRIDE_STAGE_JSON_FILE);
            if (File.Exists(overrideStageJsonFile))
            {
                var file = File.ReadAllText(overrideStageJsonFile);
                _logger.LogDebug("Parsing {MusicOverrideFile} Json File", overrideStageJsonFile);
                var outputStage = JsonConvert.DeserializeObject<Dictionary<string, StageConfig>>(file);
                _logger.LogDebug("Parsed {MusicOverrideFile} Json File", overrideStageJsonFile);
                _musicOverrideConfig.StageOverrides = outputStage;
            }
            else
                _logger.LogInformation("File {MusicOverrideFile} does not exist.", overrideStageJsonFile);

            //Override Core Bgm
            var overrideCoreJsonFile = Path.Combine(_config.Value.Sm5shMusicOverride.ModPath, Constants.MusicModFiles.MUSIC_OVERRIDE_CORE_BGM_JSON_FILE);
            if (File.Exists(overrideCoreJsonFile))
            {
                var file = File.ReadAllText(overrideCoreJsonFile);
                _logger.LogDebug("Parsing {MusicOverrideFile} Json File", overrideCoreJsonFile);
                var outputCoreBgm = JsonConvert.DeserializeObject<Dictionary<string, BgmConfig>>(file);
                _logger.LogDebug("Parsed {MusicOverrideFile} Json File", overrideCoreJsonFile);
                _musicOverrideConfig.CoreBgmOverrides = outputCoreBgm;
            }
            else
                _logger.LogInformation("File {MusicOverrideFile} does not exist.", overrideCoreJsonFile);

            //Override Core Game
            var overrideCoreGameJsonFile = Path.Combine(_config.Value.Sm5shMusicOverride.ModPath, Constants.MusicModFiles.MUSIC_OVERRIDE_CORE_GAME_JSON_FILE);
            if (File.Exists(overrideCoreGameJsonFile))
            {
                var file = File.ReadAllText(overrideCoreGameJsonFile);
                _logger.LogDebug("Parsing {MusicOverrideFile} Json File", overrideCoreGameJsonFile);
                var outputCoreGame = JsonConvert.DeserializeObject<Dictionary<string, GameConfig>>(file);
                _logger.LogDebug("Parsed {MusicOverrideFile} Json File", overrideCoreGameJsonFile);
                _musicOverrideConfig.CoreGameOverrides = outputCoreGame;
            }
            else
                _logger.LogInformation("File {MusicOverrideFile} does not exist.", overrideCoreGameJsonFile);
        }


        public bool UpdateSoundTestOrderConfig(Dictionary<string, short> orderEntries)
        {
            _musicOverrideConfig.SoundTestOrder = orderEntries;

            var overrideJsonFile = Path.Combine(_config.Value.Sm5shMusicOverride.ModPath, Constants.MusicModFiles.MUSIC_OVERRIDE_ORDER_JSON_FILE);
            File.WriteAllText(overrideJsonFile, JsonConvert.SerializeObject(_musicOverrideConfig.SoundTestOrder, _defaultFormatting));
            return true;
        }

        public bool UpdateCoreBgmEntry(Models.BgmEntry bgmEntry)
        {
            _musicOverrideConfig.CoreBgmOverrides[bgmEntry.ToneId] = _mapper.Map<BgmConfig>(bgmEntry);
            var overrideJsonFile = Path.Combine(_config.Value.Sm5shMusicOverride.ModPath, Constants.MusicModFiles.MUSIC_OVERRIDE_CORE_BGM_JSON_FILE);
            File.WriteAllText(overrideJsonFile, JsonConvert.SerializeObject(_musicOverrideConfig.CoreBgmOverrides, _defaultFormatting));
            return true;
        }

        public bool RemoveCoreBgmEntry(string toneId)
        {
            _musicOverrideConfig.CoreBgmOverrides[toneId] = new BgmConfig() { ToneId = toneId, IsDeleted = true };
            var overrideJsonFile = Path.Combine(_config.Value.Sm5shMusicOverride.ModPath, Constants.MusicModFiles.MUSIC_OVERRIDE_CORE_BGM_JSON_FILE);
            File.WriteAllText(overrideJsonFile, JsonConvert.SerializeObject(_musicOverrideConfig.CoreBgmOverrides, _defaultFormatting));
            return true;
        }

        public bool UpdateCoreGameTitleEntry(Models.GameTitleEntry gameTitleEntry)
        {
            _musicOverrideConfig.CoreGameOverrides[gameTitleEntry.UiGameTitleId] = _mapper.Map<GameConfig>(gameTitleEntry);
            var overrideJsonFile = Path.Combine(_config.Value.Sm5shMusicOverride.ModPath, Constants.MusicModFiles.MUSIC_OVERRIDE_CORE_GAME_JSON_FILE);
            File.WriteAllText(overrideJsonFile, JsonConvert.SerializeObject(_musicOverrideConfig.CoreGameOverrides, _defaultFormatting));
            return true;
        }

        public bool UpdatePlaylistConfig(Dictionary<string, PlaylistEntry> playlistEntries)
        {
            _musicOverrideConfig.Playlists = _mapper.Map<Dictionary<string, PlaylistConfig>>(playlistEntries);
            var overrideJsonFile = Path.Combine(_config.Value.Sm5shMusicOverride.ModPath, Constants.MusicModFiles.MUSIC_OVERRIDE_PLAYLIST_JSON_FILE);
            File.WriteAllText(overrideJsonFile, JsonConvert.SerializeObject(_musicOverrideConfig.Playlists, _defaultFormatting));
            return true;
        }

        public bool UpdateMusicStageOverride(List<StageEntry> stageEntries)
        {
            _musicOverrideConfig.StageOverrides = _mapper.Map<Dictionary<string, StageConfig>>(stageEntries.ToDictionary(p => p.UiStageId, p => p));
            var overrideJsonFile = Path.Combine(_config.Value.Sm5shMusicOverride.ModPath, Constants.MusicModFiles.MUSIC_OVERRIDE_STAGE_JSON_FILE);
            File.WriteAllText(overrideJsonFile, JsonConvert.SerializeObject(_musicOverrideConfig.StageOverrides, _defaultFormatting));
            return true;
        }
    }
}
