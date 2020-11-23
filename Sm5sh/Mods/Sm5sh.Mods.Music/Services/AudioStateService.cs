using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sm5sh.Data;
using Sm5sh.Data.Ui.Param.Database;
using Sm5sh.Data.Ui.Param.Database.PrcUiBgmDatabaseModels;
using Sm5sh.Data.Ui.Param.Database.PrcUiGameTitleDatabaseModels;
using Sm5sh.Helpers;
using Sm5sh.Interfaces;
using Sm5sh.Mods.Music.Data.Sound.Config;
using Sm5sh.Mods.Music.Data.Sound.Config.BgmPropertyStructs;
using Sm5sh.Mods.Music.Helpers;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Models;
using Sm5sh.Mods.Music.Models.BgmEntryModels;
using Sm5sh.Mods.Music.Services.AudioStateServiceModels;
using Sm5sh.ResourceProviders.Prc.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sm5sh.Mods.Music.Services
{
    public class AudioStateService : IAudioStateService
    {
        private readonly ILogger _logger;
        private readonly IStateManager _state;
        private readonly IOptions<Sm5shMusicOptions> _config;
        private readonly Dictionary<string, BgmToneKeyReferences> _toneIdKeyReferences;

        public AudioStateService(IOptions<Sm5shMusicOptions> config, IStateManager state, ILogger<IAudioStateService> logger)
        {
            _config = config;
            _logger = logger;
            _state = state;
            _toneIdKeyReferences = new Dictionary<string, BgmToneKeyReferences>();
        }

        public IEnumerable<BgmEntry> GetBgmEntries()
        {
            return GetBgmEntriesFromStateManager().Values;
        }

        public IEnumerable<BgmEntry> GetModBgmEntries()
        {
            return GetBgmEntries().Where(p => p.Source == EntrySource.Mod);
        }

        public BgmEntry GetBgmEntry(string toneId)
        {
            return GetBgmEntriesFromStateManager(p => p.Key == toneId).Values.FirstOrDefault();
        }

        public BgmEntry AddOrUpdateBgmEntry(BgmEntry bgmEntry)
        {
            //TODO TODO TODO
            //VALIDATE FOR WEIRD CHARACTER, LENGTH
            //VERIFY THAT IDS ARE UNIQUE IN TEMP
            //VERIFY THAT NOT IN MODS OR CORE DB
            //VERIFY HASH
            //TODO TODO TODO

            var keyRefs = GetToneIdKeyReferences(bgmEntry.ToneId, bgmEntry);
            var bgmEntries = GetBgmEntriesFromStateManager();

            //Create
            if (!bgmEntries.ContainsKey(bgmEntry.ToneId))
                CreateNewBgmEntryInStateManager(keyRefs);

            //Update
            UpdateBgmEntryInStateManager(keyRefs, bgmEntry);

            //Return mapping BgmEntry
            return GetBgmEntry(bgmEntry.ToneId);
        }

        public void RemoveBgmEntry(string toneId)
        {
            RemoveBgmEntryFromStateManager(toneId);
        }

        #region Private
        private Dictionary<string, BgmEntry> GetBgmEntriesFromStateManager(Func<KeyValuePair<string, BgmPropertyEntry>, bool> predicate = null)
        {
            //Load BGM_PROPERTY
            var daoBinBgmProperty = _state.LoadResource<BinBgmProperty>(Constants.GameResources.PRC_BGM_PROPERTY_PATH);
            var daoBinPropertyEntries = daoBinBgmProperty.Entries.AsEnumerable();
            if (predicate != null)
                daoBinPropertyEntries = daoBinPropertyEntries.Where(predicate);

            //Load UI_BGM_DB / UI_GAMETITLE_DB
            var paramBgmDatabase = _state.LoadResource<PrcUiBgmDatabase>(Constants.GameResources.PRC_UI_BGM_DB_PATH);
            var paramGameTitleDbRoot = _state.LoadResource<PrcUiGameTitleDatabase>(Constants.GameResources.PRC_UI_GAMETITLE_DB_PATH).DbRootEntries;
            var paramBgmDbRoot = paramBgmDatabase.DbRootEntries;
            var paramBgmStreamSet = paramBgmDatabase.StreamSetEntries;
            var paramBgmAssignedInfo = paramBgmDatabase.AssignedInfoEntries;
            var paramBgmStreamProperty = paramBgmDatabase.StreamPropertyEntries;
            var paramBgmPlaylists = paramBgmDatabase.PlaylistEntries.ToDictionary(p => p.Id, p => p.Values);

            //Load MSBT
            var daoMsbtBgms = GetBgmDatabases();
            var daoMsbtTitle = GetGameTitleDatabases();

            var output = new Dictionary<string, BgmEntry>();
            foreach (var daoBinPropertyKeyValue in daoBinPropertyEntries)
            {
                var toneId = daoBinPropertyKeyValue.Key;
                var keyRef = GetToneIdKeyReferences(toneId);

                //Very few songs are currently not in the db - therefore not supported for now.
                if (!paramBgmDbRoot.ContainsKey(keyRef.DbRootKey))
                    continue;

                var dbRootEntry = paramBgmDbRoot[keyRef.DbRootKey];
                var setStreamEntry = paramBgmStreamSet[keyRef.StreamSetKey];
                var assignedInfoEntry = paramBgmAssignedInfo[keyRef.AssignedInfoKey];
                var streamPropertyEntry = paramBgmStreamProperty[keyRef.StreamPropertyKey];
                var gameTitleEntry = paramGameTitleDbRoot[dbRootEntry.UiGameTitleId];
                var bgmProperty = daoBinPropertyKeyValue.Value;

                var newBgmEntry = new BgmEntry()
                {
                    ToneId = toneId,
                    GameTitle = new GameTitleEntry()
                    {
                        GameTitleId = dbRootEntry.UiGameTitleId,
                        NameId = dbRootEntry.NameId,
                        SeriesId = gameTitleEntry.UiSeriesId,
                        Title = new Dictionary<string, string>()
                    },
                    RecordType = dbRootEntry.RecordType,
                    AudioCuePoints = new AudioCuePoints()
                    {
                        LoopEndMs = bgmProperty.LoopEndMs,
                        LoopEndSample = bgmProperty.LoopEndSample,
                        LoopStartMs = bgmProperty.LoopStartMs,
                        LoopStartSample = bgmProperty.LoopStartSample,
                        TotalSamples = bgmProperty.TotalSamples,
                        TotalTimeMs = bgmProperty.TotalTimeMs
                    },
                    SoundTestIndex = dbRootEntry.TestDispOrder,
                    Mod = _toneIdKeyReferences.ContainsKey(toneId) ? _toneIdKeyReferences[toneId].Mod : null,
                    Filename = _toneIdKeyReferences.ContainsKey(toneId) ? _toneIdKeyReferences[toneId].Filename : null,
                    Playlists = paramBgmPlaylists.Where(p => p.Value.Any(p => p.UiBgmId == dbRootEntry.UiBgmId)).Select(p => new PlaylistEntry() { Id = p.Key }).ToList(),
                    IsDlcOrPatch = dbRootEntry.IsDlc,
                    HiddenInSoundTest = ToHiddenInSoundTestStatus(dbRootEntry),
                    SpecialCategory = ToSpecialCategory(setStreamEntry),
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
                foreach (var msbtDb in daoMsbtBgms)
                {
                    var entries = msbtDb.Value.Entries;
                    if (entries.ContainsKey(titleLabel))
                        newBgmEntry.Title.Add(msbtDb.Key, entries[titleLabel]);
                    if (entries.ContainsKey(authorLabel))
                        newBgmEntry.Author.Add(msbtDb.Key, entries[authorLabel]);
                    if (entries.ContainsKey(copyrightLabel))
                        newBgmEntry.Copyright.Add(msbtDb.Key, entries[copyrightLabel]);
                }
                foreach (var msbtDb in daoMsbtTitle)
                {
                    var entries = msbtDb.Value.Entries;
                    if (entries.ContainsKey(gameTitleLabel))
                        newBgmEntry.GameTitle.Title.Add(msbtDb.Key, entries[gameTitleLabel]);
                }

                output.Add(toneId, newBgmEntry);
            }

            return output;
        }

        private void CreateNewBgmEntryInStateManager(BgmToneKeyReferences keyRefs)
        {
            _logger.LogInformation("Adding Bgm Entry to State Service: {ToneId}", keyRefs.ToneId);

            var paramBgmDatabase = _state.LoadResource<PrcUiBgmDatabase>(Constants.GameResources.PRC_UI_BGM_DB_PATH);
            var paramBgmDbRoot = paramBgmDatabase.DbRootEntries;
            var daoBinBgmProperty = _state.LoadResource<BinBgmProperty>(Constants.GameResources.PRC_BGM_PROPERTY_PATH);
            
            var menuValueIndex = paramBgmDbRoot.Values.OrderByDescending(p => p.MenuValue).First().MenuValue + 1;

            //New entry - with default values
            paramBgmDbRoot.Add(keyRefs.DbRootKey, new PrcBgmDbRootEntry()
            {
                UiBgmId = keyRefs.DbRootKey,
                StreamSetId = keyRefs.StreamSetKey,
                Rarity = Constants.InternalIds.RARITY_DEFAULT,
                RecordType = Constants.InternalIds.RECORD_TYPE_DEFAULT,
                UiGameTitleId = Constants.InternalIds.GAME_TITLE_ID_DEFAULT,
                UiGameTitleId1 = Constants.InternalIds.GAME_TITLE_ID_DEFAULT,
                UiGameTitleId2 = Constants.InternalIds.GAME_TITLE_ID_DEFAULT,
                UiGameTitleId3 = Constants.InternalIds.GAME_TITLE_ID_DEFAULT,
                UiGameTitleId4 = Constants.InternalIds.GAME_TITLE_ID_DEFAULT,
                NameId = GetNewBgmId(),
                SaveNo = -1,
                TestDispOrder = -1,
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
            paramBgmDatabase.StreamSetEntries.Add(keyRefs.StreamSetKey, new PrcBgmStreamSetEntry()
            {
                StreamSetId = keyRefs.StreamSetKey,
                Info0 = keyRefs.AssignedInfoKey
            });
            paramBgmDatabase.AssignedInfoEntries.Add(keyRefs.AssignedInfoKey, new PrcBgmAssignedInfoEntry()
            {
                InfoId = keyRefs.AssignedInfoKey,
                StreamId = keyRefs.StreamPropertyKey,
                Condition = Constants.InternalIds.SOUND_CONDITION,
                ConditionProcess = "0x1b9fe75d3f",
                ChangeFadoutFrame = 55,
                MenuChangeFadeOutFrame = 55
            });
            paramBgmDatabase.StreamPropertyEntries.Add(keyRefs.StreamPropertyKey, new PrcBgmStreamPropertyEntry()
            {
                StreamId = keyRefs.StreamPropertyKey,
                DateName0 = keyRefs.ToneId,
                Loop = 1,
                EndPoint = "00:00:15.000",
                FadeOutFrame = 400,
                StartPointTransition = "00:00:04.000"
            });
            daoBinBgmProperty.Entries.Add(keyRefs.ToneId, new BgmPropertyEntry());
        }

        private void UpdateBgmEntryInStateManager(BgmToneKeyReferences keyRefs, BgmEntry bgmEntry)
        {
            _logger.LogInformation("Updating Bgm Entry to State Service: {ToneId}", keyRefs.ToneId);

            var paramBgmDatabase = _state.LoadResource<PrcUiBgmDatabase>(Constants.GameResources.PRC_UI_BGM_DB_PATH);
            var paramGameTitleDatabaseRoot = _state.LoadResource<PrcUiGameTitleDatabase>(Constants.GameResources.PRC_UI_GAMETITLE_DB_PATH).DbRootEntries;
            var binBgmPropertyEntries = _state.LoadResource<BinBgmProperty>(Constants.GameResources.PRC_BGM_PROPERTY_PATH).Entries;

            var defaultLocale = _config.Value.Sm5shMusic.DefaultLocale;
            var coreSeriesGames = paramGameTitleDatabaseRoot.Values.Select(p => p.UiSeriesId).Distinct(); //Not handling series addition right now.

            var toneId = keyRefs.ToneId;

            if (!paramBgmDatabase.DbRootEntries.ContainsKey(keyRefs.DbRootKey))
                throw new Exception($"BGM ID {keyRefs.DbRootKey} does not exist in the DBRoot");

            //BGM PRC
            var dbRootEntry = paramBgmDatabase.DbRootEntries[keyRefs.DbRootKey];
            var setStreamEntry = paramBgmDatabase.StreamSetEntries[keyRefs.StreamSetKey];
            var assignedInfoEntry = paramBgmDatabase.AssignedInfoEntries[keyRefs.AssignedInfoKey];
            var streamPropertyEntry = paramBgmDatabase.StreamPropertyEntries[keyRefs.StreamPropertyKey];
            //DB Root
            dbRootEntry.UiGameTitleId = bgmEntry.GameTitle.GameTitleId;
            dbRootEntry.RecordType = bgmEntry.RecordType;
            dbRootEntry.IsPatch = bgmEntry.IsDlcOrPatch;
            dbRootEntry.IsDlc = bgmEntry.IsDlcOrPatch;
            dbRootEntry.TestDispOrder = bgmEntry.SoundTestIndex;
            dbRootEntry = FromHiddenInSoundTestStatus(paramBgmDatabase.DbRootEntries, dbRootEntry, bgmEntry.HiddenInSoundTest);
            //Stream Set
            setStreamEntry = FromSpecialCategory(setStreamEntry, bgmEntry?.SpecialCategory);

            //GameTitle PRC
            if (!paramGameTitleDatabaseRoot.ContainsKey(bgmEntry.GameTitle.GameTitleId))
            {
                string seriesId = bgmEntry.GameTitle.SeriesId;
                if (!coreSeriesGames.Contains(seriesId))
                    seriesId = Constants.InternalIds.GAME_SERIES_ID_DEFAULT;
                paramGameTitleDatabaseRoot.Add(bgmEntry.GameTitle.GameTitleId, new PrcGameTitleDbRootEntry()
                {
                    NameId = bgmEntry.GameTitle.NameId,
                    Release = paramGameTitleDatabaseRoot.Values.OrderByDescending(p => p.Release).First().Release + 1,
                    UiGameTitleId = bgmEntry.GameTitle.GameTitleId,
                    UiSeriesId = seriesId
                });
            }

            //Bin Property
            binBgmPropertyEntries[toneId].TotalSamples = bgmEntry.AudioCuePoints.TotalSamples;
            binBgmPropertyEntries[toneId].LoopEndMs = bgmEntry.AudioCuePoints.LoopEndMs;
            binBgmPropertyEntries[toneId].LoopEndSample = bgmEntry.AudioCuePoints.LoopEndSample;
            binBgmPropertyEntries[toneId].LoopStartMs = bgmEntry.AudioCuePoints.LoopStartMs;
            binBgmPropertyEntries[toneId].LoopStartSample = bgmEntry.AudioCuePoints.LoopStartSample;
            binBgmPropertyEntries[toneId].TotalTimeMs = bgmEntry.AudioCuePoints.TotalTimeMs;
            binBgmPropertyEntries[toneId].NameId = toneId;

            //Playlists
            if (bgmEntry.Playlists != null)
            {
                //TODO BROKEN FOR SONG UPDATES!!
                foreach (var playlistId in bgmEntry.Playlists)
                {
                    var paramBgmPlaylist = paramBgmDatabase.PlaylistEntries.FirstOrDefault(p => p.Id == playlistId.Id)?.Values;
                    if (paramBgmPlaylist == null)
                    {
                        paramBgmPlaylist = new List<PrcBgmPlaylistEntry>();
                        paramBgmDatabase.PlaylistEntries.Add(new PcrFilterStruct<PrcBgmPlaylistEntry>()
                        {
                            Id = playlistId.Id,
                            Values = paramBgmPlaylist
                        });
                    }

                    var newPlaylistEntry = new PrcBgmPlaylistEntry() { UiBgmId = dbRootEntry.UiBgmId };
                    newPlaylistEntry.SetOrder((short)paramBgmPlaylist.Count);
                    newPlaylistEntry.SetIncidence(500);
                    paramBgmPlaylist.Add(newPlaylistEntry);
                }
            }

            //MSBT
            var nameId = dbRootEntry.NameId;
            var gameTitleEntry = paramGameTitleDatabaseRoot[dbRootEntry.UiGameTitleId];
            var gameTitleId = gameTitleEntry.NameId;
            var gameTitleLabel = string.Format(Constants.InternalIds.MSBT_GAME_TITLE, gameTitleId);
            var titleLabel = string.Format(Constants.InternalIds.MSBT_BGM_TITLE, nameId);
            var authorLabel = string.Format(Constants.InternalIds.MSBT_BGM_AUTHOR, nameId);
            var copyrightLabel = string.Format(Constants.InternalIds.MSBT_BGM_COPYRIGHT, nameId);
            foreach (var msbtDb in GetBgmDatabases())
            {
                var entries = msbtDb.Value.Entries;

                if (bgmEntry.Title != null && bgmEntry.Title.ContainsKey(msbtDb.Key) && !string.IsNullOrEmpty(bgmEntry.Title[msbtDb.Key]))
                    entries[titleLabel] = bgmEntry.Title[msbtDb.Key];
                else if (bgmEntry.Title != null && bgmEntry.Title.ContainsKey(defaultLocale) && !string.IsNullOrEmpty(bgmEntry.Title[msbtDb.Key]))
                    entries[titleLabel] = bgmEntry.Title[defaultLocale];

                if (bgmEntry.Author != null)
                {
                    if (bgmEntry.Author.ContainsKey(msbtDb.Key) && !string.IsNullOrEmpty(bgmEntry.Author[msbtDb.Key]))
                        entries[authorLabel] = bgmEntry.Author[msbtDb.Key];
                    else if (bgmEntry.Author.ContainsKey(defaultLocale) && !string.IsNullOrEmpty(bgmEntry.Author[msbtDb.Key]))
                        entries[authorLabel] = bgmEntry.Author[defaultLocale];
                }

                if (bgmEntry.Copyright != null)
                {
                    if (bgmEntry.Copyright.ContainsKey(msbtDb.Key) && !string.IsNullOrEmpty(bgmEntry.Copyright[msbtDb.Key]))
                        entries[copyrightLabel] = bgmEntry.Copyright[msbtDb.Key];
                    else if (bgmEntry.Copyright.ContainsKey(defaultLocale) && !string.IsNullOrEmpty(bgmEntry.Copyright[msbtDb.Key]))
                        entries[copyrightLabel] = bgmEntry.Copyright[defaultLocale];
                }
            }
            foreach (var msbtDb in GetGameTitleDatabases())
            {
                var entries = msbtDb.Value.Entries;
                if (bgmEntry.GameTitle.Title.ContainsKey(msbtDb.Key))
                    entries[gameTitleLabel] = bgmEntry.GameTitle.Title[msbtDb.Key];
                else if (bgmEntry.GameTitle.Title.ContainsKey(defaultLocale))
                    entries[gameTitleLabel] = bgmEntry.GameTitle.Title[defaultLocale];
            }
        }

        private void RemoveBgmEntryFromStateManager(string toneId)
        {
            var keyRefs = GetToneIdKeyReferences(toneId);

            var binBgmProperty = _state.LoadResource<BinBgmProperty>(Constants.GameResources.PRC_BGM_PROPERTY_PATH);
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
            }
        }

        private SpecialCategoryEntry ToSpecialCategory(PrcBgmStreamSetEntry setStreamEntry)
        {
            var output = new SpecialCategoryEntry()
            {
                Id = setStreamEntry.SpecialCategory,
                Parameters = new List<string>()
            };

            //TODO: Yeah yeah it's not great. Will need to think about that.
            if (!string.IsNullOrEmpty(setStreamEntry.Info1))
                output.Parameters.Add(setStreamEntry.Info1);
            if (!string.IsNullOrEmpty(setStreamEntry.Info2))
                output.Parameters.Add(setStreamEntry.Info2);
            if (!string.IsNullOrEmpty(setStreamEntry.Info3))
                output.Parameters.Add(setStreamEntry.Info3);
            if (!string.IsNullOrEmpty(setStreamEntry.Info4))
                output.Parameters.Add(setStreamEntry.Info4);
            if (!string.IsNullOrEmpty(setStreamEntry.Info5))
                output.Parameters.Add(setStreamEntry.Info5);
            if (!string.IsNullOrEmpty(setStreamEntry.Info6))
                output.Parameters.Add(setStreamEntry.Info6);
            if (!string.IsNullOrEmpty(setStreamEntry.Info7))
                output.Parameters.Add(setStreamEntry.Info7);
            if (!string.IsNullOrEmpty(setStreamEntry.Info8))
                output.Parameters.Add(setStreamEntry.Info8);
            if (!string.IsNullOrEmpty(setStreamEntry.Info9))
                output.Parameters.Add(setStreamEntry.Info9);
            if (!string.IsNullOrEmpty(setStreamEntry.Info10))
                output.Parameters.Add(setStreamEntry.Info10);
            if (!string.IsNullOrEmpty(setStreamEntry.Info11))
                output.Parameters.Add(setStreamEntry.Info11);
            if (!string.IsNullOrEmpty(setStreamEntry.Info12))
                output.Parameters.Add(setStreamEntry.Info12);
            if (!string.IsNullOrEmpty(setStreamEntry.Info13))
                output.Parameters.Add(setStreamEntry.Info13);
            if (!string.IsNullOrEmpty(setStreamEntry.Info14))
                output.Parameters.Add(setStreamEntry.Info14);
            if (!string.IsNullOrEmpty(setStreamEntry.Info15))
                output.Parameters.Add(setStreamEntry.Info15);

            return output;
        }

        private PrcBgmStreamSetEntry FromSpecialCategory(PrcBgmStreamSetEntry setStreamEntry, SpecialCategoryEntry specialEntry)
        {
            if (specialEntry == null)
                return setStreamEntry;

            setStreamEntry.SpecialCategory = specialEntry.Id;

            if (specialEntry.Parameters == null)
                return setStreamEntry;

            //TODO: Yeah yeah it's not great. Will need to think about that.
            if (specialEntry.Parameters.Count > 0)
                setStreamEntry.Info1 = specialEntry.Parameters[0];
            if (specialEntry.Parameters.Count > 1)
                setStreamEntry.Info2 = specialEntry.Parameters[1];
            if (specialEntry.Parameters.Count > 2)
                setStreamEntry.Info3 = specialEntry.Parameters[2];
            if (specialEntry.Parameters.Count > 3)
                setStreamEntry.Info4 = specialEntry.Parameters[3];
            if (specialEntry.Parameters.Count > 4)
                setStreamEntry.Info5 = specialEntry.Parameters[4];
            if (specialEntry.Parameters.Count > 5)
                setStreamEntry.Info6 = specialEntry.Parameters[5];
            if (specialEntry.Parameters.Count > 6)
                setStreamEntry.Info7 = specialEntry.Parameters[6];
            if (specialEntry.Parameters.Count > 7)
                setStreamEntry.Info8 = specialEntry.Parameters[7];
            if (specialEntry.Parameters.Count > 8)
                setStreamEntry.Info9 = specialEntry.Parameters[8];
            if (specialEntry.Parameters.Count > 9)
                setStreamEntry.Info10 = specialEntry.Parameters[9];
            if (specialEntry.Parameters.Count > 10)
                setStreamEntry.Info11 = specialEntry.Parameters[10];
            if (specialEntry.Parameters.Count > 11)
                setStreamEntry.Info12 = specialEntry.Parameters[11];
            if (specialEntry.Parameters.Count > 12)
                setStreamEntry.Info13 = specialEntry.Parameters[12];
            if (specialEntry.Parameters.Count > 13)
                setStreamEntry.Info14 = specialEntry.Parameters[13];
            if (specialEntry.Parameters.Count > 14)
                setStreamEntry.Info15 = specialEntry.Parameters[14];

            return setStreamEntry;
        }

        private bool ToHiddenInSoundTestStatus(PrcBgmDbRootEntry dbRootEntry)
        {
            return dbRootEntry.SaveNo == -1 && dbRootEntry.TestDispOrder == -1;
        }

        private PrcBgmDbRootEntry FromHiddenInSoundTestStatus(Dictionary<string, PrcBgmDbRootEntry> paramBgmDbRoot, PrcBgmDbRootEntry dbRootEntry, bool isHiddenInSoundTest)
        {
            if(isHiddenInSoundTest)
            {
                dbRootEntry.TestDispOrder = -1;
                dbRootEntry.SaveNo = -1;
            }
            else
            {
                if(dbRootEntry.TestDispOrder == -1)
                {
                    var testDispOrderIndex = (short)(paramBgmDbRoot.Values.OrderByDescending(p => p.TestDispOrder).First().TestDispOrder + 1);
                    dbRootEntry.TestDispOrder = testDispOrderIndex;
                }
                if(dbRootEntry.SaveNo == -1)
                {
                    //var saveNoIndex = (short)(_daoUiBgmDbRootEntries.Values.OrderByDescending(p => p.SaveNo).First().SaveNo + 1); //Not working past top save_no id
                    dbRootEntry.SaveNo = 0;
                }
            }
            return dbRootEntry;
        }

        #region Utils
        private string GetNewBgmId()
        {
            var paramBgmDatabase = _state.LoadResource<PrcUiBgmDatabase>(Constants.GameResources.PRC_UI_BGM_DB_PATH);
            var lastNameId = paramBgmDatabase.DbRootEntries.Values.Where(p => p.NameId != "random" && !string.IsNullOrEmpty(p.NameId)).OrderByDescending(p => Base36IncrementHelper.ToInt(p.NameId)).FirstOrDefault()?.NameId;
            lastNameId = Base36IncrementHelper.ToString(Base36IncrementHelper.ToInt(lastNameId) + 1);
            if (lastNameId == "random")
                return GetNewBgmId();
            return lastNameId;
        }

        private BgmToneKeyReferences GetToneIdKeyReferences(string toneId, BgmEntry bgmEntry = null)
        {
            if (_toneIdKeyReferences.ContainsKey(toneId))
                return _toneIdKeyReferences[toneId];
            var output = new BgmToneKeyReferences(toneId, bgmEntry);
            _toneIdKeyReferences.Add(toneId, output);
            return output;
        }

        private Dictionary<string, MsbtDatabase> GetBgmDatabases()
        {
            var output = new Dictionary<string, MsbtDatabase>();
            foreach (var locale in LocaleHelper.ValidLocales)
            {
                var msbt = _state.LoadResource<MsbtDatabase>(string.Format(Constants.GameResources.MSBT_BGM, locale), true);
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
                var msbt = _state.LoadResource<MsbtDatabase>(string.Format(Constants.GameResources.MSBT_TITLE, locale), true);
                if (msbt != null)
                    output.Add(locale, msbt);
            }
            return output;
        }
        #endregion
        #endregion
    }

    namespace AudioStateServiceModels
    {
        public enum BgmDbOperation
        {
            Added,
            Removed
        }

        public class BgmToneKeyReferences
        {
            public string ToneId { get; }
            public ModEntry Mod { get; set; }
            public string Filename { get; set; }
            public EntrySource Source { get; set; }
            public string DbRootKey { get; }
            public string StreamSetKey { get; }
            public string AssignedInfoKey { get; }
            public string StreamPropertyKey { get; }

            public BgmToneKeyReferences(string toneId, BgmEntry bgmEntry = null)
            {
                ToneId = toneId;
                DbRootKey = $"{Constants.InternalIds.UI_BGM_ID_PREFIX}{toneId}";
                StreamSetKey = $"{Constants.InternalIds.STREAM_SET_PREFIX}{toneId}";
                AssignedInfoKey = $"{Constants.InternalIds.INFO_ID_PREFIX}{toneId}";
                StreamPropertyKey = $"{Constants.InternalIds.STREAM_PREFIX}{toneId}";
                if (bgmEntry != null)
                {
                    Mod = bgmEntry.Mod;
                    Filename = bgmEntry.Filename;
                    Source = bgmEntry.Source;
                }
                else
                {
                    Source = EntrySource.Core;
                }
            }
        }
    }
}
