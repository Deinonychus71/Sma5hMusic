using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sm5sh.Core.Helpers;
using Sm5sh.Data.Ui.Param.Database;
using Sm5sh.Data.Ui.Param.Database.PrcUiStageDatabasModels;
using Sm5sh.Interfaces;
using Sm5sh.Mods.StagePlaylist.Helpers;
using Sm5sh.Mods.StagePlaylist.Models;
using Sm5sh.ResourceProviders.Prc.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sm5sh.Mods.StagePlaylist
{
    public class StagePlaylistMod : BaseSm5shMod
    {
        private readonly ILogger _logger;
        private readonly IOptions<Sm5shStagePlaylistOptions> _config;
        private Dictionary<string, StagePlaylistModConfig> _modConfig;

        public override string ModName => "Sm5shStagePlaylistMod";

        public StagePlaylistMod(IOptions<Sm5shStagePlaylistOptions> config, IStateManager state, ILogger<StagePlaylistMod> logger)
            : base(state)
        {
            _logger = logger;
            _state = state;
            _config = config;
        }

        public override bool Init()
        {
            //Load Music Mods
            _logger.LogInformation("Stage Playlist Mod Path: {StagePlaylistModPath}", _config.Value.Sm5shStagePlaylist.ModFile);

            var stageModJsonFile = _config.Value.Sm5shStagePlaylist.ModFile;
            var generateStageMod = File.Exists(stageModJsonFile);
            if (!generateStageMod)
            {
                _logger.LogDebug("The file {StageModJsonFile} could not be found. The stage playlists will not be updated.", stageModJsonFile);
                return false;
            }

            var modConfig = JsonConvert.DeserializeObject<List<StagePlaylistModConfig>>(File.ReadAllText(stageModJsonFile));

            foreach (var stagePlaylist in modConfig)
            {
                //Sanitize StageId
                stagePlaylist.StageId = stagePlaylist.StageId.ToLower();
                if (!stagePlaylist.StageId.StartsWith(Constants.InternalIds.STAGE_ID_PREFIX))
                    stagePlaylist.StageId = $"{Constants.InternalIds.STAGE_ID_PREFIX}{stagePlaylist.StageId}";

                //Sanitize Playlist
                stagePlaylist.PlaylistId = stagePlaylist.PlaylistId.ToLower();
                if (!stagePlaylist.PlaylistId.StartsWith(Constants.InternalIds.PLAYLIST_PREFIX))
                    stagePlaylist.PlaylistId = $"{Constants.InternalIds.PLAYLIST_PREFIX}{stagePlaylist.PlaylistId}";

                if (!IdValidationHelper.IsLegalId(stagePlaylist.PlaylistId))
                {
                    _logger.LogError("{StagePlaylistMod} will be disabled. At least one playlist id contains illegal characters.", stageModJsonFile);
                    return false;
                }

                if (!CoreConstants.VALID_STAGES.Contains(stagePlaylist.StageId))
                {
                    _logger.LogError("{StagePlaylistMod} will be disabled. At least one stage is not registered in the Stage DB.", stageModJsonFile);
                    return false;
                }

                if (stagePlaylist.OrderId < 0 || stagePlaylist.OrderId > 15)
                {
                    _logger.LogError("{StagePlaylistMod} will be disabled. At least one stage has an invalid order id. The value is 0 to 15.", stageModJsonFile);
                    return false;
                }
            }

            _modConfig = modConfig.ToDictionary(p => p.StageId, p => p);

            _logger.LogInformation("{StagePlaylistModPath}: Stage Playlist Mod loaded. {NbrPlaylistEntries} entries.", _config.Value.Sm5shStagePlaylist.ModFile, _modConfig.Count);

            return true;
        }

        public override bool SaveChanges()
        {
            var daoUiStageDatabase = _state.LoadResource<PrcUiStageDatabase>(Constants.GameResources.PRC_UI_STAGE_DB_PATH);

            foreach(var daoUiStageEntry in daoUiStageDatabase.DbRootEntries.Values)
            {
                var modEntry = _modConfig[daoUiStageEntry.UiStageId.StringValue];

                daoUiStageEntry.BgmSetId = new PrcHash40(modEntry.PlaylistId);
                daoUiStageEntry.BgmSettingNo = modEntry.OrderId;
            }

            return true;
        }
    }
}
