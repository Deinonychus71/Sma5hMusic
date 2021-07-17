using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using Force.Crc32;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sma5h.Data;
using Sma5h.Data.Ui.Param.Database;
using Sma5h.Data.Ui.Param.Database.PrcUiBgmDatabaseModels;
using Sma5h.Data.Ui.Param.Database.PrcUiGameTitleDatabaseModels;
using Sma5h.Data.Ui.Param.Database.PrcUiSeriesDatabaseModels;
using Sma5h.Data.Ui.Param.Database.PrcUiStageDatabaseModels;
using Sma5h.Helpers;
using Sma5h.Interfaces;
using Sma5h.Mods.Music.Helpers;
using Sma5h.Mods.Music.Interfaces;
using Sma5h.Mods.Music.Models;
using Sma5h.ResourceProviders.Constants;
using Sma5h.ResourceProviders.Prc.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sma5h.Mods.Music.Services
{
    public class AudioStateService : IAudioStateService
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IStateManager _state;
        private readonly IOptionsMonitor<Sma5hMusicOptions> _config;
        private readonly HashSet<string> _localesEntries;
        private readonly Dictionary<string, SeriesEntry> _seriesEntries;
        private readonly Dictionary<string, GameTitleEntry> _gameTitleEntries;
        private readonly Dictionary<string, BgmDbRootEntry> _bgmDbRootEntries;
        private readonly Dictionary<string, BgmStreamSetEntry> _bgmStreamSetEntries;
        private readonly Dictionary<string, BgmAssignedInfoEntry> _bgmAssignedInfoEntries;
        private readonly Dictionary<string, BgmStreamPropertyEntry> _bgmStreamPropertyEntries;
        private readonly Dictionary<string, BgmPropertyEntry> _bgmPropertyEntries;
        //private readonly Dictionary<string, BgmDbRootEntry> _deletedBgmEntries; //TODO
        private readonly Dictionary<string, PlaylistEntry> _playlistsEntries;
        private readonly Dictionary<string, StageEntry> _stageEntries;
        private readonly Dictionary<string, float> _coreVolumes;

        public double GameVersion { get; private set; }

        public AudioStateService(IOptionsMonitor<Sma5hMusicOptions> config, IMapper mapper, IStateManager state, ILogger<IAudioStateService> logger)
        {
            _config = config;
            _mapper = mapper;
            _logger = logger;
            _state = state;
            //_deletedBgmEntries = new Dictionary<string, BgmDbRootEntry>();
            _seriesEntries = new Dictionary<string, SeriesEntry>();
            _gameTitleEntries = new Dictionary<string, GameTitleEntry>();
            _localesEntries = new HashSet<string>();
            _bgmDbRootEntries = new Dictionary<string, BgmDbRootEntry>();
            _bgmStreamSetEntries = new Dictionary<string, BgmStreamSetEntry>();
            _bgmAssignedInfoEntries = new Dictionary<string, BgmAssignedInfoEntry>();
            _bgmStreamPropertyEntries = new Dictionary<string, BgmStreamPropertyEntry>();
            _bgmPropertyEntries = new Dictionary<string, BgmPropertyEntry>();
            _playlistsEntries = new Dictionary<string, PlaylistEntry>();
            _stageEntries = new Dictionary<string, StageEntry>();
            _coreVolumes = GetCoreNus3BankVolumes();
        }

        #region GET
        public IEnumerable<BgmDbRootEntry> GetBgmDbRootEntries()
        {
            return _bgmDbRootEntries.Values;
        }

        public IEnumerable<BgmDbRootEntry> GetModBgmDbRootEntries()
        {
            return GetBgmDbRootEntries().Where(p => p.Source == EntrySource.Mod);
        }

        public IEnumerable<BgmStreamSetEntry> GetBgmStreamSetEntries()
        {
            return _bgmStreamSetEntries.Values;
        }

        public IEnumerable<BgmStreamSetEntry> GetModBgmStreamSetEntries()
        {
            return GetBgmStreamSetEntries().Where(p => p.Source == EntrySource.Mod);
        }

        public IEnumerable<BgmAssignedInfoEntry> GetBgmAssignedInfoEntries()
        {
            return _bgmAssignedInfoEntries.Values;
        }

        public IEnumerable<BgmAssignedInfoEntry> GetModBgmAssignedInfoEntries()
        {
            return GetBgmAssignedInfoEntries().Where(p => p.Source == EntrySource.Mod);
        }

        public IEnumerable<BgmStreamPropertyEntry> GetBgmStreamPropertyEntries()
        {
            return _bgmStreamPropertyEntries.Values;
        }

        public IEnumerable<BgmStreamPropertyEntry> GetModBgmStreamPropertyEntries()
        {
            return GetBgmStreamPropertyEntries().Where(p => p.Source == EntrySource.Mod);
        }

        public IEnumerable<BgmPropertyEntry> GetBgmPropertyEntries()
        {
            return _bgmPropertyEntries.Values;
        }

        public IEnumerable<BgmPropertyEntry> GetModBgmPropertyEntries()
        {
            return GetBgmPropertyEntries().Where(p => p.Source == EntrySource.Mod);
        }

        public IEnumerable<SeriesEntry> GetSeriesEntries()
        {
            return _seriesEntries.Values;
        }

        public IEnumerable<GameTitleEntry> GetGameTitleEntries()
        {
            return _gameTitleEntries.Values;
        }

        public IEnumerable<StageEntry> GetStagesEntries()
        {
            return _stageEntries.Values;
        }

        public IEnumerable<string> GetLocales()
        {
            return _localesEntries;
        }

        public IEnumerable<PlaylistEntry> GetPlaylists()
        {
            return _playlistsEntries.Values;
        }
        #endregion

        #region CAN ADD
        public bool CanAddBgmDbRootEntry(string uiBgmId)
        {
            return !_bgmDbRootEntries.ContainsKey(uiBgmId);
        }

        public bool CanAddBgmStreamSetEntry(string streamSetId)
        {
            return !_bgmStreamSetEntries.ContainsKey(streamSetId);
        }

        public bool CanAddBgmAssignedInfoEntry(string infoId)
        {
            return !_bgmAssignedInfoEntries.ContainsKey(infoId);
        }

        public bool CanAddBgmStreamPropertyEntry(string streamId)
        {
            return !_bgmStreamPropertyEntries.ContainsKey(streamId);
        }

        public bool CanAddBgmPropertyEntry(string nameId)
        {
            return !_bgmPropertyEntries.ContainsKey(nameId);
        }

        public bool CanAddSeriesEntry(string uiSeriesId)
        {
            return !_seriesEntries.ContainsKey(uiSeriesId);
        }

        public bool CanAddGameTitleEntry(string uiGameTitleId)
        {
            return !_gameTitleEntries.ContainsKey(uiGameTitleId);
        }
        #endregion

        #region ADD
        public bool AddBgmDbRootEntry(BgmDbRootEntry bgmDbRootEntry)
        {
            if (bgmDbRootEntry.UiBgmId.Length > MusicConstants.GameResources.DbRootIdMaximumSize ||
                bgmDbRootEntry.UiBgmId.Length < MusicConstants.GameResources.DbRootIdMinimumSize)
            {
                _logger.LogError("The DBRoot ID {DBRootId} is either too long or too short. Minimum: {DbRootIdMinimumSize}, Maximum: {DbRootIdMaximumSize}",
                    bgmDbRootEntry.UiBgmId, MusicConstants.GameResources.DbRootIdMinimumSize, MusicConstants.GameResources.DbRootIdMaximumSize);
                return false;
            }

            //TODO: Figure out how MenuValue works - Incrementing for now
            bgmDbRootEntry.MenuValue = _bgmDbRootEntries.Values.OrderByDescending(p => p.MenuValue).First().MenuValue + 1; //TODO: Treat separately

            if (CanAddBgmDbRootEntry(bgmDbRootEntry.UiBgmId))
                _bgmDbRootEntries.Add(bgmDbRootEntry.UiBgmId, bgmDbRootEntry);
            else
            {
                _logger.LogError("BgmDbRootEntry with UiBgmId {UiBgmId} already exist in the database.", bgmDbRootEntry.UiBgmId);
                return false;
            }

            return true;
        }

        public bool AddBgmStreamSetEntry(BgmStreamSetEntry bgmStreamSetEntry)
        {
            if (bgmStreamSetEntry.StreamSetId.Length > MusicConstants.GameResources.StreamSetIdMaximumSize ||
                bgmStreamSetEntry.StreamSetId.Length < MusicConstants.GameResources.StreamSetIdMinimumSize)
            {
                _logger.LogError("The StreamSet ID {StreamSetId} is either too long or too short. Minimum: {StreamSetIdMinimumSize}, Maximum: {StreamSetIdMaximumSize}",
                    bgmStreamSetEntry.StreamSetId, MusicConstants.GameResources.StreamSetIdMinimumSize, MusicConstants.GameResources.StreamSetIdMaximumSize);
                return false;
            }

            if (CanAddBgmStreamSetEntry(bgmStreamSetEntry.StreamSetId))
                _bgmStreamSetEntries.Add(bgmStreamSetEntry.StreamSetId, bgmStreamSetEntry);
            else
            {
                _logger.LogError("BgmStreamSetEntry with StreamSetId {StreamSetId} already exist in the database.", bgmStreamSetEntry.StreamSetId);
                return false;
            }

            return true;
        }

        public bool AddBgmAssignedInfoEntry(BgmAssignedInfoEntry bgmAssignedInfoEntry)
        {
            if (bgmAssignedInfoEntry.InfoId.Length > MusicConstants.GameResources.AssignedInfoIdMaximumSize ||
                bgmAssignedInfoEntry.InfoId.Length < MusicConstants.GameResources.AssignedInfoIdMinimumSize)
            {
                _logger.LogError("The AssignedInfo ID {InfoId} is either too long or too short. Minimum: {AssignedInfoIdMinimumSize}, Maximum: {AssignedInfoIdMaximumSize}",
                    bgmAssignedInfoEntry.InfoId, MusicConstants.GameResources.AssignedInfoIdMinimumSize, MusicConstants.GameResources.AssignedInfoIdMaximumSize);
                return false;
            }

            if (CanAddBgmAssignedInfoEntry(bgmAssignedInfoEntry.InfoId))
                _bgmAssignedInfoEntries.Add(bgmAssignedInfoEntry.InfoId, bgmAssignedInfoEntry);
            else
            {
                _logger.LogError("BgmAssignedInfoEntry with InfoId {InfoId} already exist in the database.", bgmAssignedInfoEntry.InfoId);
                return false;
            }

            return true;
        }

        public bool AddBgmStreamPropertyEntry(BgmStreamPropertyEntry bgmStreamPropertyEntry)
        {
            if (bgmStreamPropertyEntry.StreamId.Length > MusicConstants.GameResources.StreamIdMaximumSize ||
                bgmStreamPropertyEntry.StreamId.Length < MusicConstants.GameResources.StreamIdMinimumSize)
            {
                _logger.LogError("The Stream ID {StreamId} is either too long or too short. Minimum: {StreamIdMinimumSize}, Maximum: {StreamIdMaximumSize}",
                    bgmStreamPropertyEntry.StreamId, MusicConstants.GameResources.StreamIdMinimumSize, MusicConstants.GameResources.StreamIdMaximumSize);
                return false;
            }

            if (CanAddBgmStreamPropertyEntry(bgmStreamPropertyEntry.StreamId))
                _bgmStreamPropertyEntries.Add(bgmStreamPropertyEntry.StreamId, bgmStreamPropertyEntry);
            else
            {
                _logger.LogError("BgmStreamPropertyEntry with InfoId {StreamId} already exist in the database.", bgmStreamPropertyEntry.StreamId);
                return false;
            }

            return true;
        }

        public bool AddBgmPropertyEntry(BgmPropertyEntry bgmPropertyEntry)
        {
            if (bgmPropertyEntry.NameId.Length > MusicConstants.GameResources.ToneIdMaximumSize ||
                bgmPropertyEntry.NameId.Length < MusicConstants.GameResources.ToneIdMinimumSize)
            {
                _logger.LogError("The Name ID {NameId} is either too long or too short. Minimum: {ToneIdMinimumSize}, Maximum: {ToneIdMaximumSize}",
                    bgmPropertyEntry.NameId, MusicConstants.GameResources.ToneIdMinimumSize, MusicConstants.GameResources.ToneIdMaximumSize);
                return false;
            }

            if (CanAddBgmPropertyEntry(bgmPropertyEntry.NameId))
                _bgmPropertyEntries.Add(bgmPropertyEntry.NameId, bgmPropertyEntry);
            else
            {
                _logger.LogError("BgmPropertyEntry with InfoId {NameId} already exist in the database.", bgmPropertyEntry.NameId);
                return false;
            }

            return true;
        }

        public bool AddSeriesEntry(SeriesEntry seriesEntry)
        {
            if (seriesEntry.UiSeriesId.Length > MusicConstants.GameResources.SeriesMaximumSize ||
                seriesEntry.UiSeriesId.Length < MusicConstants.GameResources.SeriesMinimumSize)
            {
                _logger.LogError("The Series ID {SeriesId} is either too long or too short. Minimum: {SeriesMinimumSize}, Maximum: {SeriesMaximumSize}",
                    seriesEntry.UiSeriesId, MusicConstants.GameResources.SeriesMinimumSize, MusicConstants.GameResources.SeriesMaximumSize);
                return false;
            }

            //Save Series
            if (!_seriesEntries.ContainsKey(seriesEntry.UiSeriesId))
                _seriesEntries.Add(seriesEntry.UiSeriesId, seriesEntry);
            //It is very well possible that the series already exists.

            return true;
        }

        public bool AddGameTitleEntry(GameTitleEntry gameTitleEntry)
        {
            if (gameTitleEntry.UiGameTitleId.Length > MusicConstants.GameResources.GameTitleMaximumSize ||
                gameTitleEntry.UiGameTitleId.Length < MusicConstants.GameResources.GameTitleMinimumSize)
            {
                _logger.LogError("The Game Title ID {GameTitleId} is either too long or too short. Minimum: {GameTitleMinimumValue}, Maximum: {GameTitleMaximumValue}",
                    gameTitleEntry.UiGameTitleId, MusicConstants.GameResources.GameTitleMinimumSize, MusicConstants.GameResources.GameTitleMaximumSize);
                return false;
            }

            if (!_seriesEntries.ContainsKey(gameTitleEntry.UiSeriesId))
            {
                _logger.LogError("The Game Title ID {GameTitleId} requires Series {SeriesId} but it doesn't seem to exist.",
                   gameTitleEntry.UiGameTitleId, gameTitleEntry.UiSeriesId);
                return false;
            }

            //Save GameTitle
            if (!_gameTitleEntries.ContainsKey(gameTitleEntry.UiGameTitleId))
                _gameTitleEntries.Add(gameTitleEntry.UiGameTitleId, gameTitleEntry);
            //It is very well possible that the game already exists.

            return true;
        }

        public bool AddPlaylistEntry(PlaylistEntry playlistEntry)
        {
            if (!_playlistsEntries.ContainsKey(playlistEntry.Id))
            {
                _playlistsEntries.Add(playlistEntry.Id, playlistEntry);
            }
            else
            {
                _logger.LogWarning("PlaylistId {PlaylistId} already exist... ", playlistEntry.Id);
            }

            return true;
        }
        #endregion

        #region DELETE
        public bool RemoveBgmDbRootEntry(string uiBgmId)
        {
            if (_bgmDbRootEntries.ContainsKey(uiBgmId))
            {
                //if (!_deletedBgmEntries.ContainsKey(toneId))
                //    _deletedBgmEntries.Add(toneId, _bgmEntries[toneId]);
                _bgmDbRootEntries.Remove(uiBgmId);
            }
            else
            {
                _logger.LogWarning("UiBgmId {UiBgmId} was not found. Cannot remove from list...", uiBgmId);
            }
            return true;
        }

        public bool RemoveBgmStreamSetEntry(string streamSetId)
        {
            if (_bgmStreamSetEntries.ContainsKey(streamSetId))
            {
                _bgmStreamSetEntries.Remove(streamSetId);
            }
            else
            {
                _logger.LogWarning("StreamSetId {StreamSetId} was not found. Cannot remove from list...", streamSetId);
            }
            return true;
        }

        public bool RemoveBgmAssignedInfoEntry(string infoId)
        {
            if (_bgmAssignedInfoEntries.ContainsKey(infoId))
            {
                _bgmAssignedInfoEntries.Remove(infoId);
            }
            else
            {
                _logger.LogWarning("InfoId {InfoId} was not found. Cannot remove from list...", infoId);
            }
            return true;
        }

        public bool RemoveBgmStreamPropertyEntry(string streamId)
        {
            if (_bgmStreamPropertyEntries.ContainsKey(streamId))
            {
                _bgmStreamPropertyEntries.Remove(streamId);
            }
            else
            {
                _logger.LogWarning("StreamId {StreamId} was not found. Cannot remove from list...", streamId);
            }
            return true;
        }

        public bool RemoveBgmPropertyEntry(string nameId)
        {
            if (_bgmPropertyEntries.ContainsKey(nameId))
            {
                _bgmPropertyEntries.Remove(nameId);
            }
            else
            {
                _logger.LogWarning("NameId {NameId} was not found. Cannot remove from list...", nameId);
            }
            return true;
        }

        public bool RemoveGameTitleEntry(string uiGameTitleId)
        {
            if (_gameTitleEntries.ContainsKey(uiGameTitleId))
            {
                _gameTitleEntries.Remove(uiGameTitleId);
            }
            else
            {
                _logger.LogWarning("UiGameTitleId {UiGameTitleId} was not found. Cannot remove from list...", uiGameTitleId);
            }
            return true;
        }

        public bool RemovePlaylistEntry(string playlistId)
        {
            if (_playlistsEntries.ContainsKey(playlistId))
            {
                _playlistsEntries.Remove(playlistId);
            }
            else
            {
                _logger.LogWarning("PlaylistId {PlaylistId} was not found. Cannot remove from list...", playlistId);
            }

            return true;
        }
        #endregion

        public bool SaveBgmEntriesToStateManager()
        {
            _logger.LogInformation("Saving Bgm Entries to State Service");

            //Load Data
            var paramBgmDatabase = _state.LoadResource<PrcUiBgmDatabase>(PrcExtConstants.PRC_UI_BGM_DB_PATH);
            var paramSeriesDbRoot = _state.LoadResource<PrcUiSeriesDatabase>(PrcExtConstants.PRC_UI_SERIES_DB_PATH).DbRootEntries;
            var paramGameTitleDatabaseRoot = _state.LoadResource<PrcUiGameTitleDatabase>(PrcExtConstants.PRC_UI_GAMETITLE_DB_PATH).DbRootEntries;
            var paramStageDbRoot = _state.LoadResource<PrcUiStageDatabase>(PrcExtConstants.PRC_UI_STAGE_DB_PATH).DbRootEntries;
            var binBgmPropertyEntries = _state.LoadResource<Data.Sound.Config.BinBgmProperty>(BgmPropertyFileConstants.BGM_PROPERTY_PATH).Entries;
            var daoMsbtBgms = GetBgmDatabases();
            var daoMsbtTitle = GetGameTitleDatabases();

            var defaultLocale = _config.CurrentValue.Sma5hMusic.DefaultLocale; //TODO: Remove? It should now be handled from UI
            var coreSeriesGames = paramGameTitleDatabaseRoot.Values.Select(p => p.UiSeriesId).Distinct(); //Not handling series addition right now.

            //Series PRC - We don't delete existing series... yet.
            foreach (var series in _seriesEntries.Values)
            {
                //Ensure that the game needs to be added - If no song is using the game, there is no need to compile it.
                if (series.Source == EntrySource.Mod)
                {
                    if (_gameTitleEntries.Values.Count(p => p.UiSeriesId == series.UiSeriesId) == 0)
                        continue;
                }

                paramSeriesDbRoot[series.UiSeriesId] = _mapper.Map<PrcSeriesDbRootEntry>(series);

                if (!string.IsNullOrEmpty(series?.NameId))
                {
                    var seriesLabel = series.MSBTTitleKey;
                    var titleDict = series.MSBTTitle;
                    foreach (var msbtDb in daoMsbtTitle)
                    {
                        var entries = msbtDb.Value.Entries;
                        if (titleDict.ContainsKey(msbtDb.Key))
                            entries[seriesLabel] = titleDict[msbtDb.Key];
                        else if (titleDict.ContainsKey(defaultLocale))
                            entries[seriesLabel] = titleDict[defaultLocale];
                    }
                }
            }

            //GameTitle PRC - We don't delete existing games... yet.
            foreach (var gameTitle in _gameTitleEntries.Values)
            {
                //Ensure that the game needs to be added - If no song is using the game, there is no need to compile it.
                if (gameTitle.Source == EntrySource.Mod)
                {
                    if (_bgmDbRootEntries.Values.Count(p =>
                    p.UiGameTitleId == gameTitle.UiGameTitleId ||
                    p.UiGameTitleId1 == gameTitle.UiGameTitleId ||
                    p.UiGameTitleId2 == gameTitle.UiGameTitleId ||
                    p.UiGameTitleId3 == gameTitle.UiGameTitleId ||
                    p.UiGameTitleId4 == gameTitle.UiGameTitleId) == 0)
                        continue;
                }

                paramGameTitleDatabaseRoot[gameTitle.UiGameTitleId] = _mapper.Map<PrcGameTitleDbRootEntry>(gameTitle);

                if (!string.IsNullOrEmpty(gameTitle?.NameId))
                {
                    var gameTitleLabel = gameTitle.MSBTTitleKey;
                    var titleDict = gameTitle.MSBTTitle;
                    foreach (var msbtDb in daoMsbtTitle)
                    {
                        var entries = msbtDb.Value.Entries;
                        if (titleDict.ContainsKey(msbtDb.Key))
                            entries[gameTitleLabel] = titleDict[msbtDb.Key];
                        else if (titleDict.ContainsKey(defaultLocale))
                            entries[gameTitleLabel] = titleDict[defaultLocale];
                    }
                }
            }

            //DbRoot Entries Saving
            //Reordering
            short orderIndex = 0;
            var listBgms = _bgmDbRootEntries.Values.Where(p => p.TestDispOrder >= 0).OrderBy(p => p.TestDispOrder);
            foreach (var bgmEntry in listBgms)
            {
                bgmEntry.TestDispOrder = orderIndex;
                orderIndex++;

                //If the song needs to be display, SaveNo has to be between 0 and last SaveNo known value
                if (bgmEntry.Source == EntrySource.Mod)
                    bgmEntry.SaveNo = 0;

            }
            foreach (var bgmDbRootEntry in _bgmDbRootEntries.Values)
            {
                var uiBgmId = bgmDbRootEntry.UiBgmId;

                //Generate NameId - If needed
                if (bgmDbRootEntry.Source != EntrySource.Core && bgmDbRootEntry.ContainsValidLabels)
                    bgmDbRootEntry.NameId = GetNewNameId();

                //Save Bin & BGM PRC
                paramBgmDatabase.DbRootEntries[bgmDbRootEntry.UiBgmId] = _mapper.Map<PrcBgmDbRootEntry>(bgmDbRootEntry);

                //Save MSBT - If needed
                #region
                if (!string.IsNullOrEmpty(bgmDbRootEntry.NameId))
                {
                    var titleLabel = bgmDbRootEntry.TitleKey;
                    var titleDict = bgmDbRootEntry.Title;
                    var authorLabel = bgmDbRootEntry.AuthorKey;
                    var authorDict = bgmDbRootEntry.Author;
                    var copyrightLabel = bgmDbRootEntry.CopyrightKey;
                    var copyrightDict = bgmDbRootEntry.Copyright;
                    foreach (var msbtDb in daoMsbtBgms)
                    {
                        var entries = msbtDb.Value.Entries;

                        //Title
                        if (titleDict != null && titleDict.ContainsKey(msbtDb.Key) && !string.IsNullOrEmpty(titleDict[msbtDb.Key]))
                            entries[titleLabel] = ConvertToGameTextTag(titleDict[msbtDb.Key]);
                        else if (titleDict != null && titleDict.ContainsKey(defaultLocale) && !string.IsNullOrEmpty(titleDict[defaultLocale]))
                            entries[titleLabel] = ConvertToGameTextTag(titleDict[defaultLocale]);

                        //Author
                        if (authorDict != null)
                        {
                            if (authorDict.ContainsKey(msbtDb.Key) && !string.IsNullOrEmpty(authorDict[msbtDb.Key]))
                                entries[authorLabel] = authorDict[msbtDb.Key];
                            else if (authorDict.ContainsKey(defaultLocale) && !string.IsNullOrEmpty(authorDict[defaultLocale]))
                                entries[authorLabel] = authorDict[defaultLocale];
                        }

                        //Copyright
                        if (copyrightDict != null)
                        {
                            if (copyrightDict.ContainsKey(msbtDb.Key) && !string.IsNullOrEmpty(copyrightDict[msbtDb.Key]))
                                entries[copyrightLabel] = copyrightDict[msbtDb.Key];
                            else if (copyrightDict.ContainsKey(defaultLocale) && !string.IsNullOrEmpty(copyrightDict[defaultLocale]))
                                entries[copyrightLabel] = copyrightDict[defaultLocale];
                        }
                    }
                }
                #endregion 
            }

            //StreamSet Entries Saving
            foreach (var bgmStreamSetEntry in _bgmStreamSetEntries.Values)
            {
                paramBgmDatabase.StreamSetEntries[bgmStreamSetEntry.StreamSetId] = _mapper.Map<PrcBgmStreamSetEntry>(bgmStreamSetEntry);
            }

            //AssignedInfo Entries Saving
            foreach (var bgmAssignedInfoEntry in _bgmAssignedInfoEntries.Values)
            {
                paramBgmDatabase.AssignedInfoEntries[bgmAssignedInfoEntry.InfoId] = _mapper.Map<PrcBgmAssignedInfoEntry>(bgmAssignedInfoEntry);
            }

            //StreamProperty Entries Saving
            foreach (var bgmStreamPropertyEntry in _bgmStreamPropertyEntries.Values)
            {
                paramBgmDatabase.StreamPropertyEntries[bgmStreamPropertyEntry.StreamId] = _mapper.Map<PrcBgmStreamPropertyEntry>(bgmStreamPropertyEntry);
            }

            //BgmProperty Entries Saving
            foreach (var bgmPropertyEntry in _bgmPropertyEntries.Values)
            {
                binBgmPropertyEntries[bgmPropertyEntry.NameId] = _mapper.Map<Data.Sound.Config.BgmPropertyStructs.BgmPropertyEntry>(bgmPropertyEntry);
            }

            //Playlists
            paramBgmDatabase.PlaylistEntries.Clear(); //Wiping everything :)
            foreach (var playlist in _playlistsEntries)
            {
                var tracks = new List<PrcBgmPlaylistEntry>();
                paramBgmDatabase.PlaylistEntries.Add(new PcrFilterStruct<PrcBgmPlaylistEntry>()
                {
                    Id = playlist.Key,
                    Values = tracks
                });

                foreach (var track in playlist.Value.Tracks)
                {
                    if (_bgmDbRootEntries.ContainsKey(track.UiBgmId))
                        tracks.Add(_mapper.Map<PrcBgmPlaylistEntry>(track));
                    else
                        _logger.LogWarning("The track with BGM ID {UiBgmId}", track.UiBgmId);
                }
            }

            //Mapping stage
            paramStageDbRoot.Clear();
            foreach (var stage in _stageEntries)
            {
                paramStageDbRoot.Add(stage.Key, _mapper.Map<StageDbRootEntry>(stage.Value));
            }

            return true;
        }

        #region Private
        public void InitBgmEntriesFromStateManager()
        {
            GuessGameVersion();

            //Make sure resources are unloaded
            _state.UnloadResources();
            _bgmDbRootEntries.Clear();
            _bgmStreamSetEntries.Clear();
            _bgmAssignedInfoEntries.Clear();
            _bgmStreamPropertyEntries.Clear();
            _bgmPropertyEntries.Clear();
            //_deletedBgmEntries.Clear();
            _gameTitleEntries.Clear();
            _seriesEntries.Clear();
            _localesEntries.Clear();
            _playlistsEntries.Clear();
            _stageEntries.Clear();

            //Load Data
            var gameResourcePath = _config.CurrentValue.GameResourcesPath;
            var daoBinBgmProperty = _state.LoadResource<Data.Sound.Config.BinBgmProperty>(BgmPropertyFileConstants.BGM_PROPERTY_PATH);
            var paramBgmDatabase = _state.LoadResource<PrcUiBgmDatabase>(PrcExtConstants.PRC_UI_BGM_DB_PATH);
            var paramGameTitleDbRoot = _state.LoadResource<PrcUiGameTitleDatabase>(PrcExtConstants.PRC_UI_GAMETITLE_DB_PATH).DbRootEntries;
            var paramStageDbRoot = _state.LoadResource<PrcUiStageDatabase>(PrcExtConstants.PRC_UI_STAGE_DB_PATH).DbRootEntries;
            var paramSeriesDbRoot = _state.LoadResource<PrcUiSeriesDatabase>(PrcExtConstants.PRC_UI_SERIES_DB_PATH).DbRootEntries;
            var daoMsbtBgms = GetBgmDatabases();
            var daoMsbtTitle = GetGameTitleDatabases();
            daoMsbtBgms.Keys.ToList().ForEach(p => _localesEntries.Add(p));

            //Map DbRoot
            foreach (var paramDbRootEntry in paramBgmDatabase.DbRootEntries.Values)
            {
                var uiBgmId = paramDbRootEntry.UiBgmId;
                var bgmDbRootEntry = new BgmDbRootEntry(paramDbRootEntry.UiBgmId);
                _bgmDbRootEntries.Add(uiBgmId, _mapper.Map(paramDbRootEntry, bgmDbRootEntry));

                //Mapping MSBT
                if (!string.IsNullOrEmpty(bgmDbRootEntry.NameId))
                {
                    var titleLabel = bgmDbRootEntry.TitleKey;
                    var authorLabel = bgmDbRootEntry.AuthorKey;
                    var copyrightLabel = bgmDbRootEntry.CopyrightKey;
                    foreach (var msbtDb in daoMsbtBgms)
                    {
                        var entries = msbtDb.Value.Entries;
                        if (entries.ContainsKey(titleLabel))
                            bgmDbRootEntry.Title.Add(msbtDb.Key, ConvertFromGameTextTag(entries[titleLabel]));
                        if (entries.ContainsKey(authorLabel))
                            bgmDbRootEntry.Author.Add(msbtDb.Key, entries[authorLabel]);
                        if (entries.ContainsKey(copyrightLabel))
                            bgmDbRootEntry.Copyright.Add(msbtDb.Key, entries[copyrightLabel]);
                    }
                }
            }

            //Map StreamSet
            foreach (var paramStreamSetEntry in paramBgmDatabase.StreamSetEntries.Values)
            {
                _bgmStreamSetEntries.Add(paramStreamSetEntry.StreamSetId, _mapper.Map(paramStreamSetEntry, new BgmStreamSetEntry(paramStreamSetEntry.StreamSetId)));
            }

            //Map AssignedInfo
            foreach (var paramAssignedInfoEntry in paramBgmDatabase.AssignedInfoEntries.Values)
            {
                _bgmAssignedInfoEntries.Add(paramAssignedInfoEntry.InfoId, _mapper.Map(paramAssignedInfoEntry, new BgmAssignedInfoEntry(paramAssignedInfoEntry.InfoId)));
            }

            //Map StreamProperty
            foreach (var paramStreamPropertyEntry in paramBgmDatabase.StreamPropertyEntries.Values)
            {
                _bgmStreamPropertyEntries.Add(paramStreamPropertyEntry.StreamId, _mapper.Map(paramStreamPropertyEntry, new BgmStreamPropertyEntry(paramStreamPropertyEntry.StreamId)));
            }

            //Map BinProperty
            foreach (var binBgnProperty in daoBinBgmProperty.Entries.Values)
            {
                var filename = Path.Combine(gameResourcePath, "stream;", "sound", "bgm", string.Format(MusicConstants.GameResources.NUS3AUDIO_FILE, binBgnProperty.NameId));
                var newBgmPropertyEntry = new BgmPropertyEntry(binBgnProperty.NameId, filename);
                if (_coreVolumes.ContainsKey(newBgmPropertyEntry.NameId))
                    newBgmPropertyEntry.AudioVolume = _coreVolumes[newBgmPropertyEntry.NameId];
                _bgmPropertyEntries.Add(binBgnProperty.NameId, _mapper.Map(binBgnProperty, newBgmPropertyEntry));
            }

            //Mapping series
            foreach (var dbRootSeriesEntry in paramSeriesDbRoot.Values)
            {
                var seriesId = dbRootSeriesEntry.UiSeriesId;
                var seriesEntry = new SeriesEntry(seriesId);
                _seriesEntries.Add(seriesId, _mapper.Map(dbRootSeriesEntry, seriesEntry));

                //MSBT
                if (!string.IsNullOrEmpty(seriesEntry?.NameId) && seriesEntry.MSBTTitle.Count == 0) //Test for cache
                {
                    var seriesLabel = seriesEntry.MSBTTitleKey;
                    foreach (var msbtDb in daoMsbtTitle)
                    {
                        var entries = msbtDb.Value.Entries;
                        if (entries.ContainsKey(seriesLabel))
                            seriesEntry.MSBTTitle.Add(msbtDb.Key, entries[seriesLabel]);
                    }
                }
            }

            //Mapping games
            foreach (var dbRootGameEntry in paramGameTitleDbRoot.Values)
            {
                var gameTitleId = dbRootGameEntry.UiGameTitleId;
                var gameEntry = new GameTitleEntry(gameTitleId);
                _gameTitleEntries.Add(gameTitleId, _mapper.Map(dbRootGameEntry, gameEntry));

                if (!_seriesEntries.ContainsKey(gameEntry.UiSeriesId))
                    throw new Exception($"A series entry '{gameEntry.UiSeriesId}' is referenced in game '{gameTitleId}' but does not seem to exist.");

                //MSBT
                if (!string.IsNullOrEmpty(gameEntry?.NameId) && gameEntry.MSBTTitle.Count == 0) //Test for cache
                {
                    var gameTitleLabel = gameEntry.MSBTTitleKey;
                    foreach (var msbtDb in daoMsbtTitle)
                    {
                        var entries = msbtDb.Value.Entries;
                        if (entries.ContainsKey(gameTitleLabel))
                            gameEntry.MSBTTitle.Add(msbtDb.Key, entries[gameTitleLabel]);
                    }
                }
            }

            //Mapping playlists
            foreach (var paramPlaylist in paramBgmDatabase.PlaylistEntries)
            {
                var newPlaylist = new PlaylistEntry(paramPlaylist.Id);
                _playlistsEntries.Add(paramPlaylist.Id, newPlaylist);

                foreach (var track in paramPlaylist.Values)
                {
                    newPlaylist.Tracks.Add(_mapper.Map<Models.PlaylistEntryModels.PlaylistValueEntry>(track));
                }
            }

            //Mapping stage
            foreach (var stage in paramStageDbRoot)
            {
                _stageEntries.Add(stage.Key, _mapper.Map<StageEntry>(stage.Value));
            }
        }

        public void GuessGameVersion()
        {
            var gameResourcePath = _config.CurrentValue.GameResourcesPath;
            var gameCrcSets = GameResourcesCrcHelper.VersionCrcSets;
            GameVersion = 0.0;
            foreach (var versionCrcSet in gameCrcSets)
            {
                bool allMatch = true;
                foreach (var resource in versionCrcSet.CrcResources)
                {
                    var file = Path.Combine(gameResourcePath, resource.Key);
                    if (File.Exists(file))
                    {
                        var hash = Crc32Algorithm.Compute(File.ReadAllBytes(file));
                        if (hash != resource.Value)
                        {
                            _logger.LogDebug("CRC Check: File {File} did not match expected CRC32 hash for Version {Version}. Expected: 0x{ExpectedHash:x}, Was: 0x{ActualHash:x}", file, versionCrcSet.Version, resource.Value, hash);
                            allMatch = false;
                            break;
                        }
                    }
                    else
                    {
                        _logger.LogDebug("CRC Check: File {File} could not be found", file);
                    }
                }
                if (allMatch)
                {
                    GameVersion = versionCrcSet.Version;
                    break;
                }
            }
        }

        #region Utils
        private string GetNewNameId()
        {
            var lastNameId = _bgmDbRootEntries.Values.Where(p => p.NameId != "random" && !string.IsNullOrEmpty(p.NameId)).OrderByDescending(p => Base36IncrementHelper.ToInt(p.NameId)).FirstOrDefault()?.NameId;
            lastNameId = Base36IncrementHelper.ToString(Base36IncrementHelper.ToInt(lastNameId) + 1);
            if (lastNameId == "random")
                return GetNewNameId();
            return lastNameId;
        }

        private Dictionary<string, MsbtDatabase> GetBgmDatabases()
        {
            var output = new Dictionary<string, MsbtDatabase>();
            foreach (var locale in LocaleHelper.ValidLocales)
            {
                var msbt = _state.LoadResource<MsbtDatabase>(string.Format(MsbtExtConstants.MSBT_BGM, locale), true);
                if (msbt != null)
                    output.Add(locale, msbt);
            }
            return output;
        }

        private Dictionary<string, MsbtDatabase> GetGameTitleDatabases()
        {
            var output = new Dictionary<string, MsbtDatabase>();
            foreach (var locale in LocaleHelper.ValidLocales)
            {
                var msbt = _state.LoadResource<MsbtDatabase>(string.Format(MsbtExtConstants.MSBT_TITLE, locale), true);
                if (msbt != null)
                    output.Add(locale, msbt);
            }
            return output;
        }

        private Dictionary<string, float> GetCoreNus3BankVolumes()
        {
            var output = new Dictionary<string, float>();

            var nusBankResourceFile = Path.Combine(_config.CurrentValue.ResourcesPath, MusicConstants.Resources.NUS3BANK_IDS_FILE);
            if (!File.Exists(nusBankResourceFile))
                return output;

            _logger.LogDebug("Retrieving NusBank Volumes from CSV {CSVResource}", nusBankResourceFile);
            using (var reader = new StreamReader(nusBankResourceFile))
            {
                var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    PrepareHeaderForMatch = (args) => Regex.Replace(args.Header, @"\s", string.Empty)
                };
                using (var csv = new CsvReader(reader, csvConfiguration))
                {
                    var records = csv.GetRecords<dynamic>();
                    foreach (var record in records)
                    {
                        var volume = Convert.ToSingle(record.Volume, CultureInfo.InvariantCulture);
                        var name = (string)record.NUS3BankName;
                        output.Add(name.TrimStart(MusicConstants.InternalIds.NUS3AUDIO_FILE_PREFIX), volume);
                    }
                }
            }
            return output;
        }

        private string ConvertFromGameTextTag(string input)
        {
            return input.Replace("\u000e\u0000\u0002\u0002P", "{{").Replace("\u000e\u0000\u0002\u0002d", "}}");
        }

        private string ConvertToGameTextTag(string input)
        {
            return input.Replace("{{", "\u000e\u0000\u0002\u0002P").Replace("}}", "\u000e\u0000\u0002\u0002d");
        }
        #endregion
        #endregion
    }
}
