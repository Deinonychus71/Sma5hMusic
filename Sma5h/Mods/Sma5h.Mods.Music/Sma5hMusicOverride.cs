using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sma5h.Interfaces;
using Sma5h.Mods.Music.Helpers;
using Sma5h.Mods.Music.Interfaces;
using Sma5h.Mods.Music.Models;
using Sma5h.Mods.Music.MusicMods.MusicModModels;
using Sma5h.Mods.Music.MusicOverride;
using Sma5h.Mods.Music.MusicOverride.MusicOverrideConfigModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sma5h.Mods.Music
{
    public class Sma5hMusicOverride : BaseSma5hMod, ISma5hMusicOverride
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IOptions<Sma5hMusicOverrideOptions> _config;
        private readonly IAudioStateService _audioStateService;
        private const Formatting _defaultFormatting = Formatting.Indented;
        private MusicOverrideConfig _musicOverrideConfig;

        public override string ModName => "Sma5hMusicOverride";

        public Sma5hMusicOverride(IOptions<Sma5hMusicOverrideOptions> config, IStateManager state, IMapper mapper, IAudioStateService audioStateService, ILogger<Sma5hMusicOverride> logger)
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
            _logger.LogInformation("Sma5hMusic Override Path: {MusicModPath}", _config.Value.Sma5hMusicOverride.ModPath);
            _logger.LogInformation("Sma5hMusic Override Version: {Version}", MusicConstants.VersionSma5hMusicOverride);

            //Load Music Override
            _logger.LogInformation("Loading Sma5hMusic Override Config");
            LoadOrCreateMusicOverrideConfig();

            if (_musicOverrideConfig.SoundTestOrder.Count > 0)
                _logger.LogInformation("Overriding SoundTest Order...");

            //Game Override
            //Even mod games can be loaded, as they can be created separately from mods
            if (_musicOverrideConfig.CoreGameOverrides != null)
            {
                var gameTitleEntries = _audioStateService.GetGameTitleEntries().ToDictionary(p => p.UiGameTitleId, p => p);
                foreach (var coreGameTitle in _musicOverrideConfig.CoreGameOverrides.Values)
                {
                    if (gameTitleEntries.ContainsKey(coreGameTitle.UiGameTitleId))
                    {
                        _logger.LogInformation("Overriding Core Game {GameId}...", coreGameTitle.UiGameTitleId);
                        _mapper.Map(coreGameTitle, gameTitleEntries[coreGameTitle.UiGameTitleId]);
                    }
                    else
                    {
                        _audioStateService.AddGameTitleEntry(_mapper.Map(coreGameTitle, new GameTitleEntry(coreGameTitle.UiGameTitleId, EntrySource.Mod)));
                    }
                }
            }

            //Bgm Override
            if (_musicOverrideConfig.CoreBgmOverrides != null)
            {
                //DbRoot
                var coreDbRootOverrides = _musicOverrideConfig.CoreBgmOverrides.CoreBgmDbRootOverrides;
                if (coreDbRootOverrides != null)
                {
                    foreach (var bgmDbRootEntry in _audioStateService.GetBgmDbRootEntries())
                    {
                        if (bgmDbRootEntry.Source == EntrySource.Core && coreDbRootOverrides.ContainsKey(bgmDbRootEntry.UiBgmId))
                        {
                            _mapper.Map(coreDbRootOverrides[bgmDbRootEntry.UiBgmId], bgmDbRootEntry);
                        }
                    }
                }
                //StreamSet
                var coreStreamSetOverrides = _musicOverrideConfig.CoreBgmOverrides.CoreBgmStreamSetOverrides;
                if (coreStreamSetOverrides != null)
                {
                    foreach (var bgmStreamSetEntry in _audioStateService.GetBgmStreamSetEntries())
                    {
                        if (bgmStreamSetEntry.Source == EntrySource.Core && coreStreamSetOverrides.ContainsKey(bgmStreamSetEntry.StreamSetId))
                        {
                            var streamSetObj = GetUpdatedStreamSetConfig(coreStreamSetOverrides[bgmStreamSetEntry.StreamSetId]);
                            _mapper.Map(streamSetObj, bgmStreamSetEntry);
                        }
                    }
                }
                //AssignedInfo
                var coreAssignedInfoOverrides = _musicOverrideConfig.CoreBgmOverrides.CoreBgmAssignedInfoOverrides;
                if (coreAssignedInfoOverrides != null)
                {
                    foreach (var bgmAssignedInfoEntry in _audioStateService.GetBgmAssignedInfoEntries())
                    {
                        if (bgmAssignedInfoEntry.Source == EntrySource.Core && coreAssignedInfoOverrides.ContainsKey(bgmAssignedInfoEntry.InfoId))
                        {
                            var assignedInfoObj = GetUpdatedBgmAssignedInfoConfig(coreAssignedInfoOverrides[bgmAssignedInfoEntry.InfoId]);
                            _mapper.Map(assignedInfoObj, bgmAssignedInfoEntry);
                        }
                    }
                }
                //StreamProperty
                var coreStreamPropertyOverrides = _musicOverrideConfig.CoreBgmOverrides.CoreBgmStreamPropertyOverrides;
                if (coreStreamPropertyOverrides != null)
                {
                    foreach (var bgmStreamPropertyEntry in _audioStateService.GetBgmStreamPropertyEntries())
                    {
                        if (bgmStreamPropertyEntry.Source == EntrySource.Core && coreStreamPropertyOverrides.ContainsKey(bgmStreamPropertyEntry.StreamId))
                        {
                            _mapper.Map(coreStreamPropertyOverrides[bgmStreamPropertyEntry.StreamId], bgmStreamPropertyEntry);
                        }
                    }
                }
                //Property
                var coreBgmPropertyOverrides = _musicOverrideConfig.CoreBgmOverrides.CoreBgmPropertyOverrides;
                if (coreBgmPropertyOverrides != null)
                {
                    foreach (var bgmPropertyEntry in _audioStateService.GetBgmPropertyEntries())
                    {
                        if (bgmPropertyEntry.Source == EntrySource.Core && coreBgmPropertyOverrides.ContainsKey(bgmPropertyEntry.NameId))
                        {
                            _mapper.Map(coreBgmPropertyOverrides[bgmPropertyEntry.NameId], bgmPropertyEntry);
                        }
                    }
                }
            }

            //Sound Test Order
            foreach (var bgmEntry in _audioStateService.GetBgmDbRootEntries())
            {

                if (_musicOverrideConfig.SoundTestOrder.ContainsKey(bgmEntry.UiBgmId))
                    bgmEntry.TestDispOrder = _musicOverrideConfig.SoundTestOrder[bgmEntry.UiBgmId];
            }

            //Playlist Override
            if (_musicOverrideConfig.PlaylistsOverrides != null && _musicOverrideConfig.PlaylistsOverrides.Count > 0)
            {
                _logger.LogInformation("Overriding Playlists...");
                var corePlaylists = _audioStateService.GetPlaylists();
                var dbRootEntries = _audioStateService.GetBgmDbRootEntries().Select(p => p.UiBgmId);

                foreach (var playlistConfig in _musicOverrideConfig.PlaylistsOverrides)
                {
                    var playlist = corePlaylists.FirstOrDefault(p => p.Id == playlistConfig.Key);
                    if (playlist == null)
                    {
                        var newPlaylist = new PlaylistEntry(playlistConfig.Key, playlistConfig.Value.Title);
                        _audioStateService.AddPlaylistEntry(newPlaylist);
                        playlist = newPlaylist;
                    }

                    playlist.Tracks.Clear();
                    foreach (var overrideTrack in playlistConfig.Value.Tracks)
                    {
                        if (dbRootEntries.Contains(overrideTrack.UiBgmId))
                            playlist.Tracks.Add(_mapper.Map<Models.PlaylistEntryModels.PlaylistValueEntry>(overrideTrack));
                        else
                            _logger.LogWarning("Track with BGM ID {BgmId} from Playlist {Playlist} was not found. This song was removed.", overrideTrack.UiBgmId, playlistConfig.Key);
                    }

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
            _musicOverrideConfig = new MusicOverrideConfig();

            //Ensure Directory is created
            Directory.CreateDirectory(_config.Value.Sma5hMusicOverride.ModPath);

            //Override order
            var overrideOrderJsonFile = Path.Combine(_config.Value.Sma5hMusicOverride.ModPath, MusicConstants.MusicModFiles.MUSIC_OVERRIDE_ORDER_JSON_FILE);
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
            var overridePlaylistJsonFile = Path.Combine(_config.Value.Sma5hMusicOverride.ModPath, MusicConstants.MusicModFiles.MUSIC_OVERRIDE_PLAYLIST_JSON_FILE);
            if (File.Exists(overridePlaylistJsonFile))
            {
                var file = File.ReadAllText(overridePlaylistJsonFile);
                _logger.LogDebug("Parsing {MusicOverrideFile} Json File", overridePlaylistJsonFile);
                var outputPlaylist = JsonConvert.DeserializeObject<Dictionary<string, PlaylistConfig>>(file);
                _logger.LogDebug("Parsed {MusicOverrideFile} Json File", overridePlaylistJsonFile);
                _musicOverrideConfig.PlaylistsOverrides = outputPlaylist;
            }
            else
                _logger.LogInformation("File {MusicOverrideFile} does not exist.", overridePlaylistJsonFile);

            //Override Stage
            var overrideStageJsonFile = Path.Combine(_config.Value.Sma5hMusicOverride.ModPath, MusicConstants.MusicModFiles.MUSIC_OVERRIDE_STAGE_JSON_FILE);
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
            var overrideCoreJsonFile = Path.Combine(_config.Value.Sma5hMusicOverride.ModPath, MusicConstants.MusicModFiles.MUSIC_OVERRIDE_CORE_BGM_JSON_FILE);
            if (File.Exists(overrideCoreJsonFile))
            {
                var file = File.ReadAllText(overrideCoreJsonFile);
                _logger.LogDebug("Parsing {MusicOverrideFile} Json File", overrideCoreJsonFile);
                var outputCoreBgm = JsonConvert.DeserializeObject<CoreBgmOverrides>(file);
                _logger.LogDebug("Parsed {MusicOverrideFile} Json File", overrideCoreJsonFile);
                _musicOverrideConfig.CoreBgmOverrides = outputCoreBgm;
            }
            else
                _logger.LogInformation("File {MusicOverrideFile} does not exist.", overrideCoreJsonFile);

            //Override Core Game
            var overrideCoreGameJsonFile = Path.Combine(_config.Value.Sma5hMusicOverride.ModPath, MusicConstants.MusicModFiles.MUSIC_OVERRIDE_CORE_GAME_JSON_FILE);
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

            UpdateOldUnknownValues();
        }

        public bool UpdateSoundTestOrderConfig(Dictionary<string, short> orderEntries)
        {
            _musicOverrideConfig.SoundTestOrder = orderEntries;

            var overrideJsonFile = Path.Combine(_config.Value.Sma5hMusicOverride.ModPath, MusicConstants.MusicModFiles.MUSIC_OVERRIDE_ORDER_JSON_FILE);
            File.WriteAllText(overrideJsonFile, JsonConvert.SerializeObject(_musicOverrideConfig.SoundTestOrder, _defaultFormatting));
            return true;
        }

        public bool UpdateCoreBgmEntries(MusicModEntries musicModEntries)
        {
            if (musicModEntries == null)
                return true;

            if (musicModEntries.BgmDbRootEntries != null)
                foreach (var bgmDbRootEntry in musicModEntries.BgmDbRootEntries)
                    _musicOverrideConfig.CoreBgmOverrides.CoreBgmDbRootOverrides[bgmDbRootEntry.UiBgmId] = _mapper.Map<BgmDbRootConfig>(bgmDbRootEntry);
            if (musicModEntries.BgmStreamSetEntries != null)
                foreach (var bgmStreamSetEntry in musicModEntries.BgmStreamSetEntries)
                    _musicOverrideConfig.CoreBgmOverrides.CoreBgmStreamSetOverrides[bgmStreamSetEntry.StreamSetId] = _mapper.Map<BgmStreamSetConfig>(bgmStreamSetEntry);
            if (musicModEntries.BgmAssignedInfoEntries != null)
                foreach (var bgmAssignedInfoEntry in musicModEntries.BgmAssignedInfoEntries)
                    _musicOverrideConfig.CoreBgmOverrides.CoreBgmAssignedInfoOverrides[bgmAssignedInfoEntry.InfoId] = _mapper.Map<BgmAssignedInfoConfig>(bgmAssignedInfoEntry);
            if (musicModEntries.BgmStreamPropertyEntries != null)
                foreach (var bgmStreamPropertyEntry in musicModEntries.BgmStreamPropertyEntries)
                    _musicOverrideConfig.CoreBgmOverrides.CoreBgmStreamPropertyOverrides[bgmStreamPropertyEntry.StreamId] = _mapper.Map<BgmStreamPropertyConfig>(bgmStreamPropertyEntry);
            if (musicModEntries.BgmPropertyEntries != null)
                foreach (var bgmPropertyEntry in musicModEntries.BgmPropertyEntries)
                    _musicOverrideConfig.CoreBgmOverrides.CoreBgmPropertyOverrides[bgmPropertyEntry.NameId] = _mapper.Map<BgmPropertyEntryConfig>(bgmPropertyEntry);

            var overrideJsonFile = Path.Combine(_config.Value.Sma5hMusicOverride.ModPath, MusicConstants.MusicModFiles.MUSIC_OVERRIDE_CORE_BGM_JSON_FILE);
            File.WriteAllText(overrideJsonFile, JsonConvert.SerializeObject(_musicOverrideConfig.CoreBgmOverrides, _defaultFormatting));
            return true;
        }

        public bool UpdateGameTitleEntry(Models.GameTitleEntry gameTitleEntry)
        {
            _musicOverrideConfig.CoreGameOverrides[gameTitleEntry.UiGameTitleId] = _mapper.Map<GameConfig>(gameTitleEntry);
            var overrideJsonFile = Path.Combine(_config.Value.Sma5hMusicOverride.ModPath, MusicConstants.MusicModFiles.MUSIC_OVERRIDE_CORE_GAME_JSON_FILE);
            File.WriteAllText(overrideJsonFile, JsonConvert.SerializeObject(_musicOverrideConfig.CoreGameOverrides, _defaultFormatting));
            return true;
        }

        public bool DeleteGameTitleEntry(string gameTitleId)
        {
            if (_musicOverrideConfig.CoreGameOverrides.ContainsKey(gameTitleId))
                _musicOverrideConfig.CoreGameOverrides.Remove(gameTitleId);
            var overrideJsonFile = Path.Combine(_config.Value.Sma5hMusicOverride.ModPath, MusicConstants.MusicModFiles.MUSIC_OVERRIDE_CORE_GAME_JSON_FILE);
            File.WriteAllText(overrideJsonFile, JsonConvert.SerializeObject(_musicOverrideConfig.CoreGameOverrides, _defaultFormatting));
            return true;
        }

        public bool UpdatePlaylistConfig(Dictionary<string, PlaylistEntry> playlistEntries)
        {
            _musicOverrideConfig.PlaylistsOverrides = _mapper.Map<Dictionary<string, PlaylistConfig>>(playlistEntries);
            var overrideJsonFile = Path.Combine(_config.Value.Sma5hMusicOverride.ModPath, MusicConstants.MusicModFiles.MUSIC_OVERRIDE_PLAYLIST_JSON_FILE);
            File.WriteAllText(overrideJsonFile, JsonConvert.SerializeObject(_musicOverrideConfig.PlaylistsOverrides, _defaultFormatting));
            return true;
        }

        public bool UpdateMusicStageOverride(List<StageEntry> stageEntries)
        {
            _musicOverrideConfig.StageOverrides = _mapper.Map<Dictionary<string, StageConfig>>(stageEntries.ToDictionary(p => p.UiStageId, p => p));
            var overrideJsonFile = Path.Combine(_config.Value.Sma5hMusicOverride.ModPath, MusicConstants.MusicModFiles.MUSIC_OVERRIDE_STAGE_JSON_FILE);
            File.WriteAllText(overrideJsonFile, JsonConvert.SerializeObject(_musicOverrideConfig.StageOverrides, _defaultFormatting));
            return true;
        }
        
        public bool ResetOverrideFile(string file)
        {
            var overrideJsonFile = Path.Combine(_config.Value.Sma5hMusicOverride.ModPath, file);
            if (File.Exists(overrideJsonFile))
            {
                File.Delete(overrideJsonFile);
                return true;
            }
            return false;
        }

        public BgmStreamSetConfig GetUpdatedStreamSetConfig(BgmStreamSetConfig bgmStreamSetConfig)
        {
            if (!string.IsNullOrEmpty(bgmStreamSetConfig.SpecialCategory) && bgmStreamSetConfig.SpecialCategory.StartsWith("0x") &&
                MusicConstants.SPECIAL_CATEGORY_LABELS.ContainsKey(bgmStreamSetConfig.SpecialCategory))
            {
                bgmStreamSetConfig.SpecialCategory = MusicConstants.SPECIAL_CATEGORY_LABELS[bgmStreamSetConfig.SpecialCategory];
            }
            return bgmStreamSetConfig;
        }

        public BgmAssignedInfoConfig GetUpdatedBgmAssignedInfoConfig(BgmAssignedInfoConfig bgmAssignedInfoConfig)
        {
            if (!string.IsNullOrEmpty(bgmAssignedInfoConfig.Condition) && bgmAssignedInfoConfig.Condition.StartsWith("0x") &&
                MusicConstants.SOUND_CONDITION_LABELS.ContainsKey(bgmAssignedInfoConfig.Condition))
            {
                bgmAssignedInfoConfig.Condition = MusicConstants.SOUND_CONDITION_LABELS[bgmAssignedInfoConfig.Condition];
            }
            if (!string.IsNullOrEmpty(bgmAssignedInfoConfig.ConditionProcess) && bgmAssignedInfoConfig.ConditionProcess.StartsWith("0x") &&
                MusicConstants.SOUND_CONDITION_PROCESS_LABELS.ContainsKey(bgmAssignedInfoConfig.ConditionProcess))
            {
                bgmAssignedInfoConfig.ConditionProcess = MusicConstants.SOUND_CONDITION_PROCESS_LABELS[bgmAssignedInfoConfig.ConditionProcess];
            }
            return bgmAssignedInfoConfig;
        }

        public void UpdateOldUnknownValues()
        {
            //Update old BGM values
            if (_musicOverrideConfig.CoreBgmOverrides != null)
            {
                bool found = false;
                foreach (var bgmBbRootEntry in _musicOverrideConfig.CoreBgmOverrides.CoreBgmDbRootOverrides.Values)
                {
                    CrackedHashValueConverter.UpdateBgmDbRootConfig(bgmBbRootEntry);
                    found = true;
                }
                foreach (var bgmAssignedInfoEntry in _musicOverrideConfig.CoreBgmOverrides.CoreBgmAssignedInfoOverrides.Values)
                {
                    CrackedHashValueConverter.UpdateAssignedInfoConfig(bgmAssignedInfoEntry);
                    found = true;
                }
                if (found)
                {
                    var overrideJsonFile = Path.Combine(_config.Value.Sma5hMusicOverride.ModPath, MusicConstants.MusicModFiles.MUSIC_OVERRIDE_CORE_BGM_JSON_FILE);
                    File.WriteAllText(overrideJsonFile, JsonConvert.SerializeObject(_musicOverrideConfig.CoreBgmOverrides, _defaultFormatting));
                }
            }

            if (_musicOverrideConfig.StageOverrides != null)
            {
                bool found = false;
                foreach (var stageEntry in _musicOverrideConfig.StageOverrides.Values)
                {
                    CrackedHashValueConverter.UpdateStageConfig(stageEntry);
                    found = true;
                }
                if (found)
                {
                    var overrideJsonFile = Path.Combine(_config.Value.Sma5hMusicOverride.ModPath, MusicConstants.MusicModFiles.MUSIC_OVERRIDE_STAGE_JSON_FILE);
                    File.WriteAllText(overrideJsonFile, JsonConvert.SerializeObject(_musicOverrideConfig.StageOverrides, _defaultFormatting));
                }
            }

            
        }
    }
}
