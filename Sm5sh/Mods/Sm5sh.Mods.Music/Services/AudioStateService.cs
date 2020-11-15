using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sm5sh.Data;
using Sm5sh.Data.Ui.Param.Database;
using Sm5sh.Data.Ui.Param.Database.PrcUiBgmDatabaseModels;
using Sm5sh.Data.Ui.Param.Database.PrcUiGameTitleDatabaseModels;
using Sm5sh.Data.Ui.Param.Database.PrcUiStageDatabasModels;
using Sm5sh.Helpers;
using Sm5sh.Interfaces;
using Sm5sh.Mods.Music.Data.Sound.Config;
using Sm5sh.Mods.Music.Data.Sound.Config.BgmPropertyStructs;
using Sm5sh.Mods.Music.Helpers;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Models;
using Sm5sh.Mods.Music.Models.BgmEntryModels;
using Sm5sh.ResourceProviders.Prc.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Sm5sh.Mods.Music.Services
{
    public class AudioStateService : IAudioStateService
    {
        private readonly ILogger _logger;
        private readonly IStateManager _state;
        private readonly IOptions<Sm5shMusicOptions> _config;

        //Dao
        private Dictionary<string, PrcBgmDbRootEntry> _daoUiBgmDbRootEntries;
        private Dictionary<string, PrcBgmStreamSetEntry> _daoUiBgmStreamSetEntries;
        private Dictionary<string, PrcBgmAssignedInfoEntry> _daoUiBgmAssignedInfoEntries;
        private Dictionary<string, PrcBgmStreamPropertyEntry> _daoUiBgmStreamPropertyEntries;
        private Dictionary<string, BgmPropertyEntry> _daoBinPropertyEntries;
        private Dictionary<string, PrcGameTitleDbRootEntry> _daoUiGameTitleDbRootEntries;
        private Dictionary<string, StageDbRootEntry> _daoUiStageDbRootEntries;
        private Dictionary<string, List<PrcBgmPlaylistEntry>> _daobgmPlaylistsEntries;
        private readonly Dictionary<string, MsbtDatabase> _daoMsbtBgms;
        private readonly Dictionary<string, MsbtDatabase> _daoMsbtTitle;
        private string _lastNameId;

        //Public
        private Dictionary<string, BgmEntry> _bgmEntries;
        private readonly Dictionary<string, AudioStateServiceModels.BgmDbOperation> _dbOperations;

        public AudioStateService(IOptions<Sm5shMusicOptions> config, IStateManager state, ILogger<IAudioStateService> logger)
        {
            _config = config;
            _logger = logger;
            _state = state;
            _dbOperations = new Dictionary<string, AudioStateServiceModels.BgmDbOperation>();
            _daoMsbtBgms = new Dictionary<string, MsbtDatabase>();
            _daoMsbtTitle = new Dictionary<string, MsbtDatabase>();
            _bgmEntries = InitializeCoreBgmEntries();
        }

        public IEnumerable<BgmEntry> GetBgmEntries()
        {
            return _bgmEntries.Values;
        }

        public IEnumerable<BgmEntry> GetModBgmEntries()
        {
            return _bgmEntries.Values.Where(p => p.Source == BgmAudioSource.Mod);
        }

        public BgmEntry AddBgmEntry(string toneId, BgmEntry newBgmEntry)
        {
            if (_bgmEntries.ContainsKey(toneId))
            {
                _logger.LogError("The ToneId {ToneId} already exists in the Bgm Entries", toneId);
                return null;
            }

            //Enforce some values
            newBgmEntry.Source = BgmAudioSource.Mod;
            newBgmEntry.ToneId = toneId;

            _bgmEntries.Add(toneId, newBgmEntry);

            //Delete remove tag
            if (_dbOperations.ContainsKey(toneId))
                _dbOperations.Remove(toneId);

            //If already exists in Core game, do not tag added
            if (!_daoUiBgmStreamPropertyEntries.Any(p => p.Value.DateName0 == toneId))
                _dbOperations[toneId] = AudioStateServiceModels.BgmDbOperation.Added;

            return newBgmEntry;
        }

        public bool RemoveBgmEntry(string toneId)
        {
            if (_bgmEntries.ContainsKey(toneId))
            {
                _bgmEntries.Remove(toneId);
                _dbOperations[toneId] = AudioStateServiceModels.BgmDbOperation.Removed;
            }
            return true;
        }


        public bool SaveChanges()
        {
            _logger.LogInformation("Saving Bgm Entries to State Service");

            //Remove
            SaveChangesDeleteEntries();

            //Add/Update
            var defaultLocale = _config.Value.Sm5shMusic.DefaultLocale;
            var coreSeriesGames = _daoUiGameTitleDbRootEntries.Values.Select(p => p.UiSeriesId.StringValue).Distinct(); //Not handling series addition right now.
            foreach (var bgmEntry in _bgmEntries.Values)
            {
                var toneId = bgmEntry.ToneId;
                if (!_daoUiBgmDbRootEntries.ContainsKey(toneId))
                {
                    //New BGM
                    SaveChangesAddEntry(toneId);
                }
                
                //BGM PRC
                var dbRootEntry = _daoUiBgmDbRootEntries[toneId];
                var setStreamEntry = _daoUiBgmStreamSetEntries[toneId];
                var assignedInfoEntry = _daoUiBgmAssignedInfoEntries[toneId];
                var streamPropertyEntry = _daoUiBgmStreamPropertyEntries[toneId];
                dbRootEntry.UiGameTitleId = new PrcHash40(bgmEntry.GameTitle.GameTitleId);
                dbRootEntry.RecordType = new PrcHash40(bgmEntry.RecordType);
                dbRootEntry.IsPatch = bgmEntry.IsPatch;
                dbRootEntry.IsDlc = bgmEntry.IsDlc;

                //GameTitle PRC
                if (!_daoUiGameTitleDbRootEntries.ContainsKey(bgmEntry.GameTitle.GameTitleId))
                {
                    string seriesId = bgmEntry.GameTitle.SeriesId;
                    if (!coreSeriesGames.Contains(seriesId))
                        seriesId = Constants.InternalIds.GAME_SERIES_ID_DEFAULT;
                    _daoUiGameTitleDbRootEntries.Add(bgmEntry.GameTitle.GameTitleId, new PrcGameTitleDbRootEntry()
                    {
                        NameId = bgmEntry.GameTitle.NameId,
                        Release = _daoUiGameTitleDbRootEntries.Values.OrderByDescending(p => p.Release).First().Release + 1,
                        UiGameTitleId = new PrcHash40(bgmEntry.GameTitle.GameTitleId),
                        UiSeriesId = new PrcHash40(seriesId)
                    });
                }

                //Bin Property
                _daoBinPropertyEntries[toneId].TotalSamples = bgmEntry.AudioCuePoints.TotalSamples;
                _daoBinPropertyEntries[toneId].LoopEndMs = bgmEntry.AudioCuePoints.LoopEndMs;
                _daoBinPropertyEntries[toneId].LoopEndSample = bgmEntry.AudioCuePoints.LoopEndSample;
                _daoBinPropertyEntries[toneId].LoopStartMs = bgmEntry.AudioCuePoints.LoopStartMs;
                _daoBinPropertyEntries[toneId].LoopStartSample = bgmEntry.AudioCuePoints.LoopStartSample;
                _daoBinPropertyEntries[toneId].TotalTimeMs = bgmEntry.AudioCuePoints.TotalTimeMs;
                _daoBinPropertyEntries[toneId].NameId = toneId;

                //Playlists
                SaveChangesPlaylist(bgmEntry.Playlists, dbRootEntry.UiBgmId.StringValue);

                //MSBT
                var nameId = dbRootEntry.NameId;
                var gameTitleEntry = _daoUiGameTitleDbRootEntries[dbRootEntry.UiGameTitleId.StringValue];
                var gameTitleId = gameTitleEntry.NameId;
                var gameTitleLabel = string.Format(Constants.InternalIds.MSBT_GAME_TITLE, gameTitleId);
                var titleLabel = string.Format(Constants.InternalIds.MSBT_BGM_TITLE, nameId);
                var authorLabel = string.Format(Constants.InternalIds.MSBT_BGM_AUTHOR, nameId);
                var copyrightLabel = string.Format(Constants.InternalIds.MSBT_BGM_COPYRIGHT, nameId);
                foreach (var msbtDb in _daoMsbtBgms)
                {
                    var entries = msbtDb.Value.Entries;

                    if (bgmEntry.Title.ContainsKey(msbtDb.Key))
                        entries[titleLabel] = bgmEntry.Title[msbtDb.Key];
                    else if (bgmEntry.Title.ContainsKey(defaultLocale))
                        entries[titleLabel] = bgmEntry.Title[defaultLocale];
                    else
                        entries[titleLabel] = "MISSING";

                    if (bgmEntry.Author.ContainsKey(msbtDb.Key))
                        entries[authorLabel] = bgmEntry.Author[msbtDb.Key];
                    else if (bgmEntry.Author.ContainsKey(defaultLocale))
                        entries[authorLabel] = bgmEntry.Author[defaultLocale];

                    if (bgmEntry.Copyright.ContainsKey(msbtDb.Key))
                        entries[copyrightLabel] = bgmEntry.Copyright[msbtDb.Key];
                    else if (bgmEntry.Copyright.ContainsKey(defaultLocale))
                        entries[copyrightLabel] = bgmEntry.Copyright[defaultLocale];
                }
                foreach (var msbtDb in _daoMsbtTitle)
                {
                    var entries = msbtDb.Value.Entries;
                    if (bgmEntry.GameTitle.Title.ContainsKey(msbtDb.Key))
                        entries[gameTitleLabel] = bgmEntry.GameTitle.Title[msbtDb.Key];
                    else if (bgmEntry.GameTitle.Title.ContainsKey(defaultLocale))
                        entries[gameTitleLabel] = bgmEntry.GameTitle.Title[defaultLocale];
                    else
                        entries[gameTitleLabel] = "MISSING";
                }
            }

            //Save BIN/PRC
            var daoBinBgmProperty = _state.LoadResource<BinBgmProperty>(Constants.GameResources.PRC_BGM_PROPERTY_PATH);
            var daoUiBgmDatabase = _state.LoadResource<PrcUiBgmDatabase>(Constants.GameResources.PRC_UI_BGM_DB_PATH);
            var daoUiGameTitleDatabase = _state.LoadResource<PrcUiGameTitleDatabase>(Constants.GameResources.PRC_UI_GAMETITLE_DB_PATH);
            var daoUiStageDatabase = _state.LoadResource<PrcUiStageDatabase>(Constants.GameResources.PRC_UI_STAGE_DB_PATH);
            daoUiBgmDatabase.DbRootEntries = _daoUiBgmDbRootEntries.Values.ToList();
            daoUiBgmDatabase.StreamSetEntries = _daoUiBgmStreamSetEntries.Values.ToList();
            daoUiBgmDatabase.AssignedInfoEntries = _daoUiBgmAssignedInfoEntries.Values.ToList();
            daoUiBgmDatabase.StreamPropertyEntries = _daoUiBgmStreamPropertyEntries.Values.ToList();
            daoUiBgmDatabase.PlaylistEntries = _daobgmPlaylistsEntries.Select(p => new PcrFilterStruct<PrcBgmPlaylistEntry>()
            {
                Id = new PrcHash40(p.Key), Values = p.Value
            }).ToList();
            daoUiGameTitleDatabase.DbRootEntries = _daoUiGameTitleDbRootEntries.Values.ToList();
            daoBinBgmProperty.Entries = _daoBinPropertyEntries.Values.ToList();

            return true;
        }

        #region Private
        private Dictionary<string, BgmEntry> InitializeCoreBgmEntries()
        {
            //Load BGM_PROPERTY
            var daoBinBgmProperty = _state.LoadResource<BinBgmProperty>(Constants.GameResources.PRC_BGM_PROPERTY_PATH);
            _daoBinPropertyEntries = daoBinBgmProperty.Entries.ToDictionary(p => p.NameId, p => p);

            //Initialize UI_BGM_DB
            var daoUiBgmDatabase = _state.LoadResource<PrcUiBgmDatabase>(Constants.GameResources.PRC_UI_BGM_DB_PATH);
            var daoUiGameTitleDatabase = _state.LoadResource<PrcUiGameTitleDatabase>(Constants.GameResources.PRC_UI_GAMETITLE_DB_PATH);
            var daoUiStageDatabase = _state.LoadResource<PrcUiStageDatabase>(Constants.GameResources.PRC_UI_STAGE_DB_PATH);
            _daoUiBgmDbRootEntries = daoUiBgmDatabase.DbRootEntries.ToDictionary(p => p.UiBgmId.StringValue.Replace(Constants.InternalIds.UI_BGM_ID_PREFIX, string.Empty), p => p);
            _daoUiBgmStreamSetEntries = daoUiBgmDatabase.StreamSetEntries.ToDictionary(p => p.StreamSetId.StringValue.Replace(Constants.InternalIds.STREAM_SET_PREFIX, string.Empty), p => p);
            _daoUiBgmAssignedInfoEntries = daoUiBgmDatabase.AssignedInfoEntries.ToDictionary(p => p.InfoId.StringValue.Replace(Constants.InternalIds.INFO_ID_PREFIX, string.Empty), p => p);
            _daoUiBgmStreamPropertyEntries = daoUiBgmDatabase.StreamPropertyEntries.ToDictionary(p => p.StreamId.StringValue.Replace(Constants.InternalIds.STREAM_PREFIX, string.Empty), p => p);
            _daoUiGameTitleDbRootEntries = daoUiGameTitleDatabase.DbRootEntries.ToDictionary(p => p.UiGameTitleId.StringValue, p => p);
            _daoUiStageDbRootEntries = daoUiStageDatabase.DbRootEntries.ToDictionary(p => p.UiStageId.StringValue, p => p);
            _daobgmPlaylistsEntries = daoUiBgmDatabase.PlaylistEntries.ToDictionary(p => p.Id.StringValue, p => p.Values);

            //Calculate last Name Id
            _lastNameId = _daoUiBgmDbRootEntries.Values.Where(p => p.NameId != "random" && !string.IsNullOrEmpty(p.NameId)).OrderByDescending(p => Base36IncrementHelper.ToInt(p.NameId)).FirstOrDefault()?.NameId;

            foreach (var locale in LocaleHelper.ValidLocales)
            {
                var msbtBgm = _state.LoadResource<MsbtDatabase>(string.Format(Constants.GameResources.MSBT_BGM, locale), true);
                if (msbtBgm != null)
                    _daoMsbtBgms.Add(locale, msbtBgm);

                var msbtTitle = _state.LoadResource<MsbtDatabase>(string.Format(Constants.GameResources.MSBT_TITLE, locale), true);
                if (msbtBgm != null)
                    _daoMsbtTitle.Add(locale, msbtTitle);
            }

            var output = new Dictionary<string, BgmEntry>();

            foreach (var dbRootEntryKeyValue in _daoUiBgmDbRootEntries)
            {
                var toneId = dbRootEntryKeyValue.Key;

                //For now, we're only treating songs that have all the data we need
                if (!_daoUiBgmStreamSetEntries.ContainsKey(toneId) || !_daoUiBgmAssignedInfoEntries.ContainsKey(toneId) ||
                   !_daoUiBgmStreamPropertyEntries.ContainsKey(toneId) || !_daoBinPropertyEntries.ContainsKey(toneId))
                    continue;

                var dbRootEntry = dbRootEntryKeyValue.Value;
                var setStreamEntry = _daoUiBgmStreamSetEntries[toneId];
                var assignedInfoEntry = _daoUiBgmAssignedInfoEntries[toneId];
                var streamPropertyEntry = _daoUiBgmStreamPropertyEntries[toneId];
                var gameTitleEntry = _daoUiGameTitleDbRootEntries[dbRootEntry.UiGameTitleId.StringValue];
                var bgmProperty = _daoBinPropertyEntries[toneId];

                var newBgmEntry = new BgmEntry()
                {
                    ToneId = toneId,
                    Source = BgmAudioSource.CoreGame,
                    GameTitle = new GameTitleEntry()
                    {
                        GameTitleId = dbRootEntry.UiGameTitleId.StringValue,
                        NameId = dbRootEntry.NameId,
                        SeriesId = gameTitleEntry.UiSeriesId.StringValue,
                        Title= new Dictionary<string, string>()
                    },
                    RecordType = dbRootEntry.RecordType.StringValue,
                    AudioCuePoints = new AudioCuePoints()
                    {
                        LoopEndMs = bgmProperty.LoopEndMs,
                        LoopEndSample = bgmProperty.LoopEndSample,
                        LoopStartMs = bgmProperty.LoopStartMs,
                        LoopStartSample = bgmProperty.LoopStartSample,
                        TotalSamples = bgmProperty.TotalSamples,
                        TotalTimeMs = bgmProperty.TotalTimeMs
                    },
                    Playlists = _daobgmPlaylistsEntries.Where(p => p.Value.Any(p => p.UiBgmId.HexValue == dbRootEntry.UiBgmId.HexValue)).Select(p => new PlaylistEntry() { Id = p.Key}).ToList(),
                    IsDlc = dbRootEntry.IsDlc,
                    IsPatch = dbRootEntry.IsPatch,
                    Title = new Dictionary<string, string>(),
                    Author = new Dictionary<string, string>(),
                    Copyright = new Dictionary<string, string>()
                };

                var nameId = dbRootEntry.NameId;
                var gameTitleId = gameTitleEntry.NameId;
                var gameTitleLabel = string.Format(Constants.InternalIds.MSBT_GAME_TITLE, gameTitleId);
                var titleLabel = string.Format(Constants.InternalIds.MSBT_BGM_TITLE, nameId);
                var authorLabel = string.Format(Constants.InternalIds.MSBT_BGM_AUTHOR, nameId);
                var copyrightLabel = string.Format(Constants.InternalIds.MSBT_BGM_COPYRIGHT, nameId);
                foreach (var msbtDb in _daoMsbtBgms)
                {
                    var entries = msbtDb.Value.Entries;
                    if (entries.ContainsKey(titleLabel))
                        newBgmEntry.Title.Add(msbtDb.Key, entries[titleLabel]);
                    if (entries.ContainsKey(authorLabel))
                        newBgmEntry.Author.Add(msbtDb.Key, entries[authorLabel]);
                    if (entries.ContainsKey(copyrightLabel))
                        newBgmEntry.Copyright.Add(msbtDb.Key, entries[copyrightLabel]);
                }
                foreach(var msbtDb in _daoMsbtTitle)
                {
                    var entries = msbtDb.Value.Entries;
                    if (entries.ContainsKey(gameTitleLabel))
                        newBgmEntry.GameTitle.Title.Add(msbtDb.Key, entries[gameTitleLabel]);
                }

                output.Add(toneId, newBgmEntry);
            }

            return output;
        }

        private void SaveChangesAddEntry(string toneId)
        {
            //var saveNoIndex = (short)(_daoUiBgmDbRootEntries.Values.OrderByDescending(p => p.SaveNo).First().SaveNo + 1); //Not working past top save_no id
            var testDispOrderIndex = (short)(_daoUiBgmDbRootEntries.Values.OrderByDescending(p => p.TestDispOrder).First().TestDispOrder + 1);
            var menuValueIndex = _daoUiBgmDbRootEntries.Values.OrderByDescending(p => p.MenuValue).First().MenuValue + 1;

            //New entry - with default values
            _daoUiBgmDbRootEntries.Add(toneId, new PrcBgmDbRootEntry() 
            { 
                UiBgmId = new PrcHash40($"{Constants.InternalIds.UI_BGM_ID_PREFIX}{toneId}"),
                StreamSetId = new PrcHash40($"{Constants.InternalIds.STREAM_SET_PREFIX}{toneId}"),
                Rarity = new PrcHash40(Constants.InternalIds.RARITY_DEFAULT),
                RecordType = new PrcHash40(Constants.InternalIds.RECORD_TYPE_DEFAULT),
                UiGameTitleId = new PrcHash40(Constants.InternalIds.GAME_TITLE_ID_DEFAULT),
                UiGameTitleId1 = new PrcHash40(Constants.InternalIds.GAME_TITLE_ID_DEFAULT),
                UiGameTitleId2 = new PrcHash40(Constants.InternalIds.GAME_TITLE_ID_DEFAULT),
                UiGameTitleId3 = new PrcHash40(Constants.InternalIds.GAME_TITLE_ID_DEFAULT),
                UiGameTitleId4 = new PrcHash40(Constants.InternalIds.GAME_TITLE_ID_DEFAULT),
                NameId = GetNewBgmId(),
                SaveNo = 0,
                TestDispOrder = testDispOrderIndex,
                MenuValue = menuValueIndex,
                JpRegion = true,
                OtherRegion = true,
                Possessed = true,
                PrizeLottery = false,
                ShopPrice = 0,
                CountTarget = true,
                MenuLoop = 1,
                IsSelectableStageMake = true,
                Unk1 = true,
                Unk2 = true,
                IsDlc = false,
                IsPatch = false
            });
            _daoUiBgmStreamSetEntries.Add(toneId, new PrcBgmStreamSetEntry()
            {
                StreamSetId = new PrcHash40($"{Constants.InternalIds.STREAM_SET_PREFIX}{toneId}"),
                SpecialCategory = new PrcHash40(0),
                Info0 = new PrcHash40($"{Constants.InternalIds.INFO_ID_PREFIX}{toneId}")
            });
            _daoUiBgmAssignedInfoEntries.Add(toneId, new PrcBgmAssignedInfoEntry()
            {
                InfoId = new PrcHash40($"{Constants.InternalIds.INFO_ID_PREFIX}{toneId}"),
                StreamId = new PrcHash40($"{Constants.InternalIds.STREAM_PREFIX}{toneId}"),
                Condition = new PrcHash40(Constants.InternalIds.SOUND_CONDITION),
                ConditionProcess = new PrcHash40(0x1b9fe75d3f),
                ChangeFadoutFrame = 55,
                MenuChangeFadeOutFrame = 55
            });
            _daoUiBgmStreamPropertyEntries.Add(toneId, new PrcBgmStreamPropertyEntry()
            {
                StreamId = new PrcHash40($"{Constants.InternalIds.STREAM_PREFIX}{toneId}"),
                DateName0 = toneId,
                Loop = 1,
                EndPoint = "00:00:15.000",
                FadeOutFrame = 400,
                StartPointTransition = "00:00:04.000"
            });
            _daoBinPropertyEntries.Add(toneId, new BgmPropertyEntry());
        }

        private void SaveChangesPlaylist(List<PlaylistEntry> playlistIds, string uiBgmId)
        {
            foreach (var playlistId in playlistIds)
            {
                if (!_daobgmPlaylistsEntries.ContainsKey(playlistId.Id))
                    _daobgmPlaylistsEntries.Add(playlistId.Id, new List<PrcBgmPlaylistEntry>());

                var playlist = _daobgmPlaylistsEntries[playlistId.Id];
                var newPlaylistEntry = new PrcBgmPlaylistEntry() { UiBgmId = new PrcHash40(uiBgmId) };
                newPlaylistEntry.SetOrder((short)playlist.Count);
                newPlaylistEntry.SetIncidence(500);

                _daobgmPlaylistsEntries[playlistId.Id].Add(newPlaylistEntry);
            }
        }

        private void SaveChangesDeleteEntries()
        {
            foreach (var toneIdToRemove in _dbOperations.Where(p => p.Value == AudioStateServiceModels.BgmDbOperation.Removed).Select(p => p.Key))
            {
                if (!_daoUiBgmDbRootEntries.ContainsKey(toneIdToRemove))
                    continue;

                var dbRootRef = _daoUiBgmDbRootEntries[toneIdToRemove];

                //Remove from DBs (untested)
                _daoBinPropertyEntries.Remove(toneIdToRemove);
                _daoUiBgmDbRootEntries.Remove(toneIdToRemove);
                _daoUiBgmStreamSetEntries.Remove(toneIdToRemove);
                _daoUiBgmAssignedInfoEntries.Remove(toneIdToRemove);
                _daoUiBgmStreamPropertyEntries.Remove(toneIdToRemove);

                //Remove from playlists (untested)
                foreach (var playlist in _daobgmPlaylistsEntries)
                    playlist.Value.RemoveAll(p => p.UiBgmId.StringValue == $"{Constants.InternalIds.UI_BGM_ID_PREFIX}{toneIdToRemove}");

                //Remove MSBT (untested)
                if (dbRootRef != null)
                {
                    var nameId = dbRootRef.NameId;
                    var titleLabel = string.Format(Constants.InternalIds.MSBT_BGM_TITLE, nameId);
                    var authorLabel = string.Format(Constants.InternalIds.MSBT_BGM_AUTHOR, nameId);
                    var copyrightLabel = string.Format(Constants.InternalIds.MSBT_BGM_COPYRIGHT, nameId);
                    foreach (var daoMsbtBgm in _daoMsbtBgms.Values)
                    {
                        daoMsbtBgm.Entries.Remove(titleLabel);
                        daoMsbtBgm.Entries.Remove(authorLabel);
                        daoMsbtBgm.Entries.Remove(copyrightLabel);
                    }
                }
            }
        }

        private string GetNewBgmId()
        {
            _lastNameId = Base36IncrementHelper.ToString(Base36IncrementHelper.ToInt(_lastNameId) + 1);
            if (_lastNameId == "random")
                return GetNewBgmId();
            return _lastNameId;
        }
        #endregion
    }

    namespace AudioStateServiceModels
    {
        public enum BgmDbOperation
        {
            Added,
            Removed
        }
    }
}
