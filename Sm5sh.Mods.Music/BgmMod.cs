using Microsoft.Extensions.Logging;
using Sm5sh.Data;
using Sm5sh.Data.Ui.Param.Database;
using Sm5sh.Helpers;
using Sm5sh.Interfaces;
using Sm5sh.Mods.Music.Data.Sound.Config;
using Sm5sh.Mods.Music.Models;
using Sm5sh.Mods.Music.Models.BgmEntryModels;
using System.Collections.Generic;
using System.Linq;

namespace Sm5sh.Mods.Music
{
    public class BgmMod : BaseSm5shMod, ISm5shMod
    {
        private readonly ILogger _logger;
        private readonly IStateManager _state;

        private const string UI_BGM_ID_PREFIX = "ui_bgm_";
        private const string STREAM_SET_PREFIX = "set_";
        private const string INFO_ID_PREFIX = "info_";
        private const string STREAM_PREFIX = "stream_";
        private const string MSBT_BGM_TITLE = "bgm_title_{0}";
        private const string MSBT_BGM_AUTHOR = "bgm_author_{0}";
        private const string MSBT_BGM_COPYRIGHT = "bgm_copyright_{0}";

        private PrcUiBgmDatabase _daoUiBgmDatabase;
        private PrcUiGameTitleDatabase _daoUiGameTitleDatabase;
        private Dictionary<string, MsbtDatabase> _daoMsbtBgms;
        private Dictionary<string, MsbtDatabase> _daoMsbtTitle;
        private BinBgmProperty _daoBinBgmProperty;
        private Dictionary<string, BgmEntry> _bgmEntries;
        private Dictionary<string, BgmServiceModels.BgmDbOperation> _dbOperations;

        public BgmMod(IStateManager state, ILogger<BgmMod> logger)
        {
            _logger = logger;
            _state = state;
            _dbOperations = new Dictionary<string, BgmServiceModels.BgmDbOperation>();
            _daoMsbtBgms = new Dictionary<string, MsbtDatabase>();
            _daoMsbtTitle = new Dictionary<string, MsbtDatabase>();

            //Initialize core files
            _daoUiBgmDatabase = _state.LoadResource<PrcUiBgmDatabase>("ui/param/database/ui_bgm_db.prc");
            _daoUiGameTitleDatabase = _state.LoadResource<PrcUiGameTitleDatabase>("ui/param/database/ui_gametitle_db.prc");
            _daoBinBgmProperty = _state.LoadResource<BinBgmProperty>("sound/config/bgm_property.bin");
            foreach(var locale in LocaleHelper.ValidLocales)
            {
                var msbtBgm = _state.LoadResource<MsbtDatabase>($"ui/message/msg_bgm+{locale}.msbt", true);
                if(msbtBgm != null)
                    _daoMsbtBgms.Add(locale, msbtBgm);

                var msbtTitle = _state.LoadResource<MsbtDatabase>($"ui/message/msg_title+{locale}.msbt", true);
                if (msbtBgm != null)
                    _daoMsbtTitle.Add(locale, msbtTitle);
            }
            _bgmEntries = InitializeCoreBgmEntries();

            //TODO Init mods
        }

        public IEnumerable<BgmEntry> GetBgmEntries()
        {
            return _bgmEntries.Values;
        }

        public BgmEntry AddBgmEntry(string toneId)
        {
            if (_bgmEntries.ContainsKey(toneId))
            {
                _logger.LogError("The ToneId {ToneId} already exists in the Bgm Entries", toneId);
                return null;
            }

            var newBgmEntry = new BgmEntry() { ToneId = toneId, Source = BgmAudioSource.Mod };
            _bgmEntries.Add(toneId, newBgmEntry);

            //Delete remove tag
            if (_dbOperations.ContainsKey(toneId))
                _dbOperations.Remove(toneId);

            //If already exists in Core game, do not tag added
            if (!_daoUiBgmDatabase.StreamPropertyEntries.Any(p => p.DateName0 == toneId))
                _dbOperations[toneId] = BgmServiceModels.BgmDbOperation.Added;

            return newBgmEntry;
        }

        public bool RemoveBgmEntry(string toneId)
        {
            if (_bgmEntries.ContainsKey(toneId))
            {
                _bgmEntries.Remove(toneId);
                _dbOperations[toneId] = BgmServiceModels.BgmDbOperation.Removed;
            }
            return true;
        }

        public override bool SaveChanges()
        {
            _logger.LogInformation("Saving Bgm Entries to State Service");

            //Remove
            foreach(var toneIdToRemove in _dbOperations.Where(p => p.Value == BgmServiceModels.BgmDbOperation.Removed).Select(p => p.Key))
            {
                //Remove from DBs
                _daoBinBgmProperty.Entries.RemoveAll(p => p.NameId == toneIdToRemove);
                _daoUiBgmDatabase.DbRootEntries.RemoveAll(p => p.UiBgmId.StringValue == $"{UI_BGM_ID_PREFIX}{toneIdToRemove}");
                _daoUiBgmDatabase.StreamSetEntries.RemoveAll(p => p.StreamSetId.StringValue == $"{STREAM_SET_PREFIX}{toneIdToRemove}");
                _daoUiBgmDatabase.AssignedInfoEntries.RemoveAll(p => p.InfoId.StringValue == $"{INFO_ID_PREFIX}{toneIdToRemove}");
                _daoUiBgmDatabase.StreamPropertyEntries.RemoveAll(p => p.DateName0 == toneIdToRemove);

                //Remove from playlists
                foreach(var playlist in _daoUiBgmDatabase.PlaylistEntries)
                {
                    playlist.Values.RemoveAll(p => p.UiBgmId.StringValue == $"{UI_BGM_ID_PREFIX}{toneIdToRemove}");
                }

                //TODO: Remove from MSBT (for cleanup)
            }

            //Add Update
            foreach (var bgmEntry in _bgmEntries)
            {
                
            }

            return true;
        }

        private Dictionary<string, BgmEntry> InitializeCoreBgmEntries()
        {
            var uiBgmDbRoot = _daoUiBgmDatabase.DbRootEntries.ToDictionary(p => p.UiBgmId.StringValue.Replace(UI_BGM_ID_PREFIX, string.Empty), p => p);
            var uiBgmStreamSet = _daoUiBgmDatabase.StreamSetEntries.ToDictionary(p => p.StreamSetId.StringValue.Replace(STREAM_SET_PREFIX, string.Empty), p => p);
            var uiBgmAssignedInfo = _daoUiBgmDatabase.AssignedInfoEntries.ToDictionary(p => p.InfoId.StringValue.Replace(INFO_ID_PREFIX, string.Empty), p => p);
            var uiBgmStreamProperty = _daoUiBgmDatabase.StreamPropertyEntries.ToDictionary(p => p.StreamId.StringValue.Replace(STREAM_PREFIX, string.Empty), p => p);
            var binBgmProperty = _daoBinBgmProperty.Entries.ToDictionary(p => p.NameId, p => p);

            var output = new Dictionary<string, BgmEntry>();

            foreach (var dbRootEntryKeyValue in uiBgmDbRoot)
            {
                var toneId = dbRootEntryKeyValue.Key;

                //For now, we're only treating songs that have all the data we need
                if (!uiBgmStreamSet.ContainsKey(toneId) || !uiBgmAssignedInfo.ContainsKey(toneId) ||
                   !uiBgmStreamProperty.ContainsKey(toneId) || !binBgmProperty.ContainsKey(toneId))
                    continue;

                var dbRootEntry = dbRootEntryKeyValue.Value;
                var setStreamEntry = uiBgmStreamSet[toneId];
                var assignedInfoEntry = uiBgmAssignedInfo[toneId];
                var streamPropertyEntry = uiBgmStreamProperty[toneId]; 
                var bgmProperty = binBgmProperty[toneId];

                var newBgmEntry = new BgmEntry()
                {
                    ToneId = toneId,
                    Source = BgmAudioSource.CoreGame,
                    GameTitleId = dbRootEntry.UiGameTitleId.StringValue,
                    RecordType = dbRootEntry.RecordType.StringValue,
                    Rarity = dbRootEntry.Rarity.StringValue,
                    AudioCuePoints =new AudioCuePoints()
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
                var titleLabel = string.Format(MSBT_BGM_TITLE, nameId);
                var authorLabel = string.Format(MSBT_BGM_AUTHOR, nameId);
                var copyrightLabel = string.Format(MSBT_BGM_COPYRIGHT, nameId);
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
    }

    namespace BgmServiceModels
    {
        public enum BgmDbOperation
        {
            Added,
            Removed
        }
    }
}
