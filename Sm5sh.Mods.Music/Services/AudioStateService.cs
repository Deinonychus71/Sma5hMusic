using Microsoft.Extensions.Logging;
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

        //Dao
        private Dictionary<string, PrcBgmDbRootEntry> _daoUiBgmDbRootEntries;
        private Dictionary<string, PrcBgmStreamSetEntry> _daoUiBgmStreamSetEntries;
        private Dictionary<string, PrcBgmAssignedInfoEntry> _daoUiBgmAssignedInfoEntries;
        private Dictionary<string, PrcBgmStreamPropertyEntry> _daoUiBgmStreamPropertyEntries;
        private Dictionary<string, BgmPropertyEntry> _daoBinPropertyEntries;
        private Dictionary<string, PrcGameTitleDbRootEntry> _daoUiGameTitleDbRootEntries;
        private Dictionary<string, StageDbRootEntry> _daoUiStageDbRootEntries;
        private readonly Dictionary<string, MsbtDatabase> _daoMsbtBgms;
        private readonly Dictionary<string, MsbtDatabase> _daoMsbtTitle;
        private string _lastNameId;

        //Public
        private Dictionary<string, BgmEntry> _bgmEntries;
        private readonly Dictionary<string, AudioStateServiceModels.BgmDbOperation> _dbOperations;

        public AudioStateService(IStateManager state, ILogger<IAudioStateService> logger)
        {
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
            foreach (var bgmEntry in _bgmEntries.Values)
            {
                var toneId = bgmEntry.ToneId;
                if (!_daoUiBgmDbRootEntries.ContainsKey(toneId))
                {
                    //New
                    SaveChangesAddEntry(toneId);
                }
                
                //Update
                var dbRootEntry = _daoUiBgmDbRootEntries[toneId];
                var setStreamEntry = _daoUiBgmStreamSetEntries[toneId];
                var assignedInfoEntry = _daoUiBgmAssignedInfoEntries[toneId];
                var streamPropertyEntry = _daoUiBgmStreamPropertyEntries[toneId];

                dbRootEntry.UiGameTitleId = new PrcHash40(bgmEntry.GameTitleId);
                dbRootEntry.RecordType = new PrcHash40(bgmEntry.RecordType);

                //Bin Property
                _daoBinPropertyEntries[toneId].TotalSamples = bgmEntry.AudioCuePoints.TotalSamples;
                _daoBinPropertyEntries[toneId].LoopEndMs = bgmEntry.AudioCuePoints.LoopEndMs;
                _daoBinPropertyEntries[toneId].LoopEndSample = bgmEntry.AudioCuePoints.LoopEndSample;
                _daoBinPropertyEntries[toneId].LoopStartMs = bgmEntry.AudioCuePoints.LoopStartMs;
                _daoBinPropertyEntries[toneId].LoopStartSample = bgmEntry.AudioCuePoints.LoopStartSample;
                _daoBinPropertyEntries[toneId].TotalTimeMs = bgmEntry.AudioCuePoints.TotalTimeMs;
            }

            //Save
            var daoBinBgmProperty = _state.LoadResource<BinBgmProperty>(Constants.GameResources.PRC_BGM_PROPERTY_PATH);
            var daoUiBgmDatabase = _state.LoadResource<PrcUiBgmDatabase>(Constants.GameResources.PRC_UI_BGM_DB_PATH);
            var daoUiGameTitleDatabase = _state.LoadResource<PrcUiGameTitleDatabase>(Constants.GameResources.PRC_UI_GAMETITLE_DB_PATH);
            var daoUiStageDatabase = _state.LoadResource<PrcUiStageDatabase>(Constants.GameResources.PRC_UI_STAGE_DB_PATH);
            daoUiBgmDatabase.DbRootEntries = _daoUiBgmDbRootEntries.Values.ToList();
            daoUiBgmDatabase.StreamSetEntries = _daoUiBgmStreamSetEntries.Values.ToList();
            daoUiBgmDatabase.AssignedInfoEntries = _daoUiBgmAssignedInfoEntries.Values.ToList();
            daoUiBgmDatabase.StreamPropertyEntries = _daoUiBgmStreamPropertyEntries.Values.ToList();
            daoBinBgmProperty.Entries = _daoBinPropertyEntries.Values.ToList();

            return true;
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
                IsPatch = false,
                Unk3 = PrcHash40.EmptyValue,
                Unk4 = PrcHash40.EmptyValue,
                Unk5 = PrcHash40.EmptyValue
            });
            _daoUiBgmStreamSetEntries.Add(toneId, new PrcBgmStreamSetEntry()
            {
                StreamSetId = new PrcHash40($"{Constants.InternalIds.STREAM_SET_PREFIX}{toneId}"),
                SpecialCategory = new PrcHash40(0),
                Info0 = new PrcHash40($"{Constants.InternalIds.INFO_ID_PREFIX}{toneId}"),
                Info1 = PrcHash40.EmptyValue,
                Info2 = PrcHash40.EmptyValue,
                Info3 = PrcHash40.EmptyValue,
                Info4 = PrcHash40.EmptyValue,
                Info5 = PrcHash40.EmptyValue,
                Info6 = PrcHash40.EmptyValue,
                Info7 = PrcHash40.EmptyValue,
                Info8 = PrcHash40.EmptyValue,
                Info9 = PrcHash40.EmptyValue,
                Info10 = PrcHash40.EmptyValue,
                Info11 = PrcHash40.EmptyValue,
                Info12 = PrcHash40.EmptyValue,
                Info13 = PrcHash40.EmptyValue,
                Info14 = PrcHash40.EmptyValue,
                Info15 = PrcHash40.EmptyValue
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
                DateName1 = string.Empty,
                DateName2 = string.Empty,
                DateName3 = string.Empty,
                DateName4 = string.Empty,
                Loop = 1,
                EndPoint = "00:00:15.000",
                FadeOutFrame = 400,
                StartPointSuddenDeath = string.Empty,
                StartPointTransition = "00:00:04.000",
                StartPoint0 = string.Empty,
                StartPoint1 = string.Empty,
                StartPoint2 = string.Empty,
                StartPoint3 = string.Empty,
                StartPoint4 = string.Empty
            });
            _daoBinPropertyEntries.Add(toneId, new BgmPropertyEntry());
        }

        private void SaveChangesDeleteEntries()
        {
            foreach (var toneIdToRemove in _dbOperations.Where(p => p.Value == AudioStateServiceModels.BgmDbOperation.Removed).Select(p => p.Key))
            {
                //Remove from DBs (untested)
                var daoBinBgmProperty = _state.LoadResource<BinBgmProperty>(Constants.GameResources.PRC_BGM_PROPERTY_PATH);
                daoBinBgmProperty.Entries.RemoveAll(p => p.NameId == toneIdToRemove);

                var daoUiBgmDatabase = _state.LoadResource<PrcUiBgmDatabase>(Constants.GameResources.PRC_UI_BGM_DB_PATH);
                var referenceDbRoot = daoUiBgmDatabase.DbRootEntries.FirstOrDefault(p => p.UiBgmId.StringValue == $"{Constants.InternalIds.UI_BGM_ID_PREFIX}{toneIdToRemove}");
                daoUiBgmDatabase.DbRootEntries.RemoveAll(p => p.UiBgmId.StringValue == $"{Constants.InternalIds.UI_BGM_ID_PREFIX}{toneIdToRemove}");
                daoUiBgmDatabase.StreamSetEntries.RemoveAll(p => p.StreamSetId.StringValue == $"{Constants.InternalIds.STREAM_SET_PREFIX}{toneIdToRemove}");
                daoUiBgmDatabase.AssignedInfoEntries.RemoveAll(p => p.InfoId.StringValue == $"{Constants.InternalIds.INFO_ID_PREFIX}{toneIdToRemove}");
                daoUiBgmDatabase.StreamPropertyEntries.RemoveAll(p => p.DateName0 == toneIdToRemove);

                //Remove from playlists (untested)
                foreach (var playlist in daoUiBgmDatabase.PlaylistEntries)
                {
                    playlist.Values.RemoveAll(p => p.UiBgmId.StringValue == $"{Constants.InternalIds.UI_BGM_ID_PREFIX}{toneIdToRemove}");
                }

                //Remove MSBT (untested)
                if (referenceDbRoot != null)
                {
                    var nameId = referenceDbRoot.NameId;
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
                var bgmProperty = _daoBinPropertyEntries[toneId];

                var newBgmEntry = new BgmEntry()
                {
                    ToneId = toneId,
                    Source = BgmAudioSource.CoreGame,
                    GameTitleId = dbRootEntry.UiGameTitleId.StringValue,
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
                    Title = new Dictionary<string, string>(),
                    Author = new Dictionary<string, string>(),
                    Copyright = new Dictionary<string, string>()
                };

                var nameId = dbRootEntry.NameId;
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

                output.Add(toneId, newBgmEntry);
            }

            return output;
        }

        private string GetNewBgmId()
        {
            _lastNameId = Base36IncrementHelper.ToString(Base36IncrementHelper.ToInt(_lastNameId) + 1);
            if (_lastNameId == "random")
                return GetNewBgmId();
            return _lastNameId;
        }
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
