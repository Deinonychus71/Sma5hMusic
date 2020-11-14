using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sm5sh.Mods.Music.BgmEntryModels;
using Sm5sh.Services.Interfaces;
using Sm5sh.Shared.Data.Sound.Config;
using Sm5sh.Shared.Data.Ui.Param.Database;
using System.Collections.Generic;
using System.Linq;

namespace Sm5sh.Mods.Music
{
    public class BgmService : IBgmService
    {
        private readonly ILogger _logger;
        private readonly IPrcService _prcService;
        private readonly IYmlService _ymlService;
        private readonly IOptions<Options> _config;
        

        private const string UI_BGM_ID_PREFIX = "ui_bgm_";
        private const string STREAM_SET_PREFIX = "set_";
        private const string INFO_ID_PREFIX = "info_";
        private const string STREAM_PREFIX = "stream_";

        private readonly Dictionary<string, BgmEntry> _bgmEntries;
        private readonly PrcUiBgmDatabase _daoUiBgmDatabase;
        private readonly BinBgmProperty _daoBgmProperty;

        public Dictionary<string, BgmEntry> BgmEntries { get { return _bgmEntries; } }

        public BgmService(IOptions<Options> config, IPrcService prcService, IYmlService ymlService, ILogger<IBgmService> logger)
        {
            _logger = logger;
            _prcService = prcService;
            _ymlService = ymlService;
            _config = config;

            //Initialize core files
            _daoUiBgmDatabase = prcService.ReadPrcFile<PrcUiBgmDatabase>(_config.Value.PrcBgmDatabaseFile);
            _daoBgmProperty = ymlService.ReadYmlFile<BinBgmProperty>(_config.Value.BinBgmPropertyFile);
            _bgmEntries = InitializeCoreBgmEntries();

            //Initialize mods
            //TODO
        }

        public bool Save()
        {
            return true;
        }

        private Dictionary<string, BgmEntry> InitializeCoreBgmEntries()
        {
            var uiBgmDbRoot = _daoUiBgmDatabase.DbRootEntries.ToDictionary(p => p.UiBgmId.StringValue, p => p);
            var uiBgmStreamSet = _daoUiBgmDatabase.StreamSetEntries.ToDictionary(p => p.StreamSetId.StringValue, p => p);
            var uiBgmAssignedInfo = _daoUiBgmDatabase.AssignedInfoEntries.ToDictionary(p => p.InfoId.StringValue, p => p);
            var uiBgmStreamProperty = _daoUiBgmDatabase.StreamPropertyEntries.ToDictionary(p => p.DateName0, p => p);
            var binBgmProperty = _daoBgmProperty.Entries.ToDictionary(p => p.NameId, p => p);

            var output = new Dictionary<string, BgmEntry>();

            foreach(var uiBgmStreamPropertyEntry in uiBgmStreamProperty)
            {
                var toneId = uiBgmStreamPropertyEntry.Key;
                var dbRootEntry = uiBgmDbRoot[$"{UI_BGM_ID_PREFIX}{toneId}"];
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

        public class Options
        {
            public string PrcBgmDatabaseFile { get; set; }
            public string BinBgmPropertyFile { get; set; }

        }
    }
}
