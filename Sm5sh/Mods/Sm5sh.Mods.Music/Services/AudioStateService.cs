using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sm5sh.Data;
using Sm5sh.Data.Ui.Param.Database;
using Sm5sh.Data.Ui.Param.Database.PrcUiBgmDatabaseModels;
using Sm5sh.Data.Ui.Param.Database.PrcUiGameTitleDatabaseModels;
using Sm5sh.Helpers;
using Sm5sh.Interfaces;
using Sm5sh.Mods.Music.Helpers;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Models;
using Sm5sh.ResourceProviders.Prc.Helpers;
using System.Collections.Generic;
using System.Linq;
using Sm5sh.ResourceProviders.Constants;
using System.IO;

namespace Sm5sh.Mods.Music.Services
{
    public class AudioStateService : IAudioStateService
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IStateManager _state;
        private readonly IOptions<Sm5shMusicOptions> _config;
        private readonly HashSet<string> _seriesEntries;
        private readonly HashSet<string> _localesEntries;
        private readonly Dictionary<string, GameTitleEntry> _gameEntries;
        private readonly Dictionary<string, BgmEntry> _bgmEntries;
        private readonly Dictionary<string, BgmEntry> _deletedBgmEntries; //TODO
        private readonly Dictionary<string, PlaylistEntry> _playlistsEntries;
        private readonly Dictionary<string, StageEntry> _stageEntries;

        public AudioStateService(IOptions<Sm5shMusicOptions> config, IMapper mapper, IStateManager state, ILogger<IAudioStateService> logger)
        {
            _config = config;
            _mapper = mapper;
            _logger = logger;
            _state = state;
            _deletedBgmEntries = new Dictionary<string, BgmEntry>();
            _gameEntries = new Dictionary<string, GameTitleEntry>();
            _seriesEntries = new HashSet<string>();
            _localesEntries = new HashSet<string>();
            _bgmEntries = new Dictionary<string, BgmEntry>();
            _playlistsEntries = new Dictionary<string, PlaylistEntry>();
            _stageEntries = new Dictionary<string, StageEntry>();
            InitBgmEntriesFromStateManager();
        }

        public IEnumerable<BgmEntry> GetBgmEntries()
        {
            return _bgmEntries.Values;
        }

        public IEnumerable<BgmEntry> GetModBgmEntries()
        {
            return GetBgmEntries().Where(p => p.Source == EntrySource.Mod);
        }

        public BgmEntry GetBgmEntry(string toneId)
        {
            return _bgmEntries.ContainsKey(toneId) ? _bgmEntries[toneId] : null;
        }

        public IEnumerable<GameTitleEntry> GetGameTitleEntries()
        {
            return _gameEntries.Values;
        }

        public IEnumerable<StageEntry> GetStagesEntries()
        {
            return _stageEntries.Values;
        }

        public IEnumerable<string> GetSeriesEntries()
        {
            return _seriesEntries;
        }

        public IEnumerable<string> GetLocales()
        {
            return _localesEntries;
        }

        public IEnumerable<PlaylistEntry> GetPlaylists()
        {
            return _playlistsEntries.Values;
        }

        public bool AddBgmEntry(BgmEntry bgmEntry)
        {
            //TODO TODO TODO
            //VALIDATE FOR WEIRD CHARACTER, LENGTH
            //VERIFY THAT IDS ARE UNIQUE IN TEMP
            //VERIFY HASH
            //TODO TODO TODO

            //Temporary - Only do that if not hidden!
            bgmEntry.DbRoot.MenuValue = _bgmEntries.Values.OrderByDescending(p => p.DbRoot.MenuValue).First().DbRoot.MenuValue + 1;
            bgmEntry.DbRoot.TestDispOrder = (short)(_bgmEntries.Values.OrderByDescending(p => p.DbRoot.TestDispOrder).First().DbRoot.TestDispOrder + 1);
            bgmEntry.DbRoot.NameId = GetNewBgmId();
            bgmEntry.DbRoot.SaveNo = 0;

            //Save GameTitle & Series
            if (!_gameEntries.ContainsKey(bgmEntry.GameTitleId))
                _gameEntries.Add(bgmEntry.GameTitleId, bgmEntry.GameTitle);
            if (!_seriesEntries.Contains(bgmEntry.GameTitle.UiSeriesId))
                _seriesEntries.Add(bgmEntry.GameTitle.UiSeriesId);

            //Create
            if (!_bgmEntries.ContainsKey(bgmEntry.ToneId))
                _bgmEntries.Add(bgmEntry.ToneId, bgmEntry);
            else
            {
                _logger.LogError("Bgm with ToneId {ToneId} already exist in the database.", bgmEntry.ToneId);
                return false;
            }

            return true;
        }

        public bool RemoveBgmEntry(string toneId)
        {
            if (_bgmEntries.ContainsKey(toneId))
            {
                if(!_deletedBgmEntries.ContainsKey(toneId))
                    _deletedBgmEntries.Add(toneId, _bgmEntries[toneId]);
                _bgmEntries.Remove(toneId);
            }
            else
            {
                _logger.LogWarning("ToneId {ToneId} was not found. Cannot remove from list...", toneId);
            }

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

        public bool SaveBgmEntriesToStateManager()
        {
            _logger.LogInformation("Saving Bgm Entries to State Service");

            //Load Data
            var paramBgmDatabase = _state.LoadResource<PrcUiBgmDatabase>(PrcExtConstants.PRC_UI_BGM_DB_PATH);
            var paramGameTitleDatabaseRoot = _state.LoadResource<PrcUiGameTitleDatabase>(PrcExtConstants.PRC_UI_GAMETITLE_DB_PATH).DbRootEntries;
            var binBgmPropertyEntries = _state.LoadResource<Data.Sound.Config.BinBgmProperty>(BgmPropertyFileConstants.BGM_PROPERTY_PATH).Entries;
            var daoMsbtBgms = GetBgmDatabases();
            var daoMsbtTitle = GetGameTitleDatabases();

            var defaultLocale = _config.Value.Sm5shMusic.DefaultLocale;
            var gameTitles = _bgmEntries.Values.GroupBy(p => p.GameTitleId).Select(p => p.First().GameTitle).Where(p => p != null && p.UiGameTitleId != Constants.InternalIds.GAME_TITLE_ID_DEFAULT);
            var coreSeriesGames = paramGameTitleDatabaseRoot.Values.Select(p => p.UiSeriesId).Distinct(); //Not handling series addition right now.

            //GameTitle PRC - We don't delete existing games... yet.
            foreach (var gameTitle in gameTitles)
            {
                paramGameTitleDatabaseRoot[gameTitle.UiGameTitleId] = _mapper.Map<PrcGameTitleDbRootEntry>(gameTitle);
            }

            //BGM Saving
            foreach (var bgmEntry in _bgmEntries.Values)
            {
                var toneId = bgmEntry.ToneId;

                //Save Bin & BGM PRC
                binBgmPropertyEntries[toneId] = _mapper.Map<Data.Sound.Config.BgmPropertyStructs.BgmPropertyEntry>(bgmEntry.BgmProperties);
                paramBgmDatabase.DbRootEntries[bgmEntry.DbRootKey] = _mapper.Map<PrcBgmDbRootEntry>(bgmEntry.DbRoot);
                paramBgmDatabase.AssignedInfoEntries[bgmEntry.AssignedInfoKey] = _mapper.Map<PrcBgmAssignedInfoEntry>(bgmEntry.AssignedInfo);
                paramBgmDatabase.StreamPropertyEntries[bgmEntry.StreamPropertyKey] = _mapper.Map<PrcBgmStreamPropertyEntry>(bgmEntry.StreamingProperty);
                paramBgmDatabase.StreamSetEntries[bgmEntry.StreamSetKey] = _mapper.Map<PrcBgmStreamSetEntry>(bgmEntry.StreamSet);

                //Save MSBT
                #region
                if (!string.IsNullOrEmpty(bgmEntry.DbRoot.NameId))
                {
                    var titleLabel = bgmEntry.MSBTLabels.TitleKey;
                    var titleDict = bgmEntry.MSBTLabels.Title;
                    var authorLabel = bgmEntry.MSBTLabels.AuthorKey;
                    var authorDict = bgmEntry.MSBTLabels.Author;
                    var copyrightLabel = bgmEntry.MSBTLabels.CopyrightKey;
                    var copyrightDict = bgmEntry.MSBTLabels.Copyright;
                    foreach (var msbtDb in daoMsbtBgms)
                    {
                        var entries = msbtDb.Value.Entries;

                        //Title
                        if (titleDict != null && titleDict.ContainsKey(msbtDb.Key) && !string.IsNullOrEmpty(titleDict[msbtDb.Key]))
                            entries[titleLabel] = titleDict[msbtDb.Key];
                        else if (titleDict != null && titleDict.ContainsKey(defaultLocale) && !string.IsNullOrEmpty(titleDict[msbtDb.Key]))
                            entries[titleLabel] = titleDict[defaultLocale];

                        //Author
                        if (authorDict != null)
                        {
                            if (authorDict.ContainsKey(msbtDb.Key) && !string.IsNullOrEmpty(authorDict[msbtDb.Key]))
                                entries[authorLabel] = authorDict[msbtDb.Key];
                            else if (authorDict.ContainsKey(defaultLocale) && !string.IsNullOrEmpty(authorDict[msbtDb.Key]))
                                entries[authorLabel] = authorDict[defaultLocale];
                        }

                        //Copyright
                        if (copyrightDict != null)
                        {
                            if (copyrightDict.ContainsKey(msbtDb.Key) && !string.IsNullOrEmpty(copyrightDict[msbtDb.Key]))
                                entries[copyrightLabel] = copyrightDict[msbtDb.Key];
                            else if (copyrightDict.ContainsKey(defaultLocale) && !string.IsNullOrEmpty(copyrightDict[msbtDb.Key]))
                                entries[copyrightLabel] = copyrightDict[defaultLocale];
                        }
                    }
                }
                //Game Title
                if (!string.IsNullOrEmpty(bgmEntry.GameTitle?.NameId))
                {
                    var gameTitleLabel = bgmEntry.GameTitle.MSBTTitleKey;
                    var titleDict = bgmEntry.GameTitle.MSBTTitle;
                    foreach (var msbtDb in daoMsbtTitle)
                    {
                        var entries = msbtDb.Value.Entries;
                        if (titleDict.ContainsKey(msbtDb.Key))
                            entries[gameTitleLabel] = titleDict[msbtDb.Key];
                        else if (titleDict.ContainsKey(defaultLocale))
                            entries[gameTitleLabel] = titleDict[defaultLocale];
                    }
                }
                #endregion 
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
                    tracks.Add(_mapper.Map<PrcBgmPlaylistEntry>(track));
                }
            }

            return true;
        }

        #region Private
        private void InitBgmEntriesFromStateManager()
        {
            //Make sure resources are unloaded
            _state.UnloadResources();
            _bgmEntries.Clear();
            _deletedBgmEntries.Clear();
            _gameEntries.Clear();
            _seriesEntries.Clear();
            _localesEntries.Clear();
            _playlistsEntries.Clear();
            _stageEntries.Clear();

            //Load Data
            var daoBinBgmProperty = _state.LoadResource<Data.Sound.Config.BinBgmProperty>(BgmPropertyFileConstants.BGM_PROPERTY_PATH);
            var paramBgmDatabase = _state.LoadResource<PrcUiBgmDatabase>(PrcExtConstants.PRC_UI_BGM_DB_PATH);
            var paramGameTitleDbRoot = _state.LoadResource<PrcUiGameTitleDatabase>(PrcExtConstants.PRC_UI_GAMETITLE_DB_PATH).DbRootEntries;
            var paramStageDbRoot = _state.LoadResource<PrcUiStageDatabase>(PrcExtConstants.PRC_UI_STAGE_DB_PATH).DbRootEntries;
            var daoMsbtBgms = GetBgmDatabases();
            var daoMsbtTitle = GetGameTitleDatabases();
            daoMsbtBgms.Keys.ToList().ForEach(p => _localesEntries.Add(p));

            foreach (var daoBinPropertyKeyValue in daoBinBgmProperty.Entries)
            {
                var toneId = daoBinPropertyKeyValue.Key;
                var newBgmEntry = new BgmEntry(toneId);
                newBgmEntry.Filename = Path.Combine(_config.Value.GameResourcesPath, "stream;", "sound", "bgm", string.Format(Constants.GameResources.NUS3AUDIO_FILE, newBgmEntry.ToneId));

                //Very few songs are currently not in the db - therefore not supported for now.
                if (!paramBgmDatabase.DbRootEntries.ContainsKey(newBgmEntry.DbRootKey))
                    continue;

                //Mapping PRC / Property Bin
                var dbRootEntry = paramBgmDatabase.DbRootEntries[newBgmEntry.DbRoot.UiBgmId];
                _mapper.Map(daoBinPropertyKeyValue.Value, newBgmEntry.BgmProperties);
                _mapper.Map(dbRootEntry, newBgmEntry.DbRoot);
                _mapper.Map(paramBgmDatabase.StreamSetEntries[newBgmEntry.StreamSetKey], newBgmEntry.StreamSet);
                _mapper.Map(paramBgmDatabase.AssignedInfoEntries[newBgmEntry.AssignedInfoKey], newBgmEntry.AssignedInfo);
                _mapper.Map(paramBgmDatabase.StreamPropertyEntries[newBgmEntry.StreamPropertyKey], newBgmEntry.StreamingProperty);

                //Mapping Game Title PRC
                var gameTitleId = dbRootEntry.UiGameTitleId;
                if (!_gameEntries.ContainsKey(gameTitleId))
                    _gameEntries.Add(gameTitleId, _mapper.Map(paramGameTitleDbRoot[gameTitleId], new GameTitleEntry(gameTitleId)));
                newBgmEntry.GameTitle = _gameEntries[gameTitleId];
                if (!_seriesEntries.Contains(newBgmEntry.GameTitle.UiSeriesId))
                    _seriesEntries.Add(newBgmEntry.GameTitle.UiSeriesId);

                //Mapping MSBT
                if (!string.IsNullOrEmpty(newBgmEntry.DbRoot.NameId))
                {
                    var titleLabel = newBgmEntry.MSBTLabels.TitleKey;
                    var authorLabel = newBgmEntry.MSBTLabels.AuthorKey;
                    var copyrightLabel = newBgmEntry.MSBTLabels.CopyrightKey;
                    foreach (var msbtDb in daoMsbtBgms)
                    {
                        var entries = msbtDb.Value.Entries;
                        if (entries.ContainsKey(titleLabel))
                            newBgmEntry.MSBTLabels.Title.Add(msbtDb.Key, entries[titleLabel]);
                        if (entries.ContainsKey(authorLabel))
                            newBgmEntry.MSBTLabels.Author.Add(msbtDb.Key, entries[authorLabel]);
                        if (entries.ContainsKey(copyrightLabel))
                            newBgmEntry.MSBTLabels.Copyright.Add(msbtDb.Key, entries[copyrightLabel]);
                    }
                }
                if (!string.IsNullOrEmpty(newBgmEntry.GameTitle?.NameId) && newBgmEntry.GameTitle.MSBTTitle.Count == 0) //Test for cache
                {
                    var gameTitleLabel = newBgmEntry.GameTitle.MSBTTitleKey;
                    foreach (var msbtDb in daoMsbtTitle)
                    {
                        var entries = msbtDb.Value.Entries;
                        if (entries.ContainsKey(gameTitleLabel))
                            newBgmEntry.GameTitle.MSBTTitle.Add(msbtDb.Key, entries[gameTitleLabel]);
                    }
                }

                _bgmEntries.Add(toneId, newBgmEntry);
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
            foreach(var stage in paramStageDbRoot)
            {
                _stageEntries.Add(stage.Key, _mapper.Map<StageEntry>(stage.Value));
            }
        }

        private void RemoveBgmEntryFromStateManager(string toneId)
        {
            //TODO LATER

            /*var keyRefs = GetToneIdKeyReferences(toneId);

            var binBgmProperty = _state.LoadResource<Data.Sound.Config.BinBgmProperty>(Constants.GameResources.PRC_BGM_PROPERTY_PATH);
            var paramBgmDatabase = _state.LoadResource<PrcUiBgmDatabase>(Constants.GameResources.PRC_UI_BGM_DB_PATH);

            //If not in DBROOT, skip but no error
            if (!paramBgmDatabase.DbRootEntries.ContainsKey(keyRefs.DbRootKey))
                return;

            //BIN
            binBgmProperty.Entries.Remove(toneId);

            //PRC
            var dbRootRef = paramBgmDatabase.DbRootEntries[keyRefs.DbRootKey];
            paramBgmDatabase.DbRootEntries.Remove(keyRefs.DbRootKey);
            paramBgmDatabase.StreamSetEntries.Remove(keyRefs.StreamSetKey);
            paramBgmDatabase.AssignedInfoEntries.Remove(keyRefs.AssignedInfoKey);
            paramBgmDatabase.StreamPropertyEntries.Remove(keyRefs.StreamPropertyKey);

            //TODO GAMETITLE
            //TODO GAMETITLE
            //TODO GAMETITLE

            //PLAYLISTS
            foreach (var playlist in paramBgmDatabase.PlaylistEntries)
                playlist.Values.RemoveAll(p => p.UiBgmId == keyRefs.DbRootKey);

            //MSBT
            if (dbRootRef != null)
            {
                var nameId = dbRootRef.NameId;
                var titleLabel = string.Format(Constants.InternalIds.MSBT_BGM_TITLE, nameId);
                var authorLabel = string.Format(Constants.InternalIds.MSBT_BGM_AUTHOR, nameId);
                var copyrightLabel = string.Format(Constants.InternalIds.MSBT_BGM_COPYRIGHT, nameId);
                foreach (var msbtBgm in GetBgmDatabases().Values)
                {
                    msbtBgm.Entries.Remove(titleLabel);
                    msbtBgm.Entries.Remove(authorLabel);
                    msbtBgm.Entries.Remove(copyrightLabel);
                }
            }*/
        }

        #region Utils
        private string GetNewBgmId()
        {
            var lastNameId = _bgmEntries.Values.Where(p => p.DbRoot.NameId != "random" && !string.IsNullOrEmpty(p.DbRoot.NameId)).OrderByDescending(p => Base36IncrementHelper.ToInt(p.DbRoot.NameId)).FirstOrDefault()?.DbRoot.NameId;
            lastNameId = Base36IncrementHelper.ToString(Base36IncrementHelper.ToInt(lastNameId) + 1);
            if (lastNameId == "random")
                return GetNewBgmId();
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
        #endregion
        #endregion
    }
}
