using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sma5h.Data.Ui.Param.Database.PrcUiBgmDatabaseModels;
using Sma5h.Interfaces;
using Sma5h.Mods.Music.Helpers;
using Sma5h.Mods.Music.Interfaces;
using Sma5h.Mods.Music.Models;
using Sma5h.Mods.Music.Models.PlaylistEntryModels;
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
            //AutoAddToBgmSelector - To Optimize :-)
            if (_config.CurrentValue.Sma5hMusic.PlaylistMapping.Enabled)
            {
                var configIncidence = _config.CurrentValue.Sma5hMusic.PlaylistMapping.Incidence;
                var configMapping = _config.CurrentValue.Sma5hMusic.PlaylistMapping.Mapping;
                var gameToSeries = _audioStateService.GetGameTitleEntries().ToDictionary(p => p.UiGameTitleId, p => p.UiSeriesId);
                var playlists = _audioStateService.GetPlaylists().ToDictionary(p => p.Id, p => p);
                var allModSongInPlaylists = playlists.Values.SelectMany(p => p.Tracks.Select(p2 => p2.UiBgmId)).Distinct();
                var allModSongs = _audioStateService.GetBgmDbRootEntries().Where(p => p.TestDispOrder >= 0 && p.MusicMod != null && !allModSongInPlaylists.Contains(p.UiBgmId)).OrderBy(p => p.TestDispOrder);
                foreach (var modSong in allModSongs)
                {
                    var seriesId = gameToSeries.ContainsKey(modSong.UiGameTitleId) && gameToSeries[modSong.UiGameTitleId] != null ? gameToSeries[modSong.UiGameTitleId] : string.Empty;
                    if (!string.IsNullOrWhiteSpace(seriesId))
                    {
                        var mappingConfig = configMapping.ContainsKey(seriesId) ? configMapping[seriesId].Split(',', System.StringSplitOptions.RemoveEmptyEntries) : null;
                        if (mappingConfig != null && mappingConfig.Length > 0)
                        {
                            foreach (var mappingPlaylist in mappingConfig) {
                                var bgmplaylist = playlists.Values.Where(p => p.Id == mappingPlaylist).FirstOrDefault();
                                if (bgmplaylist != null)
                                {
                                    short i = (short)bgmplaylist.Tracks.Max(p => p.Order0);
                                    bgmplaylist.Tracks.Add(new PlaylistValueEntry()
                                    {
                                        UiBgmId = modSong.UiBgmId,
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
                                        Order0 = bgmplaylist.Tracks.Max(p => p.Order0),
                                        Order1 = bgmplaylist.Tracks.Max(p => p.Order1),
                                        Order2 = bgmplaylist.Tracks.Max(p => p.Order2),
                                        Order3 = bgmplaylist.Tracks.Max(p => p.Order3),
                                        Order4 = bgmplaylist.Tracks.Max(p => p.Order4),
                                        Order5 = bgmplaylist.Tracks.Max(p => p.Order5),
                                        Order6 = bgmplaylist.Tracks.Max(p => p.Order6),
                                        Order7 = bgmplaylist.Tracks.Max(p => p.Order7),
                                        Order8 = bgmplaylist.Tracks.Max(p => p.Order8),
                                        Order9 = bgmplaylist.Tracks.Max(p => p.Order9),
                                        Order10 = bgmplaylist.Tracks.Max(p => p.Order10),
                                        Order11 = bgmplaylist.Tracks.Max(p => p.Order11),
                                        Order12 = bgmplaylist.Tracks.Max(p => p.Order12),
                                        Order13 = bgmplaylist.Tracks.Max(p => p.Order13),
                                        Order14 = bgmplaylist.Tracks.Max(p => p.Order14),
                                        Order15 = bgmplaylist.Tracks.Max(p => p.Order15)
                                    });
                                    _logger.LogInformation("Playlist Auto-Mapping: Added BGM {UiBgmId} to Playlist {BgmPlaylist}.", modSong.UiBgmId, mappingPlaylist);
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}
