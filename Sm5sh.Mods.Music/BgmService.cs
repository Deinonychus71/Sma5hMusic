using Microsoft.Extensions.Logging;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Models;
using Sm5sh.Mods.Music.Models.BgmEntryModels;
using Sm5sh.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Sm5sh.Mods.Music
{
    public class BgmService : IBgmService, ISm5shMod
    {
        private readonly ILogger _logger;
        private readonly IStateService _stateService;

        private const string UI_BGM_ID_PREFIX = "ui_bgm_";
        private const string STREAM_SET_PREFIX = "set_";
        private const string INFO_ID_PREFIX = "info_";

        private Dictionary<string, BgmEntry> _bgmEntries;
        private Dictionary<string, BgmServiceModels.BgmDbOperation> _dbOperations;

        public BgmService(IStateService stateService, ILogger<IBgmService> logger)
        {
            _logger = logger;
            _stateService = stateService;
            _dbOperations = new Dictionary<string, BgmServiceModels.BgmDbOperation>();

            //Initialize core files
            _bgmEntries = InitializeCoreBgmEntries();

            //Initialize mods
            //TODO
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
            if (!_stateService.UiBgmDatabase.StreamPropertyEntries.Any(p => p.DateName0 == toneId))
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

        public bool SaveChangesToStateService()
        {
            _logger.LogInformation("Saving Bgm Entries to State Service");

            //Remove
            foreach(var toneIdToRemove in _dbOperations.Where(p => p.Value == BgmServiceModels.BgmDbOperation.Removed).Select(p => p.Key))
            {
                //Remove from DBs
                _stateService.BgmProperty.Entries.RemoveAll(p => p.NameId == toneIdToRemove);
                _stateService.UiBgmDatabase.DbRootEntries.RemoveAll(p => p.UiBgmId.StringValue == $"{UI_BGM_ID_PREFIX}{toneIdToRemove}");
                _stateService.UiBgmDatabase.StreamSetEntries.RemoveAll(p => p.StreamSetId.StringValue == $"{STREAM_SET_PREFIX}{toneIdToRemove}");
                _stateService.UiBgmDatabase.AssignedInfoEntries.RemoveAll(p => p.InfoId.StringValue == $"{INFO_ID_PREFIX}{toneIdToRemove}");
                _stateService.UiBgmDatabase.StreamPropertyEntries.RemoveAll(p => p.DateName0 == toneIdToRemove);

                //Remove from playlists
                foreach(var playlist in _stateService.UiBgmDatabase.PlaylistEntries)
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
            var uiBgmDbRoot = _stateService.UiBgmDatabase.DbRootEntries.ToDictionary(p => p.UiBgmId.StringValue, p => p);
            var uiBgmStreamSet = _stateService.UiBgmDatabase.StreamSetEntries.ToDictionary(p => p.StreamSetId.StringValue, p => p);
            var uiBgmAssignedInfo = _stateService.UiBgmDatabase.AssignedInfoEntries.ToDictionary(p => p.InfoId.StringValue, p => p);
            var uiBgmStreamProperty = _stateService.UiBgmDatabase.StreamPropertyEntries.ToDictionary(p => p.DateName0, p => p);
            var binBgmProperty = _stateService.BgmProperty.Entries.ToDictionary(p => p.NameId, p => p);

            var output = new Dictionary<string, BgmEntry>();

            foreach(var uiBgmStreamPropertyEntry in uiBgmStreamProperty)
            {
                var toneId = uiBgmStreamPropertyEntry.Key;
                var dbRootEntry = uiBgmDbRoot[$"{UI_BGM_ID_PREFIX}{toneId}"];
                var setStreamEntry = uiBgmDbRoot[$"{STREAM_SET_PREFIX}{toneId}"];
                var assignedInfoEntry = uiBgmDbRoot[$"{INFO_ID_PREFIX}{toneId}"];
                var streamPropertyEntry = uiBgmDbRoot[toneId];
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
                    }
                };

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
