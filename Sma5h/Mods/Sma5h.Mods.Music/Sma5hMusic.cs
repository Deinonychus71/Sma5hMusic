using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sma5h.Interfaces;
using Sma5h.Mods.Music.Helpers;
using Sma5h.Mods.Music.Interfaces;
using Sma5h.Mods.Music.Models;
using Sma5h.Mods.Music.Models.PlaylistEntryModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sma5h.Mods.Music
{
    public class Sma5hMusic : BaseSma5hMod
    {
        private readonly ILogger _logger;
        private readonly IOptionsMonitor<Sma5hMusicOptions> _config;
        private readonly IAudioStateService _audioStateService;
        private readonly IMusicModManagerService _musicModManagerService;
        private readonly INus3AudioService _nus3AudioService;

        public override string ModName => "Sma5hMusic";

        public Sma5hMusic(IOptionsMonitor<Sma5hMusicOptions> config, IMusicModManagerService musicModManagerService, IAudioStateService audioStateService,
            INus3AudioService nus3AudioService, IStateManager state, ILogger<Sma5hMusic> logger)
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
            _logger.LogInformation("Sma5hMusic Path: {MusicModPath}", _config.CurrentValue.Sma5hMusic.ModPath);
            _logger.LogInformation("Sma5hMusic Version: {Version}", MusicConstants.VersionSma5hMusic);
            _logger.LogInformation("Audio Conversion Format: {AudioConversionFormat}", _config.CurrentValue.Sma5hMusic.AudioConversionFormat);
            _logger.LogInformation("Resources Path: {ResourcesPath}", _config.CurrentValue.Sma5hMusic.EnableAudioCaching ? "Enabled - If songs are mismatched try to clear the cache!" : "Disabled");
            _logger.LogInformation("Cache Path: {CachePath}", _config.CurrentValue.Sma5hMusic.CachePath);
            _logger.LogInformation("Default Locale: {DefaultLocale}", _config.CurrentValue.Sma5hMusic.DefaultLocale);


            //Load Music Mods
            _logger.LogInformation("Loading AudioState Service");
            _audioStateService.InitBgmEntriesFromStateManager();
            _logger.LogInformation("Loading Sma5hMusic Mods");
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
                foreach (var seriesEntry in musicModEntries.SeriesEntries)
                    _audioStateService.AddSeriesEntry(seriesEntry);
                foreach (var gameTitleEntry in musicModEntries.GameTitleEntries)
                    _audioStateService.AddGameTitleEntry(gameTitleEntry);
                foreach (var bgmPropertiesEntry in musicModEntries.BgmPropertyEntries)
                    _audioStateService.AddBgmPropertyEntry(bgmPropertiesEntry);
            }

            return true;
        }

        public override string BuildPreCheck()
        {
            try
            {
                //Checks
                CheckBuildSpecialCategory();
                return string.Empty;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public override bool Build(bool useCache)
        {
            _logger.LogInformation("Starting Build...");

            //AutoAddToBgmSelector - To Optimize :-)
            ProcessPlaylistAutoMapping();

            //Persist DB changes
            _audioStateService.SaveBgmEntriesToStateManager();

            if (useCache)
                Directory.CreateDirectory(_config.CurrentValue.Sma5hMusic.CachePath);

            //Save NUS3Audio/Nus3Bank
            foreach (var bgmPropertyEntry in _audioStateService.GetModBgmPropertyEntries())
            {
                var nusBankOutputFile = Path.Combine(_config.CurrentValue.OutputPath, "stream;", "sound", "bgm", string.Format(MusicConstants.GameResources.NUS3BANK_FILE, bgmPropertyEntry.NameId));
                var nusAudioOutputFile = Path.Combine(_config.CurrentValue.OutputPath, "stream;", "sound", "bgm", string.Format(MusicConstants.GameResources.NUS3AUDIO_FILE, bgmPropertyEntry.NameId));

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
                var cachedAudioFile = Path.Combine(_config.CurrentValue.Sma5hMusic.CachePath, string.Format(MusicConstants.GameResources.NUS3AUDIO_FILE, bgmPropertyEntry.NameId));
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

        private bool ProcessPlaylistAutoMapping()
        {
            var generationMode = _config.CurrentValue.Sma5hMusic.PlaylistMapping.GenerationMode;

            //AutoAddToBgmSelector - To Optimize :-)
            if (generationMode == Sma5hMusicOptions.PlaylistGeneration.OnlyMissingSongs || generationMode == Sma5hMusicOptions.PlaylistGeneration.AllSongs)
            {
                var configIncidence = _config.CurrentValue.Sma5hMusic.PlaylistMapping.AutoMappingIncidence;
                var configMapping = _config.CurrentValue.Sma5hMusic.PlaylistMapping.AutoMapping.ToDictionary(p => p.Key, p => p.Value != null ? p.Value.Split(',', System.StringSplitOptions.RemoveEmptyEntries) : new string[0]);
                var playlists = _audioStateService.GetPlaylists().ToDictionary(p => p.Id, p => p);

                IEnumerable<BgmDbRootEntry> songsToProcess = null;
                if (generationMode == Sma5hMusicOptions.PlaylistGeneration.AllSongs)
                {
                    //Select all visible songs + clear all tracks
                    var allAffectedPlaylists = configMapping.Keys.ToHashSet();
                    playlists.Where(p => allAffectedPlaylists.Contains(p.Key)).ToList().ForEach(p => p.Value.Tracks.Clear());
                    songsToProcess = _audioStateService.GetBgmDbRootEntries().Where(p => p.TestDispOrder >= 0).OrderBy(p => p.TestDispOrder);
                }
                else if (generationMode == Sma5hMusicOptions.PlaylistGeneration.OnlyMissingSongs)
                {
                    //Select all visible modded songs not in playlist
                    var allModSongInPlaylists = playlists.Values.SelectMany(p => p.Tracks.Select(p2 => p2.UiBgmId)).Distinct();
                    songsToProcess = _audioStateService.GetBgmDbRootEntries().Where(p => p.TestDispOrder >= 0 && p.MusicMod != null && !allModSongInPlaylists.Contains(p.UiBgmId)).OrderBy(p => p.TestDispOrder);
                }

                //Get series from BGM
                var gameToSeries = _audioStateService.GetGameTitleEntries().ToDictionary(p => p.UiGameTitleId, p => p.UiSeriesId);

                //Enumerate playlist
                foreach (var configMappingPlaylist in configMapping)
                {
                    var mappingPlaylist = configMappingPlaylist.Key;
                    var bgmplaylist = playlists.ContainsKey(mappingPlaylist) ? playlists[mappingPlaylist] : null;

                    if (bgmplaylist != null)
                    {
                        var songsToProcessPlaylist = songsToProcess.ToList();

                        //Enumerate mapping
                        foreach (var configMappingEntry in configMappingPlaylist.Value)
                        {
                            var configMappingEntryTrim = configMappingEntry.Trim().ToLower();

                            //Filter songs per series/game
                            IEnumerable<BgmDbRootEntry> songsToProcessMapping = null;
                            if (configMappingEntryTrim.StartsWith(MusicConstants.InternalIds.SERIES_ID_PREFIX))
                            {
                                songsToProcessMapping = songsToProcessPlaylist.Where(p => p.UiGameTitleId != null && gameToSeries.ContainsKey(p.UiGameTitleId) && configMappingEntryTrim == gameToSeries[p.UiGameTitleId]);
                            }
                            else if (configMappingEntryTrim.StartsWith(MusicConstants.InternalIds.GAME_TITLE_ID_PREFIX))
                            {
                                songsToProcessMapping = songsToProcessPlaylist.Where(p => p.UiGameTitleId != null && configMappingEntryTrim == p.UiGameTitleId);
                            }
                            else if (configMappingEntryTrim.StartsWith(MusicConstants.InternalIds.UI_BGM_ID_PREFIX))
                            {
                                songsToProcessMapping = songsToProcessPlaylist.Where(p => p.UiBgmId != null && configMappingEntryTrim == p.UiBgmId);
                            }
                            else
                            {
                                _logger.LogWarning($"Playlist Auto-Mapping: Mapping Entry {configMappingEntryTrim} is invalid. Skipping...");
                                continue;
                            }

                            //Add to playlist
                            foreach (var songToProcessMapping in songsToProcessMapping)
                            {
                                songToProcessMapping.IsSelectableOriginal = true;
                                bgmplaylist.Tracks.Add(new PlaylistValueEntry()
                                {
                                    UiBgmId = songToProcessMapping.UiBgmId,
                                    Incidence0 = configIncidence,
                                    Incidence1 = configIncidence,
                                    Incidence2 = configIncidence,
                                    Incidence3 = configIncidence,
                                    Incidence4 = configIncidence,
                                    Incidence5 = configIncidence,
                                    Incidence6 = configIncidence,
                                    Incidence7 = configIncidence,
                                    Incidence8 = configIncidence,
                                    Incidence9 = configIncidence,
                                    Incidence10 = configIncidence,
                                    Incidence11 = configIncidence,
                                    Incidence12 = configIncidence,
                                    Incidence13 = configIncidence,
                                    Incidence14 = configIncidence,
                                    Incidence15 = configIncidence,
                                    Order0 = bgmplaylist.Tracks.Count > 0 ? (short)(bgmplaylist.Tracks.Max(p => p.Order0) + 1) : (short)0,
                                    Order1 = bgmplaylist.Tracks.Count > 0 ? (short)(bgmplaylist.Tracks.Max(p => p.Order1) + 1) : (short)0,
                                    Order2 = bgmplaylist.Tracks.Count > 0 ? (short)(bgmplaylist.Tracks.Max(p => p.Order2) + 1) : (short)0,
                                    Order3 = bgmplaylist.Tracks.Count > 0 ? (short)(bgmplaylist.Tracks.Max(p => p.Order3) + 1) : (short)0,
                                    Order4 = bgmplaylist.Tracks.Count > 0 ? (short)(bgmplaylist.Tracks.Max(p => p.Order4) + 1) : (short)0,
                                    Order5 = bgmplaylist.Tracks.Count > 0 ? (short)(bgmplaylist.Tracks.Max(p => p.Order5) + 1) : (short)0,
                                    Order6 = bgmplaylist.Tracks.Count > 0 ? (short)(bgmplaylist.Tracks.Max(p => p.Order6) + 1) : (short)0,
                                    Order7 = bgmplaylist.Tracks.Count > 0 ? (short)(bgmplaylist.Tracks.Max(p => p.Order7) + 1) : (short)0,
                                    Order8 = bgmplaylist.Tracks.Count > 0 ? (short)(bgmplaylist.Tracks.Max(p => p.Order8) + 1) : (short)0,
                                    Order9 = bgmplaylist.Tracks.Count > 0 ? (short)(bgmplaylist.Tracks.Max(p => p.Order9) + 1) : (short)0,
                                    Order10 = bgmplaylist.Tracks.Count > 0 ? (short)(bgmplaylist.Tracks.Max(p => p.Order10) + 1) : (short)0,
                                    Order11 = bgmplaylist.Tracks.Count > 0 ? (short)(bgmplaylist.Tracks.Max(p => p.Order11) + 1) : (short)0,
                                    Order12 = bgmplaylist.Tracks.Count > 0 ? (short)(bgmplaylist.Tracks.Max(p => p.Order12) + 1) : (short)0,
                                    Order13 = bgmplaylist.Tracks.Count > 0 ? (short)(bgmplaylist.Tracks.Max(p => p.Order13) + 1) : (short)0,
                                    Order14 = bgmplaylist.Tracks.Count > 0 ? (short)(bgmplaylist.Tracks.Max(p => p.Order14) + 1) : (short)0,
                                    Order15 = bgmplaylist.Tracks.Count > 0 ? (short)(bgmplaylist.Tracks.Max(p => p.Order15) + 1) : (short)0
                                });
                                _logger.LogInformation("Playlist Auto-Mapping: Added BGM {UiBgmId} to Playlist {BgmPlaylist}.", songToProcessMapping.UiBgmId, mappingPlaylist);
                            }

                            //Remove processed songs from this playlist
                            songsToProcessPlaylist.RemoveAll(p => songsToProcessMapping.Contains(p));
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Playlist Auto-Mapping: Playlist {PlaylistId} wasn't found. Skipping...", bgmplaylist);
                    }
                }

                //Check for empty playlists
                if (generationMode == Sma5hMusicOptions.PlaylistGeneration.AllSongs)
                {
                    //Select all visible songs + clear all tracks
                    var allAffectedPlaylists = configMapping.Values.SelectMany(p => p).Distinct().ToHashSet();
                    var emptyPlaylist = playlists.Where(p => allAffectedPlaylists.Contains(p.Key) && p.Value.Tracks.Count == 0).ToList();
                    if (emptyPlaylist != null && emptyPlaylist.Count > 0)
                        throw new Exception($"Playlist '{emptyPlaylist[0]}' had no tracks after running playlist auto-mapping. This could cause an issue in game");
                }
            }

            return true;
        }

        private void CheckBuildSpecialCategory()
        {
            var dbRoots = _audioStateService.GetBgmDbRootEntries();
            var streamSets = _audioStateService.GetBgmStreamSetEntries();
            var infoEntries = _audioStateService.GetBgmAssignedInfoEntries().Select(p => p.InfoId).ToHashSet();
            bool passed = true;
            var messages = new List<string>();
            foreach (var streamSet in streamSets)
            {
                if (!string.IsNullOrEmpty(streamSet.Info1) && !infoEntries.Contains(streamSet.Info1))
                {
                    var dbRootEntry = dbRoots.FirstOrDefault(p => p.StreamSetId == streamSet.StreamSetId);
                    var newMessage = $"'{dbRootEntry?.Title["us_en"]}' ({streamSet.StreamSetId}) references '{streamSet.Info1}' in its special category field, but the reference seems broken.\rThis can happen if the '{streamSet.Info1}' was renamed or removed.";
                    _logger.LogError(newMessage);
                    messages.Add(newMessage);
                    passed = false;
                }
            }
            if (!passed)
                throw new Exception($"Issues were found when running a precheck on the build:\r\r{string.Join("\r\r-", messages)}\r\rPlease check the logs for more information.");
        }
    }
}
